using UnityEngine;

public class PlayerHudManager : MonoBehaviour
{
    public static PlayerHudManager Instance = null;

    public Color colorBombOff = new Color();

    public Color colorBombOn = new Color();

    public Camera mainCamera = null;

    public Transform parentPlayerHud = null;

    public PlayerHud prefabPlayerHud = null;


    private void Awake()
    {
        Instance = this;
    }


    public PlayerHud GetPlayerHud(Player player)
    {
        PlayerHud playerHud = Instantiate(prefabPlayerHud, parentPlayerHud);
        playerHud.Initialize(player);
        return playerHud;
    }
}