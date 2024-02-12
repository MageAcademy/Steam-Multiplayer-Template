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
                    prop.Initialize(this);
                    break;
                }
            }
        }
    }


    [Command(requiresAuthority = false)]
    public void DebugSetProp(int key)
    {
        switch (key)
        {
            case 1:
                prop.SetRemainingBombCountOnServer(prop.remainingBombCount - 1);
                break;
            case 2:
                prop.SetRemainingBombCountOnServer(prop.remainingBombCount + 1);
                break;
            case 3:
                prop.SetBombCountOnServer(prop.bombCount - 1);
                break;
            case 4:
                prop.SetBombCountOnServer(prop.bombCount + 1);
                break;
            case 5:
                prop.SetHealthOnServer(prop.health - Random.Range(0f, 1000f));
                break;
            case 6:
                prop.SetHealthOnServer(prop.health + Random.Range(0f, 1000f));
                break;
            case 7:
                prop.SetShieldOnServer(prop.shield - Random.Range(0f, 1250f));
                break;
            case 8:
                prop.SetShieldOnServer(prop.shield + Random.Range(0f, 1250f));
                break;
            case 9:
                prop.SetShieldLevelOnServer(prop.shieldLevel - 1);
                break;
            case 0:
                prop.SetShieldLevelOnServer(prop.shieldLevel + 1);
                break;
        }
    }
}