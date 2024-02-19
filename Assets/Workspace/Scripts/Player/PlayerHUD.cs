using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
    public Image[] imageBombForeground = null;

    public Image imageHealthBackground = null;

    public Image imageHealthForeground = null;

    public GameObject imageLowHealth = null;

    public Image[] imageShieldBackground = null;

    public Image[] imageShieldForeground = null;

    public TextMeshProUGUI textSteamName = null;

    private float currentHealthBackground = 0f;

    private float currentShieldBackground = 0f;

    private float healthBackgroundCooldown = 0f;

    private Player player = null;

    private float shieldBackgroundCooldown = 0f;

    private float targetHealthBackground = 0f;

    private float targetShieldBackground = 0f;


    private void Update()
    {
        CheckNull();
        RefreshBackground();
    }


    private void LateUpdate()
    {
        Follow();
    }


    public void ApplyRemainingBombCount(int value)
    {
        for (int a = 0; a < imageBombForeground.Length; ++a)
        {
            imageBombForeground[a].color =
                a < value ? PlayerHudManager.Instance.colorBombOn : PlayerHudManager.Instance.colorBombOff;
        }
    }


    public void ApplyBombCount(int value)
    {
        for (int a = 0; a < imageBombForeground.Length; ++a)
        {
            imageBombForeground[a].gameObject.SetActive(a < value);
        }
    }


    public void ApplyHealth(float value)
    {
        imageHealthForeground.transform.localScale = new Vector3(value / PlayerProperty.MAX_HEALTH, 1f, 1f);
        imageLowHealth.SetActive(value < 200f && value > 0f);
        if (value < targetHealthBackground)
        {
            healthBackgroundCooldown = 0.2f;
        }
        else
        {
            currentHealthBackground = value;
            healthBackgroundCooldown = 0f;
        }

        targetHealthBackground = value;
    }


    public void ApplyShield(float value)
    {
        ApplyShieldImage(imageShieldForeground, value);
        if (value < targetShieldBackground)
        {
            shieldBackgroundCooldown = 0.2f;
        }
        else
        {
            currentShieldBackground = value;
            shieldBackgroundCooldown = 0f;
        }

        targetShieldBackground = value;
    }


    private void ApplyShieldImage(Image[] images, float shield)
    {
        float ratio = shield / PlayerProperty.SHIELD_PER_LEVEL;
        int index = Mathf.FloorToInt(ratio);
        for (int a = 0; a < images.Length; ++a)
        {
            if (a < index)
            {
                images[a].transform.localScale = Vector3.one;
            }
            else if (a == index)
            {
                images[a].transform.localScale = new Vector3(ratio - index, 1f, 1f);
            }
            else
            {
                images[a].transform.localScale = new Vector3(0f, 1f, 1f);
            }
        }
    }


    public void ApplyShieldLevel(int value)
    {
        for (int a = 0; a < imageShieldBackground.Length; ++a)
        {
            imageShieldBackground[a].gameObject.SetActive(a < value);
            imageShieldForeground[a].color = LootManager.Instance.colorQuality[value - 2];
            imageShieldForeground[a].gameObject.SetActive(a < value);
        }
    }


    private void CheckNull()
    {
        if (player == null)
        {
            Destroy(gameObject);
        }
    }


    private void Follow()
    {
        if (player == null)
        {
            return;
        }

        Vector3 center = PlayerHudManager.Instance.mainCamera.WorldToScreenPoint(player.playerCenter.position);
        Vector3 top = PlayerHudManager.Instance.mainCamera.WorldToScreenPoint(player.playerTop.position);
        transform.position = new Vector3(center.x, top.y + 50f, 0f);
    }


    public void Initialize(Player player)
    {
        currentHealthBackground = PlayerProperty.MAX_HEALTH;
        currentShieldBackground = player.prop.shieldLevel * PlayerProperty.SHIELD_PER_LEVEL;
        healthBackgroundCooldown = 0f;
        this.player = player;
        shieldBackgroundCooldown = 0f;
        targetHealthBackground = PlayerProperty.MAX_HEALTH;
        targetShieldBackground = player.prop.shieldLevel * PlayerProperty.SHIELD_PER_LEVEL;
        textSteamName.text = player.identity.networkSteamName;
        ApplyRemainingBombCount(player.prop.remainingBombCount);
        ApplyBombCount(player.prop.bombCount);
        ApplyHealth(player.prop.health);
        ApplyShield(player.prop.shield);
        ApplyShieldLevel(player.prop.shieldLevel);
    }


    private void RefreshBackground()
    {
        if (player == null)
        {
            return;
        }

        if (healthBackgroundCooldown > 0f)
        {
            healthBackgroundCooldown -= Time.deltaTime;
        }
        else
        {
            if (currentHealthBackground < targetHealthBackground)
            {
                imageHealthBackground.transform.localScale =
                    new Vector3(targetHealthBackground / PlayerProperty.MAX_HEALTH, 1f, 1f);
            }
            else
            {
                currentHealthBackground -= Time.deltaTime * 1000f;
                imageHealthBackground.transform.localScale =
                    new Vector3(currentHealthBackground / PlayerProperty.MAX_HEALTH, 1f, 1f);
            }
        }

        if (shieldBackgroundCooldown > 0f)
        {
            shieldBackgroundCooldown -= Time.deltaTime;
        }
        else
        {
            if (currentShieldBackground < targetShieldBackground)
            {
                ApplyShieldImage(imageShieldBackground, targetShieldBackground);
            }
            else
            {
                currentShieldBackground -= Time.deltaTime * 1000f;
                ApplyShieldImage(imageShieldBackground, currentShieldBackground);
            }
        }
    }
}