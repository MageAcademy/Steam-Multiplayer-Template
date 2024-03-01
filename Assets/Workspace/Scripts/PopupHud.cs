using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupHud : MonoBehaviour
{
    public CanvasGroup canvasGroup = null;

    public Image image = null;

    public RectTransform imageRect = null;

    public bool needFollow = false;

    public Vector3 position = new Vector3();

    public TextMeshProUGUI text = null;

    public RectTransform textRect = null;

    public Tweener tweener = null;


    private void LateUpdate()
    {
        if (!needFollow)
        {
            return;
        }

        imageRect.position = PlayerHudManager.GetScreenPosition(position);
    }
}