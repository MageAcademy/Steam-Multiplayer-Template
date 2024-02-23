using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public PlayerIdentity identity = null;

    public bool isInitialized = false;

    [SyncVar] public ulong networkSteamID = 0L;

    public PlayerAppearance playerAppearance = null;

    public Transform playerCenter = null;

    public PlayerHud playerHud = null;

    public PlayerMove playerMove = null;

    public PlayerPlantBomb playerPlantBomb = null;

    public Transform playerTop = null;

    public PlayerProperty prop = null;


    private IEnumerator Start()
    {
        yield return Initialize();
    }


    private void Update()
    {
        CheckNull();
        HandleBombInfoListOnServerOwner();
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
                    }

                    playerIdentity.player = this;
                    identity = playerIdentity;
                    isInitialized = true;
                    playerAppearance.Initialize();
                    playerHud = PlayerHudManager.Instance.GetPlayerHud(this);
                    playerMove.Initialize(this);
                    playerPlantBomb.Initialize(this);
                    prop.Initialize(this);
                    break;
                }
            }
        }
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
        string json = MapManager.Instance.GenerateOnServer(20, 20);
        GenerateMapClientRPC(json);
    }


    [ClientRpc]
    private void HandleBombInfoListClientRPC(Vector2Int[] coordinates)
    {
        foreach (Vector2Int coordinate in coordinates)
        {
            PrefabManager.PrefabMap["Bomb Explosion Effect"].pool.Get(out GameObject element);
            element.transform.position = MapManager.Instance.GetPositionByCoordinate(coordinate);
            MapManager.Instance.DestroyBlock(coordinate);
        }
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
                prop.TakeDamageOnServer(info.damageSource.prop, 400f);
            }
        }

        Vector2Int[] coordinates = new Vector2Int[Bomb.InfoList.Count];
        for (int a = 0; a < coordinates.Length; ++a)
        {
            coordinates[a] = Bomb.InfoList[a].coordinate;
            if (MapManager.Instance.GetCell(coordinates[a].x, coordinates[a].y) == MapManager.Type.BlockDestructible)
            {
                LootEntry entry = LootManager.Instance.GetRandomInstance();
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

    #endregion METHOD ON SERVER OWNER
}