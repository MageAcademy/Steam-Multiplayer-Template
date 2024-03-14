using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootHud : MonoBehaviour
{
    public RectTransform background = null;

    public Image image = null;

    public TextMeshProUGUI text = null;

    private LootEntry entry = null;


    private void Update()
    {
        CheckNull();
    }


    private void LateUpdate()
    {
        Follow();
    }


    private void CheckNull()
    {
        if (entry == null)
        {
            PrefabManager.PrefabMap["Loot Hud"].pool.Release(gameObject);
        }
    }


    private void Follow()
    {
        if (entry == null)
        {
            return;
        }

        transform.position = PlayerHudManager.GetScreenPosition(entry.transform.position);
    }


    public void Initialize(LootEntry entry)
    {
        this.entry = entry;
        LootManager.Data data = entry.data;
        if (data.quality < 3)
        {
            image.color = new Color(0f, 0f, 0f, 0.6f);
            text.color = LootManager.Instance.colorQuality[data.quality];
        }
        else
        {
            Color color = LootManager.Instance.colorQuality[data.quality];
            color.a = 0.6f;
            image.color = color;
            text.color = Color.white;
        }

        text.text = data.name;
        background.sizeDelta = new Vector2(text.preferredWidth + 40f, text.preferredHeight + 10f);
    }
}