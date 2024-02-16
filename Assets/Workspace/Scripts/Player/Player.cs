using System.Collections;
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

    #endregion RPC
}