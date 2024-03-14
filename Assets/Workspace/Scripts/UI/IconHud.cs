using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconHud : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image imageBackground = null;

    public Image imageForeground = null;

    public bool isIncreased = false;

    public Describable target = null;

    public TextMeshProUGUI text = null;


    private void Update()
    {
        if (isIncreased)
        {
            imageForeground.fillAmount = Mathf.Lerp(1f, 0f, target.remainingTime / target.duration);
        }
        else
        {
            imageForeground.fillAmount = Mathf.Lerp(0f, 1f, target.remainingTime / target.duration);
        }

        string message = "";
        if (target.remainingTime > 0f)
        {
            message = Mathf.FloorToInt(target.remainingTime).ToString();
        }

        text.text = message;
    }


    private void OnDestroy()
    {
        DescriptionManager.Instance.Hide();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        DescriptionManager.Instance.Show(target);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        DescriptionManager.Instance.Hide();
    }
}