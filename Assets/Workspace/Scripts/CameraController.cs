using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance = null;

    public Transform cameraDistance = null;

    public Transform cameraPosition = null;

    public Transform cameraRotation = null;

    public Transform mainCamera = null;

    public float offsetRange = 2f;

    public float teleportThreshold = 0.5f;

    private bool isTargetNull = true;

    private bool isTweening = false;

    private Vector3 lastTargetPosition = new Vector3();

    private Vector3 offset = new Vector3();

    private float positionY = 0f;

    private Transform target = null;

    private Tweener tweenerCameraDistance = null;


    private void Awake()
    {
        Instance = this;
    }


    private void LateUpdate()
    {
        Look();
        Follow();
    }


    private void Apply(Vector3 position)
    {
        cameraPosition.position = position;
        mainCamera.position = cameraDistance.position + offset;
        mainCamera.rotation = cameraDistance.rotation;
    }


    private void Follow()
    {
        if (isTweening)
        {
            return;
        }

        isTargetNull = target == null;
        Vector3 currentTargetPosition = isTargetNull ? Vector3.zero : target.position;
        currentTargetPosition.y = 0f;
        if (Vector3.Distance(currentTargetPosition, lastTargetPosition) < teleportThreshold)
        {
            lastTargetPosition = currentTargetPosition;
            positionY = Mathf.Lerp(positionY, isTargetNull ? 0f : target.position.y, Time.deltaTime * 6f);
            currentTargetPosition.y = positionY;
            Apply(currentTargetPosition);
        }
        else
        {
            isTweening = true;
            Vector3 startPosition = cameraPosition.position;
            Tweener tweener = DOTween.To(value =>
            {
                Apply(Vector3.Lerp(startPosition, target == null ? Vector3.zero : target.position,
                    value));
            }, 0f, 1f, 0.4f);
            tweener.SetEase(Ease.InOutSine);
            tweener.onComplete = () =>
            {
                isTweening = false;
                lastTargetPosition = target == null ? Vector3.zero : target.position;
                positionY = lastTargetPosition.y;
                lastTargetPosition.y = 0f;
            };
        }
    }


    private void Look()
    {
        Vector3 targetOffset = (InputManager.IsLookValid ? InputManager.LookValue : Vector3.zero) * offsetRange;
        offset = Vector3.Lerp(offset, targetOffset, Time.deltaTime * 8f);
    }


    public void SetTarget(Transform target)
    {
        isTargetNull = target == null;
        this.target = target;
        tweenerCameraDistance?.Kill();
        if (isTargetNull)
        {
            positionY = 0f;
            tweenerCameraDistance = DOTween
                .To(value => { cameraDistance.localPosition = new Vector3(0f, 0f, value); }, -10f, -24f, 1.2f)
                .SetEase(Ease.InOutCirc);
        }
        else
        {
            cameraDistance.localPosition = new Vector3(0f, 0f, -10f);
            positionY = target.position.y;
        }
    }
}