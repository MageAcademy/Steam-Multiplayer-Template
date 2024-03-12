#pragma warning disable 465

using System.Collections.Generic;
using System.Linq;
using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerIdentity : NetworkBehaviour
{
    public static List<PlayerIdentity> InstanceList = new List<PlayerIdentity>();

    public static PlayerIdentity Local = null;

    [SyncVar] public ulong networkSteamID = 0L;

    [SyncVar] public string networkSteamName = null;

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


    private void Finalize()
    {
        InstanceList.Remove(this);
        if (hasAuthority)
        {
            Local = null;
        }
    }


    public static int GetAlivePlayerCount()
    {
        return InstanceList.Count(identity => identity.player != null && !identity.player.prop.networkIsDead);
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


    [ClientRpc]
    private void ResetGameClientRPC()
    {
        if (Local != null)
        {
            Local.SpawnPlayerServerRPC();
        }

        MapManager.Instance.ClearOnClient();
        PopupManager.Instance.Reset();
    }


    [ServerCallback]
    public void ResetGameOnServerOwner()
    {
        PlayerProperty.BuffID = 0;
        foreach (Bomb bomb in Bomb.InstanceMap.Values)
        {
            NetworkServer.UnSpawn(bomb.gameObject);
            Destroy(bomb.gameObject);
        }

        Bomb.InstanceMap.Clear();
        foreach (LootEntry entry in LootEntry.InstanceList)
        {
            NetworkServer.UnSpawn(entry.gameObject);
            Destroy(entry.gameObject);
        }

        foreach (PlayerIdentity identity in InstanceList)
        {
            if (identity.player != null)
            {
                NetworkServer.UnSpawn(identity.player.gameObject);
                Destroy(identity.player.gameObject);
            }
        }

        ResetGameClientRPC();
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
        player.transform.position = Vector3.zero;
        NetworkServer.Spawn(gameObject, conn);
    }
}