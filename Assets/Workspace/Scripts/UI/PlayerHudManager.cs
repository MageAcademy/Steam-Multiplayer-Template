using UnityEngine;

public class PlayerHudManager : MonoBehaviour
{
    public static PlayerHudManager Instance = null;

    public Color colorBombOff = new Color();

    public Color colorBombOn = new Color();

    public Color colorHealthAlly = new Color();

    public Color colorHealthEnemy = new Color();

    public Camera mainCamera = null;

    public Transform parentLootHud = null;

    public Transform parentPlayerHud = null;

    public PlayerHud prefabPlayerHud = null;

    public RectTransform pivotBottomLeftMainCanvas = null;


    private void Awake()
    {
        Instance = this;
    }


    public void GetLootHud(LootEntry entry)
    {
        PrefabManager.PrefabMap["Loot Hud"].pool.Get(out GameObject element);
        element.transform.SetParent(parentLootHud);
        LootHud hud = element.GetComponent<LootHud>();
        hud.Initialize(entry);
    }


    public PlayerHud GetPlayerHud(Player player)
    {
        PlayerHud playerHud = Instantiate(prefabPlayerHud, parentPlayerHud);
        playerHud.Initialize(player);
        return playerHud;
    }


    public static Vector2 GetMousePosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Instance.pivotBottomLeftMainCanvas, Input.mousePosition, null,
            out Vector2 mousePosition);
        return mousePosition;
    }


    public static Vector3 GetScreenPosition(Vector3 worldPosition)
    {
        return Instance.mainCamera.WorldToScreenPoint(worldPosition);
    }
}