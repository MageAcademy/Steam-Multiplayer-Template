using System.Collections;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public PlayerIdentity identity = null;

    public bool isInitialized = false;

    [SyncVar] public ulong networkSteamID = 0L;


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
                    isInitialized = true;
                    identity = playerIdentity;
                    playerIdentity.player = this;
                    break;
                }
            }
        }
    }
}