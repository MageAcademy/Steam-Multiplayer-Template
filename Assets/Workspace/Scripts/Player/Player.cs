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


    private void Awake()
    {
        OnServerOwnerAwake();
    }


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
            element.transform.position =
                MapManager.Instance.GetPositionOnFloor(coordinate) + new Vector3(0f, 0.01f, 0f);
        }
    }

    #endregion RPC


    #region METHOD ON SERVER OWNER

    [ServerCallback]
    private void HandleBombInfoListOnServerOwner()
    {
        if (!hasAuthority || !isServer)
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
            if (Bomb.InfoList.Contains(prop.player.playerMove.networkCoordinate))
            {
                prop.TakeDamageOnServer(null, 400f);
            }
        }

        HandleBombInfoListClientRPC(Bomb.InfoList.ToArray());
        Bomb.InfoList.Clear();
    }


    [ServerCallback]
    private void OnServerOwnerAwake()
    {
        if (!hasAuthority || !isServer)
        {
            return;
        }

        Bomb.InstanceMap.Clear();
    }

    #endregion METHOD ON SERVER OWNER
}