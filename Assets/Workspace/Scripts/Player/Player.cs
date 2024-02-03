using System.Collections;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public PlayerIdentity identity = null;

    public bool isInitialized = false;

    [SyncVar] public ulong networkSteamID = 0L;

    public PlayerAppearance playerAppearance = null;

    public PlayerMove playerMove = null;


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
            DestroyImmediate(this);
        }
    }


    [ClientRpc]
    private void GenerateMapClientRPC(string json)
    {
        MapManager.Instance.GenerateOnClient(json);
    }


    [Command(requiresAuthority = false)]
    public void GenerateMapServerRPC()
    {
        string json = MapManager.Instance.GenerateOnServer(12, 12);
        GenerateMapClientRPC(json);
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
                    }

                    identity = playerIdentity;
                    isInitialized = true;
                    playerAppearance.Initialize();
                    playerIdentity.player = this;
                    playerMove.Initialize(this);
                    if (isServer)
                    {
                        PlayerMove.IsEnabled = true;
                    }

                    break;
                }
            }
        }
    }
}