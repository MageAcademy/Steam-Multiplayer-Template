using TMPro;
using UnityEngine;

public class DescriptionManager : MonoBehaviour
{
    public static DescriptionManager Instance = null;

    public RectTransform image = null;

    public TextMeshProUGUI text = null;

    private bool isShowing = false;

    private Describable target = null;


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        if (!isShowing)
        {
            return;
        }

        image.anchoredPosition = PlayerHudManager.GetMousePosition();
    }


    public void Hide()
    {
        image.gameObject.SetActive(false);
        image.sizeDelta = Vector2.zero;
        isShowing = false;
        text.text = "";
    }


    public void Show(Describable target)
    {
        image.gameObject.SetActive(true);
        isShowing = true;
        this.target = target;
        text.text = target.GetDescription();
        Vector2 size = text.GetComponent<RectTransform>().sizeDelta;
        size.x += 100f;
        size.y = text.preferredHeight + 50f;
        image.sizeDelta = size;
    }
}