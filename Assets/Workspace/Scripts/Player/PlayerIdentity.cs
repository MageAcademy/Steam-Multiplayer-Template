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
        GameObject gameObject = Instantiate(prefabPlayer);
        Player player = gameObject.GetComponent<Player>();
        player.networkSteamID = networkSteamID;
        player.playerAppearance.networkColor =
            new Color(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 1f);
        player.transform.position = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        NetworkServer.Spawn(gameObject, conn);
    }


    [Command(requiresAuthority = false)]
    public void UnSpawnPlayerServerRPC()
    {
        if (player == null)
        {
            return;
        }

        Destroy(player.gameObject);
        NetworkServer.UnSpawn(player.gameObject);
    }
}