using DG.Tweening;
using Mirror;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static float GameTime = 300f;
    
    public static bool InGame = false;

    public static Vector2Int MapSize = new Vector2Int(20, 20);


    private void Awake()
    {
        DOTween.Init(false, false, LogBehaviour.Verbose);
        InGame = false;
        Bomb.InfoList.Clear();
        Bomb.InstanceMap.Clear();
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