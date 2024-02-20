using Mirror;
using UnityEngine;

public class PlayerProperty : Unit
{
    public const int MAX_BOMB_COUNT = 5;
    
    public const int MAX_BOMB_RANGE = 5;

    public const float MAX_HEALTH = 1000f;

    public const int MAX_SHIELD_LEVEL = 6;

    public const int MIN_BOMB_COUNT = 1;
    
    public const int MIN_BOMB_RANGE = 1;

    public const int MIN_SHIELD_LEVEL = 2;

    public const float SHIELD_PER_LEVEL = 250f;

    [SyncVar(hook = nameof(OnRemainingBombCountChange))]
    public int remainingBombCount = MIN_BOMB_COUNT;

    [SyncVar(hook = nameof(OnBombCountChange))]
    public int bombCount = MIN_BOMB_COUNT;

    [SyncVar] public int bombRange = 1;

    [SyncVar(hook = nameof(OnHealthChange))]
    public float health = MAX_HEALTH;

    [SyncVar] public float moveSpeed = 4f;

    public Player player = null;

    [SyncVar(hook = nameof(OnShieldChange))]
    public float shield = MIN_SHIELD_LEVEL * SHIELD_PER_LEVEL;

    [SyncVar(hook = nameof(OnShieldLevelChange))]
    public int shieldLevel = MIN_SHIELD_LEVEL;


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


    public void Initialize(Player player)
    {
        this.player = player;
    }


    [ServerCallback]
    public void SetRemainingBombCountOnServer(int value)
    {
        if (!isServer)
        {
            return;
        }

        remainingBombCount = Mathf.Clamp(value, 0, bombCount);
    }


    [ServerCallback]
    public void SetBombCountOnServer(int value)
    {
        if (!isServer)
        {
            return;
        }

        bombCount = Mathf.Clamp(value, MIN_BOMB_COUNT, MAX_BOMB_COUNT);
    }


    [ServerCallback]
    public void SetBombRangeOnServer(int value)
    {
        if (!isServer)
        {
            return;
        }

        bombRange = Mathf.Clamp(value, MIN_BOMB_RANGE, MAX_BOMB_RANGE);
    }


    [ServerCallback]
    public void SetHealthOnServer(float value)
    {
        if (!isServer)
        {
            return;
        }

        health = Mathf.Clamp(value, 0f, MAX_HEALTH);
    }


    [ServerCallback]
    public void SetShieldOnServer(float value)
    {
        if (!isServer)
        {
            return;
        }

        shield = Mathf.Clamp(value, 0f, shieldLevel * SHIELD_PER_LEVEL);
    }


    [ServerCallback]
    public void SetShieldLevelOnServer(int value)
    {
        if (!isServer)
        {
            return;
        }

        shieldLevel = Mathf.Clamp(value, MIN_SHIELD_LEVEL, MAX_SHIELD_LEVEL);
    }
}