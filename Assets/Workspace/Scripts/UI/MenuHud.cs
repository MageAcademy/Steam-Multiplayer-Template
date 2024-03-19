using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuHud : MonoBehaviour
{
    public Image image = null;

    public RectTransform rectTransformImage = null;

    private Tweener tweener = null;


    public void Initialize(int index, int count)
    {
        image.fillAmount = 1f / count;
        rectTransformImage.localRotation = Quaternion.Euler(0f, 0f, 180f / count);
        transform.localRotation = Quaternion.Euler(0f, 0f, -360f * index / count);
    }


    public void TweenIn()
    {
        tweener?.Kill();
        tweener = DOTween.To(value =>
        {
            image.color = Color.Lerp(Color.white, Color.green, value);
            float size = Mathf.Lerp(500f, 520f, value);
            rectTransformImage.sizeDelta = new Vector2(size, size);
        }, 0f, 1f, 0.1f).SetEase(Ease.OutExpo);
    }


    public void TweenOut()
    {
        tweener?.Kill();
        tweener = DOTween.To(value =>
        {
            image.color = Color.Lerp(Color.white, Color.green, value);
            float size = Mathf.Lerp(500f, 520f, value);
            rectTransformImage.sizeDelta = new Vector2(size, size);
        }, 1f, 0f, 0.1f).SetEase(Ease.OutExpo);
    }
}