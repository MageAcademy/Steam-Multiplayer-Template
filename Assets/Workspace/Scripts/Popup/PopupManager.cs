using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance = null;

    public PostProcessVolume volume = null;

    private ColorGrading colorGrading = null;

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


    public void PlayTakeDamageEffect(float value, Vector3 position, bool isLocalPlayer)
    {
        AudioManager.Instance.Play("玩家击中", null, position);
        if (!isLocalPlayer)
        {
            return;
        }

        tweenerTakeDamageEffect?.Kill();
        tweenerTakeDamageEffect = DOTween.To(value => { vignette.intensity.value = value; }, 0.6f, 0f, 0.6f)
            .SetEase(Ease.InOutSine);
    }


    public void PlayTakeFatalDamageEffect(float value, Vector3 position, bool isLocalPlayer)
    {
        AudioManager.Instance.Play("玩家击倒", null, position);
        if (!isLocalPlayer)
        {
            return;
        }

        CameraController.Instance.SetTarget(null);
        tweenerTakeFatalDamageEffect?.Kill();
        tweenerTakeFatalDamageEffect = DOTween.To(value => { colorGrading.saturation.value = value; }, 0f, -60f, 1.2f)
            .SetEase(Ease.InOutSine);
    }


    public void Reset()
    {
        tweenerTakeDamageEffect?.Kill();
        tweenerTakeFatalDamageEffect?.Kill();
        colorGrading.saturation.value = 0f;
        vignette.intensity.value = 0f;
    }
}