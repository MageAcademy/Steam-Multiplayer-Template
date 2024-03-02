using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public enum DamageType
    {
        HealthOnly,
        Null,
        ShieldOnly
    }

    public enum Type
    {
        Bomb,
        Hero,
        Null,
        SafeZone
    }

    public static List<Unit> InstanceList = new List<Unit>();

    [SyncVar(hook = nameof(OnIsDeadValueChange))]
    public bool networkIsDead = false;

    public Type type = Type.Null;

    [SyncVar] public string networkUnitName = null;


    private void Start()
    {
        InstanceList.Add(this);
    }


    private void OnDestroy()
    {
        InstanceList.Remove(this);
    }


    private void OnIsDeadValueChange(bool _, bool newValue)
    {
        if (newValue)
        {
            Die();
        }
    }


    [ServerCallback]
    public static void HandleDamageOnServer(Unit source, Unit destination, float value, DamageType damageType)
    {
        float pureValue = value;
        if (destination.type == Type.Hero)
        {
            PlayerProperty prop = destination as PlayerProperty;
            float totalValue = 0f;
            if (damageType == DamageType.Null)
            {
                totalValue = prop.health + prop.shield;
            }
            else if (damageType == DamageType.HealthOnly)
            {
                totalValue = prop.health;
            }
            else if (damageType == DamageType.ShieldOnly)
            {
                totalValue = prop.shield;
            }

            float trueValue = Mathf.Min(pureValue, totalValue);
            prop.player.stat.networkTakeDamage += trueValue;
            if (source.type == Type.Hero && destination != source)
            {
                (source as PlayerProperty).player.stat.networkDealDamage += trueValue;
            }

            if (damageType == DamageType.Null)
            {
                if (value <= prop.shield)
                {
                    prop.SetShieldOnServer(prop.shield - value);
                    destination.TakeDamageClientRPC(trueValue, destination.transform.position,
                        destination.networkUnitName, source.networkUnitName);
                    if (destination != source)
                    {
                        source.DealDamageClientRPC(trueValue, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                    }
                }
                else
                {
                    value -= prop.shield;
                    prop.SetShieldOnServer(0f);
                    if (value < prop.health)
                    {
                        prop.SetHealthOnServer(prop.health - value);
                        destination.TakeDamageClientRPC(trueValue, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                        if (destination != source)
                        {
                            source.DealDamageClientRPC(trueValue, destination.transform.position,
                                destination.networkUnitName, source.networkUnitName);
                        }
                    }
                    else
                    {
                        if (source.type == Type.Hero && destination != source)
                        {
                            ++(source as PlayerProperty).player.stat.networkKillCount;
                        }

                        prop.SetHealthOnServer(0f);
                        prop.DieOnServer();
                        destination.TakeFatalDamageClientRPC(trueValue, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                        if (destination != source)
                        {
                            source.DealFatalDamageClientRPC(trueValue, destination.transform.position,
                                destination.networkUnitName, source.networkUnitName);
                            if (source.type == Type.Hero)
                            {
                                PlayerProperty sourceProp = source as PlayerProperty;
                                ++sourceProp.player.stat.networkKillCount;
                                sourceProp.SetShieldOnServer(PlayerProperty.MAX_SHIELD_LEVEL *
                                                             PlayerProperty.SHIELD_PER_LEVEL);
                                sourceProp.player.PlayRestoreShieldEffectClientRPC(sourceProp.shieldLevel);
                            }
                        }
                    }
                }
            }
            else if (damageType == DamageType.HealthOnly)
            {
                if (value < prop.health)
                {
                    prop.SetHealthOnServer(prop.health - value);
                    destination.TakeDamageClientRPC(trueValue, destination.transform.position,
                        destination.networkUnitName, source.networkUnitName);
                    if (destination != source)
                    {
                        source.DealDamageClientRPC(trueValue, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                    }
                }
                else
                {
                    if (source.type == Type.Hero && destination != source)
                    {
                        ++(source as PlayerProperty).player.stat.networkKillCount;
                    }

                    prop.SetHealthOnServer(0f);
                    prop.DieOnServer();
                    destination.TakeFatalDamageClientRPC(trueValue, destination.transform.position,
                        destination.networkUnitName, source.networkUnitName);
                    if (destination != source)
                    {
                        source.DealFatalDamageClientRPC(trueValue, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                        if (source.type == Type.Hero)
                        {
                            PlayerProperty sourceProp = source as PlayerProperty;
                            sourceProp.SetShieldOnServer(PlayerProperty.MAX_SHIELD_LEVEL *
                                                         PlayerProperty.SHIELD_PER_LEVEL);
                            sourceProp.player.PlayRestoreShieldEffectClientRPC(sourceProp.shieldLevel);
                        }
                    }
                }
            }
            else if (damageType == DamageType.ShieldOnly)
            {
                // TODO
            }
        }

        if (destination.type == Type.Bomb)
        {
            Bomb bomb = destination as Bomb;
            bomb.DieOnServer();
        }
    }


    [ClientRpc]
    public void DealDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayDamageEffect(value, position, destinationName, sourceName, false, hasAuthority);
    }


    [ServerCallback]
    public void DealDamageOnServer(Unit destination, float value, DamageType damageType)
    {
        HandleDamageOnServer(this, destination, value, damageType);
    }


    [ClientRpc]
    public void DealFatalDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayFatalDamageEffect(value, position, destinationName, sourceName, false, hasAuthority);
    }


    public virtual void Die()
    {
        if (type == Type.Hero)
        {
            PopupManager.Instance.SetAlivePlayerCount(PlayerIdentity.GetAlivePlayerCount());
        }
    }


    [ServerCallback]
    public virtual void DieOnServer()
    {
        if (networkIsDead)
        {
            return;
        }

        if (type == Type.Hero && PlayerIdentity.GetAlivePlayerCount() <= 1)
        {
            return;
        }

        networkIsDead = true;
        if (type == Type.Hero)
        {
            PlayerProperty prop = this as PlayerProperty;
            int count = PlayerIdentity.GetAlivePlayerCount();
            prop.player.stat.networkRank = count;
            PopupManager.Instance.SetAlivePlayerCount(count);
            if (count <= 1)
            {
                PlayerIdentity.Local.player.HandlePlayerStatisticsOnServerOwner();
                StartCoroutine(ResetGameOnServerAsync());
            }
        }
    }


    private IEnumerator ResetGameOnServerAsync()
    {
        yield return new WaitForSeconds(3f);
        GameManager.ResetGameOnServer();
    }


    [ClientRpc]
    private void TakeDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayDamageEffect(value, position, destinationName, sourceName, hasAuthority, false);
    }


    [ServerCallback]
    public void TakeDamageOnServer(Unit source, float value, DamageType damageType)
    {
        HandleDamageOnServer(source, this, value, damageType);
    }


    [ClientRpc]
    private void TakeFatalDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayFatalDamageEffect(value, position, destinationName, sourceName, hasAuthority, false);
        PopupManager.Instance.PlayKnockDownGlobalEffect(destinationName, sourceName);
    }
}