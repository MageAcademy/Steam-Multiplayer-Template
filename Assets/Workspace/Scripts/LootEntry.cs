using Mirror;
using UnityEngine;

public class LootEntry : NetworkBehaviour
{
    public MeshRenderer meshRenderer = null;

    [SyncVar] public int networkID = 0;

    public ParticleSystem[] particleSystems = null;

    private LootManager.Data data = null;


    private void Start()
    {
        Initialize();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || LayerMask.LayerToName(other.gameObject.layer) != "Player")
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        PickLootOnServer(player);
    }


    public void Initialize()
    {
        data = LootManager.Instance.data[networkID - 1];
        Color color = LootManager.Instance.colorQuality[data.quality];
        meshRenderer.material.color = color;
        meshRenderer.material.SetColor("_Emission", color);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startColor = color;
        }
    }


    [ServerCallback]
    private void PickLootOnServer(Player player)
    {
        PlayerProperty prop = player.prop;
        bool flag = false;
        switch (networkID)
        {
            case 2:
                if (prop.shieldLevel < 2)
                {
                    flag = true;
                    prop.SetShieldLevelOnServer(2);
                }

                break;
            case 3:
                if (prop.shieldLevel < 3)
                {
                    flag = true;
                    prop.SetShieldLevelOnServer(3);
                }

                break;
            case 4:
                if (prop.shieldLevel < 4)
                {
                    flag = true;
                    prop.SetShieldLevelOnServer(4);
                }

                break;
            case 5:
                if (prop.shieldLevel < 5)
                {
                    flag = true;
                    prop.SetShieldLevelOnServer(5);
                }

                break;
            case 6:
                if (prop.shieldLevel < 6)
                {
                    flag = true;
                    prop.SetShieldLevelOnServer(6);
                }

                break;
            case 7:
                if (prop.health < PlayerProperty.MAX_HEALTH)
                {
                    flag = true;
                    prop.SetHealthOnServer(prop.health + 250f);
                }

                break;
            case 8:
                if (prop.health < PlayerProperty.MAX_HEALTH)
                {
                    flag = true;
                    prop.SetHealthOnServer(prop.health + 500f);
                }

                break;
            case 9:
                if (prop.shield < prop.shieldLevel * PlayerProperty.SHIELD_PER_LEVEL)
                {
                    flag = true;
                    prop.SetShieldOnServer(prop.shield + 250f);
                }

                break;
            case 10:
                if (prop.shield < prop.shieldLevel * PlayerProperty.SHIELD_PER_LEVEL)
                {
                    flag = true;
                    prop.SetShieldOnServer(prop.shield + 500f);
                }

                break;
            case 12:
                if (prop.bombRange < PlayerProperty.MAX_BOMB_RANGE)
                {
                    flag = true;
                    prop.SetBombRangeOnServer(prop.bombRange + 1);
                }

                break;
            case 13:
                if (prop.bombCount < PlayerProperty.MAX_BOMB_COUNT)
                {
                    flag = true;
                    prop.SetBombCountOnServer(prop.bombCount + 1);
                }

                break;
        }

        if (flag)
        {
            NetworkServer.UnSpawn(gameObject);
            Destroy(gameObject);
        }
    }
}