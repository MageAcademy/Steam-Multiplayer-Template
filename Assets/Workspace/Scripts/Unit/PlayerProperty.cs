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

    public class Buff
    {
        public float duration = 0f;

        public float remainingTime = 0f;

        public BuffType type = BuffType.Null;

        public object value = null;
    }

    public const int MAX_BOMB_COUNT = 5;

    public const int MAX_BOMB_RANGE = 5;

    public const float MAX_HEALTH = 1000f;

    public const float MAX_MOVE_SPEED = 10f;

    public const int MAX_SHIELD_LEVEL = 6;

    public const int MIN_BOMB_COUNT = 1;

    public const int MIN_BOMB_RANGE = 1;

    public const float MIN_MOVE_SPEED = 0f;

    public const int MIN_SHIELD_LEVEL = 2;

    public const float SHIELD_PER_LEVEL = 250f;

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
        CountdownBuffListOnServer();
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


    [ServerCallback]
    public void AddBuffOnServer(BuffType buffType, object value, float duration)
    {
        buffList.Add(new Buff { duration = duration, remainingTime = duration, type = buffType, value = value });
        RefreshBuffListOnServer(buffType);
    }


    [ServerCallback]
    private void CountdownBuffListOnServer()
    {
        List<Buff> oldBuffList = new List<Buff>();
        foreach (Buff buff in buffList)
        {
            buff.remainingTime -= Time.deltaTime;
            if (buff.remainingTime <= 0f)
            {
                oldBuffList.Add(buff);
            }
        }

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
        }
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
                    moveSpeedAdd += (float)buff.value;
                }

                if (buffType == BuffType.MoveSpeedAdd_SoulJade)
                {
                    List<Buff> oldBuffList = buffList.FindAll(buff => buff.type == BuffType.MoveSpeedAdd_SoulJade);
                    if (oldBuffList.Count > 0)
                    {
                        moveSpeedAdd += (float)oldBuffList[^1].value;
                    }
                }

                foreach (Buff buff in buffList.FindAll(buff => buff.type == BuffType.MoveSpeedEql))
                {
                    hasEqual = true;
                    moveSpeedEql = Mathf.Min(moveSpeedEql, (float)buff.value);
                }

                foreach (Buff buff in buffList.FindAll(buff => buff.type == BuffType.MoveSpeedMul))
                {
                    moveSpeedMul *= (float)buff.value;
                }

                moveSpeed = hasEqual ? moveSpeedEql : moveSpeedBase * moveSpeedMul + moveSpeedAdd;
                moveSpeed = Mathf.Clamp(moveSpeed, MIN_MOVE_SPEED, MAX_MOVE_SPEED);
                break;
        }
    }


    [ServerCallback]
    private void RemoveBuffOnServer(Buff oldBuff)
    {
        buffList.Remove(oldBuff);
        RefreshBuffListOnServer(oldBuff.type);
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