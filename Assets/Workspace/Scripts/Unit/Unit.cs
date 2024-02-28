using System.Collections.Generic;
using Mirror;

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
                    destination.TakeDamageClientRPC(pureValue);
                }
                else
                {
                    value -= prop.shield;
                    prop.SetShieldOnServer(0f);
                    if (value < prop.health)
                    {
                        prop.SetHealthOnServer(prop.health - value);
                        destination.TakeDamageClientRPC(pureValue);
                    }
                    else
                    {
                        prop.SetHealthOnServer(0f);
                        prop.DieOnServer();
                        destination.TakeFatalDamageClientRPC(pureValue);
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


    [ServerCallback]
    public void DealDamageOnServer(Unit destination, float value)
    {
        HandleDamageOnServer(this, destination, value);
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
    private void TakeDamageClientRPC(float value)
    {
        PopupManager.Instance.PlayTakeDamageEffect(value, transform.position, hasAuthority);
    }


    [ServerCallback]
    public void TakeDamageOnServer(Unit source, float value)
    {
        HandleDamageOnServer(source, this, value);
    }


    [ClientRpc]
    private void TakeFatalDamageClientRPC(float value)
    {
        PopupManager.Instance.PlayTakeFatalDamageEffect(value, transform.position, hasAuthority);
    }
}