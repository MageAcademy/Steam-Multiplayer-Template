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

    private bool isTweening = false;

    private Vector3 lastTargetPosition = new Vector3();

    private Vector3 offset = new Vector3();

    private float positionY = 0f;

    private Transform target = null;


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
        if (target == null)
        {
            lastTargetPosition = cameraPosition.position;
            lastTargetPosition.y = 0f;
            return;
        }

        if (isTweening)
        {
            return;
        }

        Vector3 currentTargetPosition = target.position;
        currentTargetPosition.y = 0f;
        if (Vector3.Distance(currentTargetPosition, lastTargetPosition) < teleportThreshold)
        {
            lastTargetPosition = currentTargetPosition;
            positionY = Mathf.Lerp(positionY, target.position.y, Time.deltaTime * 6f);
            currentTargetPosition.y = positionY;
            Apply(currentTargetPosition);
        }
        else
        {
            isTweening = true;
            Vector3 startPosition = cameraPosition.position;
            Tweener tweener = DOTween
                .To(value => { Apply(Vector3.Lerp(startPosition, target.position, value)); }, 0f, 1f, 0.4f);
            tweener.SetEase(Ease.InOutSine);
            tweener.onComplete = () =>
            {
                isTweening = false;
                lastTargetPosition = target.position;
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
        positionY = target.position.y;
        this.target = target;
    }
}