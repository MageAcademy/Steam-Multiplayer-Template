#pragma warning disable 465

using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerIdentity : NetworkBehaviour
{
    public static List<PlayerIdentity> InstanceList = new List<PlayerIdentity>();

    public static PlayerIdentity Local = null;

    [SyncVar] public ulong networkSteamID = 0L;

    [SyncVar(hook = nameof(OnSteamNameValueChange))]
    public string networkSteamName = null;

    public Player player = null;

    private UNetworkManager networkManager = null;

    private GameObject prefabPlayer = null;


    private void Start()
    {
        Initialize();
        InitializeOnLocalPlayer();
    }


    private void OnDestroy()
    {
        Finalize();
    }


    private void OnSteamNameValueChange(string _, string newValue)
    {
    }


    private void Finalize()
    {
        InstanceList.Remove(this);
        if (hasAuthority)
        {
            Local = null;
        }
    }


    private void Initialize()
    {
        InstanceList.Add(this);
        networkManager = NetworkManager.singleton as UNetworkManager;
        prefabPlayer = networkManager.spawnPrefabs.Find(gameObject => gameObject.name == "Player");
    }


    private void InitializeOnLocalPlayer()
    {
        if (!hasAuthority)
        {
            return;
        }

        Local = this;
        SetSteamIDServerRPC(SteamUser.GetSteamID().m_SteamID);
        SetSteamNameServerRPC(SteamFriends.GetPersonaName());
    }


    [Command(requiresAuthority = false)]
    private void SetSteamIDServerRPC(ulong steamID)
    {
        networkSteamID = steamID;
    }


    [Command(requiresAuthority = false)]
    private void SetSteamNameServerRPC(string steamName)
    {
        networkSteamName = steamName;
    }


    [Command(requiresAuthority = false)]
    public void SpawnPlayerServerRPC(NetworkConnectionToClient conn = null)
    {
        GameObject player = Instantiate(prefabPlayer);
        player.GetComponent<Player>().networkSteamID = networkSteamID;
        NetworkServer.Spawn(player, conn);
    }


    [Command(requiresAuthority = false)]
    public void UnSpawnPlayerServerRPC(NetworkConnectionToClient conn = null)
    {
        if (player == null)
        {
            return;
        }

        Destroy(player.gameObject);
        NetworkServer.UnSpawn(player.gameObject);
    }
}