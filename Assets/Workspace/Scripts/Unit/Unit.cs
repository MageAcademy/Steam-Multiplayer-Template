using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public enum Type
    {
        Bomb,
        Hero,
        Null
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
    public static void HandleDamageOnServer(Unit source, Unit destination, float value)
    {
        float pureValue = value;
        if (destination.type == Type.Hero)
        {
            PlayerProperty prop = destination as PlayerProperty;
            float totalHealthAndShield = prop.health + prop.shield;
            if (value < 0f)
            {
                if (prop.health - value <= PlayerProperty.MAX_HEALTH)
                {
                    prop.SetHealthOnServer(prop.health - value);
                }
                else
                {
                    value += PlayerProperty.MAX_HEALTH - prop.health;
                    prop.SetHealthOnServer(PlayerProperty.MAX_HEALTH);
                    prop.SetShieldOnServer(prop.shield - value);
                }
            }
            else
            {
                if (value <= prop.shield)
                {
                    prop.SetShieldOnServer(prop.shield - value);
                    destination.TakeDamageClientRPC(pureValue, destination.transform.position,
                        destination.networkUnitName, source.networkUnitName);
                    if (destination != source)
                    {
                        source.DealDamageClientRPC(pureValue, destination.transform.position,
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
                        destination.TakeDamageClientRPC(pureValue, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                        if (destination != source)
                        {
                            source.DealDamageClientRPC(pureValue, destination.transform.position,
                                destination.networkUnitName, source.networkUnitName);
                        }
                    }
                    else
                    {
                        float trueDamage = Mathf.Min(pureValue, totalHealthAndShield);
                        prop.SetHealthOnServer(0f);
                        prop.DieOnServer();
                        destination.TakeFatalDamageClientRPC(trueDamage, destination.transform.position,
                            destination.networkUnitName, source.networkUnitName);
                        if (destination != source)
                        {
                            source.DealFatalDamageClientRPC(trueDamage, destination.transform.position,
                                destination.networkUnitName, source.networkUnitName);
                        }
                    }
                }
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
    public void DealDamageOnServer(Unit destination, float value)
    {
        HandleDamageOnServer(this, destination, value);
    }


    [ClientRpc]
    public void DealFatalDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayFatalDamageEffect(value, position, destinationName, sourceName, false, hasAuthority);
    }


    public virtual void Die()
    {
    }


    [ServerCallback]
    public virtual void DieOnServer()
    {
        if (networkIsDead)
        {
            return;
        }

        networkIsDead = true;
    }


    [ClientRpc]
    private void TakeDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayDamageEffect(value, position, destinationName, sourceName, hasAuthority, false);
    }


    [ServerCallback]
    public void TakeDamageOnServer(Unit source, float value)
    {
        HandleDamageOnServer(source, this, value);
    }


    [ClientRpc]
    private void TakeFatalDamageClientRPC(float value, Vector3 position, string destinationName, string sourceName)
    {
        PopupManager.Instance.PlayFatalDamageEffect(value, position, destinationName, sourceName, hasAuthority, false);
        PopupManager.Instance.PlayKnockDownGlobalEffect(destinationName, sourceName);
    }
}