using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public PlayerIdentity identity = null;

    public bool isInitialized = false;

    [SyncVar] public float networkGameTime = 0f;

    [SyncVar] public ulong networkSteamID = 0L;

    public PlayerAppearance playerAppearance = null;

    public Transform playerCenter = null;

    public Collider playerCollider = null;

    public PlayerHud playerHud = null;

    public PlayerMove playerMove = null;

    public PlayerPlantBomb playerPlantBomb = null;

    public Transform playerTop = null;

    public PlayerProperty prop = null;

    public ParticleSystem restoreShieldEffect = null;

    public PlayerStatistics stat = null;

    private int hashFinalColor = Shader.PropertyToID("_FinalColor");


    private IEnumerator Start()
    {
        yield return Initialize();
    }


    private void Update()
    {
        CheckNull();
        HandleBombInfoListOnServerOwner();
        RefreshGameTimeOnServer();
    }


    private void CheckNull()
    {
        if (!isInitialized)
        {
            return;
        }

        if (identity == null)
        {
            Destroy(gameObject);
        }
    }


    private IEnumerator Initialize()
    {
        while (!isInitialized)
        {
            yield return null;
            foreach (PlayerIdentity playerIdentity in PlayerIdentity.InstanceList)
            {
                if (playerIdentity.networkSteamID == networkSteamID)
                {
                    Debug.LogError($"玩家[{playerIdentity.networkSteamName}]已生成。");
                    if (hasAuthority)
                    {
                        CameraController.Instance.SetTarget(transform);
                        PlayerMove.IsEnabled = true;
                        PlayerPlantBomb.IsEnabled = true;
                    }

                    playerIdentity.player = this;
                    identity = playerIdentity;
                    isInitialized = true;
                    playerAppearance.Initialize();
                    playerHud = PlayerHudManager.Instance.GetPlayerHud(this);
                    playerMove.Initialize(this);
                    playerPlantBomb.Initialize(this);
                    prop.Initialize(this);
                    if (isServer)
                    {
                        prop.networkUnitName = identity.networkSteamName;
                    }

                    stat.Initialize(this);
                    PopupManager.Instance.SetAlivePlayerCount(PlayerIdentity.GetAlivePlayerCount());
                    break;
                }
            }
        }
    }


    [ServerCallback]
    private void RefreshGameTimeOnServer()
    {
        if (!GameManager.InGame)
        {
            return;
        }

        networkGameTime += Time.deltaTime;
    }


    #region RPC

    [ClientRpc]
    private void GenerateMapClientRPC(string json)
    {
        MapManager.Instance.GenerateOnClient(json);
    }


    [Command(requiresAuthority = false)]
    public void GenerateMapServerRPC()
    {
        string json = MapManager.Instance.GenerateOnServer(GameManager.MapSize.x, GameManager.MapSize.y);
        GenerateMapClientRPC(json);
    }


    [ClientRpc]
    private void HandleBombInfoListClientRPC(Vector2Int[] coordinates)
    {
        AudioManager.Instance.Play("炸弹爆炸", null, MapManager.Instance.GetPositionByCoordinate(coordinates[0]));
        foreach (Vector2Int coordinate in coordinates)
        {
            PrefabManager.PrefabMap["Bomb Explosion Effect"].pool.Get(out GameObject element);
            element.transform.position = MapManager.Instance.GetPositionByCoordinate(coordinate);
            MapManager.Instance.DestroyBlock(coordinate);
        }
    }


    [ClientRpc]
    private void HandlePlayerStatisticsClientRPC(string json)
    {
        PopupManager.Instance.DrawStatisticsPanel(JsonConvert.DeserializeObject<List<PlayerStatistics.Data>>(json));
    }


    [ClientRpc]
    public void PlayAudioClientRPC(string name, Vector3 position)
    {
        AudioManager.Instance.Play(name, null, position);
    }


    [ClientRpc]
    public void PlayAudioClientRPCLocalPlayerOnly(string name, Vector3 position)
    {
        if (!hasAuthority)
        {
            return;
        }

        AudioManager.Instance.Play(name, null, position);
    }


    [ClientRpc]
    public void PlayRestoreShieldEffectClientRPC(int shieldLevel)
    {
        restoreShieldEffect.gameObject.SetActive(true);
        restoreShieldEffect.Stop();
        restoreShieldEffect.GetComponent<Renderer>().material
            .SetColor(hashFinalColor, LootManager.Instance.colorQuality[shieldLevel - 2]);
        restoreShieldEffect.Play();
        if (!hasAuthority)
        {
            return;
        }

        AudioManager.Instance.Play("护甲恢复", AudioManager.Instance.audioListener.transform, Vector3.zero, -1f,
            (ulong)(44100f * 0.2f));
    }

    #endregion RPC


    #region METHOD ON SERVER OWNER

    [ServerCallback]
    private void HandleBombInfoListOnServerOwner()
    {
        if (!hasAuthority)
        {
            return;
        }

        if (Bomb.InfoList.Count == 0)
        {
            return;
        }

        List<Unit> units = Unit.InstanceList.FindAll(unit => unit is PlayerProperty);
        foreach (Unit unit in units)
        {
            PlayerProperty prop = unit as PlayerProperty;
            Bomb.Info info = Bomb.InfoList.Find(info => info.coordinate == prop.player.playerMove.networkCoordinate);
            if (info != null)
            {
                prop.TakeDamageOnServer(info.damageSource.prop, Bomb.DAMAGE, Unit.DamageType.Default);
            }
        }

        Vector2Int[] coordinates = new Vector2Int[Bomb.InfoList.Count];
        for (int a = 0; a < coordinates.Length; ++a)
        {
            coordinates[a] = Bomb.InfoList[a].coordinate;
            if (MapManager.Instance.GetCell(coordinates[a]) == MapManager.Type.BlockDestructible)
            {
                Block block = MapManager.Instance.GetBlock(coordinates[a]);
                LootEntry entry = LootManager.Instance.GetRandomInstance(block.lootID);
                if (entry != null)
                {
                    entry.transform.position = MapManager.Instance.GetPositionByCoordinate(coordinates[a]);
                    NetworkServer.Spawn(entry.gameObject);
                }
            }
        }

        HandleBombInfoListClientRPC(coordinates);
        Bomb.InfoList.Clear();
    }


    [ServerCallback]
    public void HandlePlayerStatisticsOnServerOwner()
    {
        List<PlayerStatistics.Data> dataList = new List<PlayerStatistics.Data>();
        foreach (PlayerIdentity identity in PlayerIdentity.InstanceList)
        {
            if (identity == null || identity.player == null)
            {
                continue;
            }

            dataList.Add(identity.player.stat.GetData());
        }

        PlayerStatistics.Data highestData = new PlayerStatistics.Data();
        PlayerStatistics.Data lowestData = new PlayerStatistics.Data
        {
            dealDamage = new PlayerStatistics.FloatValue { value = float.MaxValue },
            killCount = new PlayerStatistics.IntValue { value = int.MaxValue },
            rank = new PlayerStatistics.IntValue { value = int.MaxValue },
            dealHealing = new PlayerStatistics.FloatValue { value = float.MaxValue }
        };
        foreach (PlayerStatistics.Data data in dataList)
        {
            if (data.dealDamage.value > highestData.dealDamage.value)
            {
                highestData.dealDamage.value = data.dealDamage.value;
            }

            if (data.dealDamage.value < lowestData.dealDamage.value)
            {
                lowestData.dealDamage.value = data.dealDamage.value;
            }

            if (data.killCount.value > highestData.killCount.value)
            {
                highestData.killCount.value = data.killCount.value;
            }

            if (data.killCount.value < lowestData.killCount.value)
            {
                lowestData.killCount.value = data.killCount.value;
            }

            if (data.rank.value > highestData.rank.value)
            {
                highestData.rank.value = data.rank.value;
            }

            if (data.rank.value < lowestData.rank.value)
            {
                lowestData.rank.value = data.rank.value;
            }

            if (data.dealHealing.value > highestData.dealHealing.value)
            {
                highestData.dealHealing.value = data.dealHealing.value;
            }

            if (data.dealHealing.value < lowestData.dealHealing.value)
            {
                lowestData.dealHealing.value = data.dealHealing.value;
            }
        }

        foreach (PlayerStatistics.Data data in dataList)
        {
            if (data.dealDamage.value == highestData.dealDamage.value)
            {
                data.dealDamage.isHighest = true;
            }

            if (data.dealDamage.value == lowestData.dealDamage.value)
            {
                data.dealDamage.isLowest = true;
            }

            if (data.killCount.value == highestData.killCount.value)
            {
                data.killCount.isHighest = true;
            }

            if (data.killCount.value == lowestData.killCount.value)
            {
                data.killCount.isLowest = true;
            }

            if (data.rank.value == highestData.rank.value)
            {
                data.rank.isHighest = true;
            }

            if (data.rank.value == lowestData.rank.value)
            {
                data.rank.isLowest = true;
            }

            if (data.dealHealing.value == highestData.dealHealing.value)
            {
                data.dealHealing.isHighest = true;
            }

            if (data.dealHealing.value == lowestData.dealHealing.value)
            {
                data.dealHealing.isLowest = true;
            }
        }

        HandlePlayerStatisticsClientRPC(
            JsonConvert.SerializeObject(dataList.OrderBy(data => data.rank.value).ToList(), Formatting.Indented));
    }

    #endregion METHOD ON SERVER OWNER
}