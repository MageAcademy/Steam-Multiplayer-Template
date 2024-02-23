using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LootEntry : NetworkBehaviour
{
    public static List<LootEntry> InstanceList = new List<LootEntry>();
    
    public GameObject beamOrange = null;

    public GameObject beamRed = null;

    public LootManager.Data data = null;

    public MeshRenderer meshRenderer = null;

    [SyncVar] public int networkID = 0;

    public ParticleSystem[] particleSystems = null;

    public ParticleSystem[] sparkles = null;


    private void Start()
    {
        Initialize();
    }


    private void OnDestroy()
    {
        InstanceList.Remove(this);
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


    private void Initialize()
    {
        InstanceList.Add(this);
        data = LootManager.Instance.data[networkID - 1];
        beamOrange.SetActive(data.quality == 3);
        beamRed.SetActive(data.quality == 4);
        Color color = LootManager.Instance.colorQuality[data.quality];
        meshRenderer.material.color = color;
        meshRenderer.material.SetColor("_EmissionColor", color);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.startColor = color;
        }

        int[] frameIndex = { 3, 6, 2, 8, 1 };
        foreach (ParticleSystem particleSystem in sparkles)
        {
            ParticleSystem.TextureSheetAnimationModule animationModule = particleSystem.textureSheetAnimation;
            animationModule.startFrame = frameIndex[data.quality] / 9f;
        }

        PlayerHudManager.Instance.GetLootHud(this);
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
                    prop.SetRemainingBombCountOnServer(prop.remainingBombCount + 1);
                }

                break;
            case 14:
                flag = true;
                prop.AddBuffOnServer(PlayerProperty.BuffType.MoveSpeedAdd_SoulJade, 2f, 20f);
                break;
        }

        if (flag)
        {
            NetworkServer.UnSpawn(gameObject);
            Destroy(gameObject);
        }
    }
}