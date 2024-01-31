using System.Collections;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    private const string KEY_LOBBY_ADDRESS = "LobbyAddress";

    private const string KEY_LOBBY_NAME = "LobbyName";

    public Button buttonCreateLobby = null;

    private UNetworkManager networkManager = null;

    private Callback<GameLobbyJoinRequested_t> onJoinRequested = null;

    private Callback<LobbyCreated_t> onLobbyCreated = null;

    private Callback<LobbyEnter_t> onLobbyEnter = null;


    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam未能初始化。");
            return;
        }

        buttonCreateLobby.onClick.AddListener(CreateLobby);
        networkManager = NetworkManager.singleton as UNetworkManager;
        onJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        onLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        onLobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }


    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.LogError($"正在加入大厅[{callback.m_steamIDLobby}]。");
        buttonCreateLobby.gameObject.SetActive(false);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"创建大厅失败：{callback.m_eResult}。");
            HandleJoinLobbyResult(false);
            return;
        }

        Debug.LogError("创建大厅成功。");
        networkManager.StartHost();
        CSteamID steamIDLobby = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(steamIDLobby, KEY_LOBBY_ADDRESS, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(steamIDLobby, KEY_LOBBY_NAME, $"{SteamFriends.GetPersonaName()}的大厅");
        HandleJoinLobbyResult(true);
    }


    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        Debug.LogError($"加入大厅[{callback.m_ulSteamIDLobby}]成功。");
        if (NetworkServer.active)
        {
            return;
        }

        CSteamID steamIDLobby = new CSteamID(callback.m_ulSteamIDLobby);
        networkManager.networkAddress = SteamMatchmaking.GetLobbyData(steamIDLobby, KEY_LOBBY_ADDRESS);
        networkManager.StartClient();
        HandleJoinLobbyResult(true);
    }


    private void CreateLobby()
    {
        Debug.LogError("正在创建大厅。");
        buttonCreateLobby.gameObject.SetActive(false);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }


    private void HandleJoinLobbyResult(bool isSuccess)
    {
        if (isSuccess)
        {
            StartCoroutine(SpawnLocalPlayerAsync());
        }
        else
        {
            buttonCreateLobby.gameObject.SetActive(true);
        }
    }


    private IEnumerator SpawnLocalPlayerAsync()
    {
        while (PlayerIdentity.Local == null)
        {
            yield return null;
        }

        PlayerIdentity.Local.SpawnPlayerServerRPC();
    }
}