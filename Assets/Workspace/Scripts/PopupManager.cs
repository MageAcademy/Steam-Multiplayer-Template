using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Vector2 = UnityEngine.Vector2;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance = null;

    public AnimationCurve curveKnockDownAlpha = new AnimationCurve();

    public AnimationCurve curveKnockDownPositionX = new AnimationCurve();

    public AnimationCurve curveTakeDamageAlpha = new AnimationCurve();

    public AnimationCurve curveTakeDamageScale = new AnimationCurve();

    public TMP_FontAsset fontHanyiwenhei = null;

    public TMP_FontAsset fontPingfang = null;

    public Transform parentKnockDownGlobal = null;

    public Transform parentKnockDownLocal = null;

    public Transform parentPopupHud = null;

    public PopupHud prefab = null;

    public PostProcessVolume volume = null;

    private ColorGrading colorGrading = null;

    private List<PopupHud> popopHudQueue = new List<PopupHud>();

    private Tweener tweenerTakeDamageEffect = null;

    private Tweener tweenerTakeFatalDamageEffect = null;

    private Vignette vignette = null;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        volume.profile.TryGetSettings(out colorGrading);
        volume.profile.TryGetSettings(out vignette);
    }


    public void PlayDamageEffect(float value, Vector3 position, string destinationName, string sourceName,
        bool isDestination, bool isSource)
    {
        if (!isDestination && !isSource)
        {
            return;
        }

        if (isDestination)
        {
            tweenerTakeDamageEffect?.Kill();
            tweenerTakeDamageEffect = DOTween.To(value => { vignette.intensity.value = value; }, 0.6f, 0f, 0.6f)
                .SetEase(Ease.InOutSine);
        }

        AudioManager.Instance.Play("玩家击中", null, position);
        PopupHud hud = Instantiate(prefab, parentPopupHud);
        hud.image.color = Color.clear;
        hud.needFollow = true;
        hud.position = position + new Vector3(0f, 0.6f, 0f);
        hud.text.color = new Color(1f, 0.1f, 0.1f, 1f);
        hud.text.font = fontHanyiwenhei;
        hud.text.fontSize = 40f;
        hud.text.text = Mathf.RoundToInt(value).ToString();
        hud.imageRect.sizeDelta = hud.text.GetPreferredValues();
        hud.tweener = DOTween.To(value =>
        {
            hud.canvasGroup.alpha = curveTakeDamageAlpha.Evaluate(value);
            float scale = curveTakeDamageScale.Evaluate(value);
            hud.textRect.localScale = new Vector3(scale, scale, 1f);
        }, 0f, 1f, 2f).SetEase(Ease.Linear);
        hud.tweener.onComplete = () => { Destroy(hud.gameObject); };
    }


    public void PlayFatalDamageEffect(float value, Vector3 position, string destinationName, string sourceName,
        bool isDestination, bool isSource)
    {
        AudioManager.Instance.Play("玩家击倒", null, position);
        if (!isDestination && !isSource)
        {
            return;
        }

        if (isDestination)
        {
            CameraController.Instance.SetTarget(null);
            tweenerTakeFatalDamageEffect?.Kill();
            tweenerTakeFatalDamageEffect = DOTween
                .To(value => { colorGrading.saturation.value = value; }, 0f, -60f, 1.2f).SetEase(Ease.InOutSine);
        }

        if (isSource)
        {
            PopupHud hud2 = Instantiate(prefab, parentKnockDownLocal);
            hud2.image.color = new Color(0f, 0f, 0f, 0.6f);
            hud2.needFollow = false;
            hud2.text.color = Color.white;
            hud2.text.font = fontHanyiwenhei;
            hud2.text.fontSize = 40f;
            hud2.text.text = $"击倒 <#FF0000>{destinationName}</color>";
            hud2.imageRect.sizeDelta = hud2.text.GetPreferredValues() + new Vector2(100f, 50f);
            hud2.tweener = DOTween.To(value =>
            {
                hud2.canvasGroup.alpha = curveTakeDamageAlpha.Evaluate(value);
                float scale = curveTakeDamageScale.Evaluate(value);
                hud2.textRect.localScale = new Vector3(scale, scale, 1f);
            }, 0f, 1f, 3f).SetEase(Ease.Linear);
            hud2.tweener.onComplete = () => { Destroy(hud2.gameObject); };
        }

        PopupHud hud3 = Instantiate(prefab, parentPopupHud);
        hud3.image.color = Color.clear;
        hud3.needFollow = true;
        hud3.position = position + new Vector3(0f, 0.6f, 0f);
        hud3.text.color = new Color(1f, 0f, 0f, 1f);
        hud3.text.font = fontHanyiwenhei;
        hud3.text.fontSize = 50f;
        hud3.text.text = Mathf.RoundToInt(value).ToString();
        hud3.imageRect.sizeDelta = hud3.text.GetPreferredValues();
        hud3.tweener = DOTween.To(value =>
        {
            hud3.canvasGroup.alpha = curveTakeDamageAlpha.Evaluate(value);
            float scale = curveTakeDamageScale.Evaluate(value);
            hud3.textRect.localScale = new Vector3(scale, scale, 1f);
        }, 0f, 1f, 2f).SetEase(Ease.Linear);
        hud3.tweener.onComplete = () => { Destroy(hud3.gameObject); };
    }


    public void PlayKnockDownGlobalEffect(string destinationName, string sourceName)
    {
        PopupHud hud = Instantiate(prefab, parentKnockDownGlobal);
        popopHudQueue.Add(hud);
        while (popopHudQueue.Count > 6)
        {
            popopHudQueue[0].tweener?.Kill(true);
        }

        hud.image.color = Color.clear;
        hud.needFollow = false;
        hud.text.color = Color.white;
        hud.text.font = fontPingfang;
        hud.text.fontSize = 30f;
        hud.text.text = $"{sourceName} <#808080>击倒</color> {destinationName}";
        hud.imageRect.sizeDelta = hud.text.GetPreferredValues();
        hud.tweener = DOTween.To(value =>
        {
            hud.canvasGroup.alpha = curveKnockDownAlpha.Evaluate(value);
            hud.textRect.anchoredPosition = new Vector2(curveKnockDownPositionX.Evaluate(value), 0f);
        }, 0f, 1f, 10f).SetEase(Ease.Linear);
        hud.tweener.onComplete = () =>
        {
            Destroy(hud.gameObject);
            popopHudQueue.Remove(hud);
        };
    }


    public void Reset()
    {
        tweenerTakeDamageEffect?.Kill();
        tweenerTakeFatalDamageEffect?.Kill();
        colorGrading.saturation.value = 0f;
        vignette.intensity.value = 0f;
    }
}