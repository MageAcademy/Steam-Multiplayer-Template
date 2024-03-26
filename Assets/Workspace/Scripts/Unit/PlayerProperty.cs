using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerProperty : Unit
{
    public enum BuffType
    {
        MoveSpeedAdd,
        MoveSpeedAdd_SoulJade,
        MoveSpeedEql,
        MoveSpeedMul,
        Null
    }

    public class Buff : Describable
    {
        public GameObject iconHud = null;

        public int id = 0;

        public BuffType type = BuffType.Null;

        public float floatValue = 0f;


        public override string GetDescription()
        {
            return GetBuffDescription(this);
        }
    }

    public const int MAX_BOMB_COUNT = 5;

    public const int MAX_BOMB_RANGE = 5;

    public const float MAX_HEALTH = 500f;

    public const float MAX_MOVE_SPEED = 10f;

    public const int MAX_SHIELD_LEVEL = 6;

    public const int MIN_BOMB_COUNT = 1;

    public const int MIN_BOMB_RANGE = 1;

    public const float MIN_MOVE_SPEED = 0f;

    public const int MIN_SHIELD_LEVEL = 2;

    public const float SHIELD_PER_LEVEL = 250f;

    public static int BuffID = 0;

    [SyncVar(hook = nameof(OnRemainingBombCountChange))]
    public int remainingBombCount = MIN_BOMB_COUNT;

    [SyncVar(hook = nameof(OnBombCountChange))]
    public int bombCount = MIN_BOMB_COUNT;

    [SyncVar] public int bombRange = 1;

    public List<Buff> buffList = new List<Buff>();

    [SyncVar(hook = nameof(OnHealthChange))]
    public float health = MAX_HEALTH;

    [SyncVar] public float moveSpeed = 4f;

    public Player player = null;

    [SyncVar(hook = nameof(OnShieldChange))]
    public float shield = MIN_SHIELD_LEVEL * SHIELD_PER_LEVEL;

    [SyncVar(hook = nameof(OnShieldLevelChange))]
    public int shieldLevel = MIN_SHIELD_LEVEL;

    private float moveSpeedBase = 4f;


    private void Update()
    {
        CountdownBuffList();
    }


    private void OnDestroy()
    {
        foreach (Buff buff in buffList)
        {
            if (buff.iconHud != null)
            {
                Destroy(buff.iconHud);
            }
        }
    }


    private void OnRemainingBombCountChange(int _, int newValue)
    {
        player?.playerHud?.ApplyRemainingBombCount(newValue);
    }


    private void OnBombCountChange(int _, int newValue)
    {
        player?.playerHud?.ApplyBombCount(newValue);
    }


    private void OnHealthChange(float _, float newValue)
    {
        player?.playerHud?.ApplyHealth(newValue);
    }


    private void OnShieldChange(float _, float newValue)
    {
        player?.playerHud?.ApplyShield(newValue);
    }


    private void OnShieldLevelChange(int _, int newValue)
    {
        player?.playerHud?.ApplyShieldLevel(newValue);
    }


    [ClientRpc]
    private void AddBuffClientRPC(int buffID, BuffType buffType, float floatValue, float duration)
    {
        if (isServer)
        {
            return;
        }

        Buff buff = new Buff
            { duration = duration, id = buffID, remainingTime = duration, type = buffType, floatValue = floatValue };
        buffList.Add(buff);
        if (hasAuthority)
        {
            buff.iconHud =
                IconManager.Instance.GetInstance(buff, "移动速度增益", IconManager.Instance.parentBuffIcons, false);
        }
    }


    [ServerCallback]
    public void AddBuffOnServer(BuffType buffType, float floatValue, float duration)
    {
        ++BuffID;
        Buff buff = new Buff
            { duration = duration, id = BuffID, remainingTime = duration, type = buffType, floatValue = floatValue };
        buffList.Add(buff);
        if (hasAuthority)
        {
            buff.iconHud =
                IconManager.Instance.GetInstance(buff, "移动速度增益", IconManager.Instance.parentBuffIcons, false);
        }

        RefreshBuffListOnServer(buffType);
        AddBuffClientRPC(BuffID, buffType, floatValue, duration);
    }


    private void CountdownBuffList()
    {
        foreach (Buff buff in buffList)
        {
            buff.remainingTime -= Time.deltaTime;
        }

        if (!isServer)
        {
            return;
        }

        List<Buff> oldBuffList = buffList.FindAll(buff => buff.remainingTime <= 0f);
        foreach (Buff oldBuff in oldBuffList)
        {
            RemoveBuffOnServer(oldBuff);
        }
    }


    public override void Die()
    {
        base.Die();
        player.playerAppearance.Hide();
        player.playerCollider.enabled = false;
        player.playerHud.gameObject.SetActive(false);
        if (hasAuthority)
        {
            PlayerMove.IsEnabled = false;
            PlayerPlantBomb.IsEnabled = false;
            PopupManager.Instance.PlayDeathLocalPlayerEffect();
        }
    }


    private static string GetBuffDescription(Buff buff)
    {
        float floatValue = buff.floatValue;
        switch (buff.type)
        {
            case BuffType.MoveSpeedAdd:
                if (floatValue < 0f)
                {
                    return $"移动速度减少<#FF1A1A>{-floatValue:F1}</color>，持续{buff.duration:F1}秒。";
                }
                else
                {
                    return $"移动速度增加<#1AFF1A>{floatValue:F1}</color>，持续{buff.duration:F1}秒。";
                }
            case BuffType.MoveSpeedAdd_SoulJade:
                if (floatValue < 0f)
                {
                    return $"移动速度减少<#FF1A1A>{-floatValue:F1}</color>，持续{buff.duration:F1}秒。魂玉效果不会叠加。";
                }
                else
                {
                    return $"移动速度增加<#1AFF1A>{floatValue:F1}</color>，持续{buff.duration:F1}秒。魂玉效果不会叠加。";
                }
            case BuffType.MoveSpeedEql:
                return $"移动速度变为{floatValue:F1}，持续{buff.duration:F1}秒。";
            case BuffType.MoveSpeedMul:
                if (floatValue < 1f)
                {
                    return $"移动速度降低至<#FF1A1A>{floatValue:F1}</color>倍，持续{buff.duration:F1}秒。";
                }
                else
                {
                    return $"移动速度提升至<#1AFF1A>{floatValue:F1}</color>，持续{buff.duration:F1}秒。";
                }
        }

        return "无效果。";
    }


    private static string GetBuffIconName(Buff buff)
    {
        switch (buff.type)
        {
            case BuffType.MoveSpeedAdd:
            case BuffType.MoveSpeedAdd_SoulJade:
            case BuffType.MoveSpeedEql:
            case BuffType.MoveSpeedMul:
                return "移动速度增益";
        }

        return "无";
    }


    public void Initialize(Player player)
    {
        this.player = player;
    }


    [ServerCallback]
    private void RefreshBuffListOnServer(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.MoveSpeedAdd:
            case BuffType.MoveSpeedAdd_SoulJade:
            case BuffType.MoveSpeedEql:
            case BuffType.MoveSpeedMul:
                bool hasEqual = false;
                float moveSpeedAdd = 0f;
                float moveSpeedEql = float.MaxValue;
                float moveSpeedMul = 1f;
                foreach (Buff buff in buffList.FindAll(buff => buff.type == BuffType.MoveSpeedAdd))
                {
                    moveSpeedAdd += buff.floatValue;
                }

                if (buffType == BuffType.MoveSpeedAdd_SoulJade)
                {
                    List<Buff> oldBuffList = buffList.FindAll(buff => buff.type == BuffType.MoveSpeedAdd_SoulJade);
                    if (oldBuffList.Count > 0)
                    {
                        moveSpeedAdd += oldBuffList[^1].floatValue;
                    }
                }

                foreach (Buff buff in buffList.FindAll(buff => buff.type == BuffType.MoveSpeedEql))
                {
                    hasEqual = true;
                    moveSpeedEql = Mathf.Min(moveSpeedEql, buff.floatValue);
                }

                foreach (Buff buff in buffList.FindAll(buff => buff.type == BuffType.MoveSpeedMul))
                {
                    moveSpeedMul *= buff.floatValue;
                }

                moveSpeed = hasEqual ? moveSpeedEql : moveSpeedBase * moveSpeedMul + moveSpeedAdd;
                moveSpeed = Mathf.Clamp(moveSpeed, MIN_MOVE_SPEED, MAX_MOVE_SPEED);
                break;
        }
    }


    [ClientRpc]
    private void RemoveBuffClientRpc(int buffID)
    {
        if (isServer)
        {
            return;
        }

        Buff buff = buffList.Find(buff => buff.id == buffID);
        if (buff != null)
        {
            buffList.Remove(buff);
            if (hasAuthority && buff.iconHud != null)
            {
                Destroy(buff.iconHud);
            }
        }
    }


    [ServerCallback]
    private void RemoveBuffOnServer(Buff oldBuff)
    {
        buffList.Remove(oldBuff);
        if (hasAuthority && oldBuff.iconHud != null)
        {
            Destroy(oldBuff.iconHud);
        }

        RefreshBuffListOnServer(oldBuff.type);
        RemoveBuffClientRpc(oldBuff.id);
    }


    [ServerCallback]
    public void SetRemainingBombCountOnServer(int value)
    {
        remainingBombCount = Mathf.Clamp(value, 0, bombCount);
    }


    [ServerCallback]
    public void SetBombCountOnServer(int value)
    {
        bombCount = Mathf.Clamp(value, MIN_BOMB_COUNT, MAX_BOMB_COUNT);
    }


    [ServerCallback]
    public void SetBombRangeOnServer(int value)
    {
        bombRange = Mathf.Clamp(value, MIN_BOMB_RANGE, MAX_BOMB_RANGE);
    }


    [ServerCallback]
    public void SetHealthOnServer(float value)
    {
        health = Mathf.Clamp(value, 0f, MAX_HEALTH);
    }


    [ServerCallback]
    public void SetShieldOnServer(float value)
    {
        shield = Mathf.Clamp(value, 0f, shieldLevel * SHIELD_PER_LEVEL);
    }


    [ServerCallback]
    public void SetShieldLevelOnServer(int value)
    {
        shieldLevel = Mathf.Clamp(value, MIN_SHIELD_LEVEL, MAX_SHIELD_LEVEL);
    }
}