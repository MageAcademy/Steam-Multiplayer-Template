using Mirror;
using UnityEngine;

public class SafeZone : Unit
{
    public static SafeZone Instance = null;

    public AnimationCurve curve = null;

    public Transform mesh = null;

    private int count = 0;

    private Vector3 currentCenter = new Vector3();

    private float currentDamage = 0f;

    private float currentScale = 0f;

    private float currentTime = 0f;


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        Refresh();
        DealDamageOnServer();
    }


    [ServerCallback]
    private void DealDamageOnServer()
    {
        if (!GameManager.InGame || currentTime < count)
        {
            return;
        }

        ++count;
        foreach (PlayerIdentity identity in PlayerIdentity.InstanceList)
        {
            if (identity == null || identity.player == null || identity.player.prop.networkIsDead)
            {
                continue;
            }

            Vector3 playerPosition = identity.player.transform.position;
            playerPosition.y = 0f;
            if (Vector3.Distance(playerPosition, currentCenter) > currentScale / 2f)
            {
                DealDamageOnServer(identity.player.prop, currentDamage, DamageType.HealthOnly);
            }
        }
    }


    public void Hide()
    {
        mesh.gameObject.SetActive(false);
    }


    private void Refresh()
    {
        if (!GameManager.InGame || PlayerIdentity.Local == null || PlayerIdentity.Local.player == null)
        {
            count = 0;
            return;
        }

        currentTime = PlayerIdentity.Local.player.networkGameTime;
        float progress = currentTime / GameManager.GameTime;
        currentCenter = Vector3.Lerp(Vector3.zero, MapManager.Instance.data.safeZoneEndCenter,
            curve.Evaluate(progress));
        currentDamage = Mathf.Lerp(10f, 100f, curve.Evaluate(progress));
        currentScale = Mathf.Lerp(MapManager.Instance.data.safeZoneStartScale, 0f, curve.Evaluate(progress));
        mesh.localPosition = currentCenter;
        mesh.localScale = new Vector3(currentScale, 1.5f, currentScale);
    }


    public void Show()
    {
        mesh.gameObject.SetActive(true);
    }
}