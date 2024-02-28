using Mirror;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool InGame = false;


    private void Awake()
    {
        InGame = false;
    }


    public static void ResetGameOnServer()
    {
        if (!NetworkServer.active || !InGame)
        {
            return;
        }

        InGame = false;
        if (PlayerIdentity.Local != null)
        {
            PlayerIdentity.Local.ResetGameOnServerOwner();
        }
    }


    public static void StartGameOnServer()
    {
        if (!NetworkServer.active || InGame)
        {
            return;
        }

        InGame = true;
        if (PlayerIdentity.Local != null || PlayerIdentity.Local.player != null)
        {
            PlayerIdentity.Local.player.GenerateMapServerRPC();
        }
    }
}