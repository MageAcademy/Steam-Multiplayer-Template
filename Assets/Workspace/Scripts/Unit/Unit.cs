using System.Collections.Generic;
using Mirror;

public class Unit : NetworkBehaviour
{
    public enum Type
    {
        BlockDestructible,
        BlockIndestructible,
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
        if (destination.type == Type.Hero)
        {
            PlayerProperty prop = destination as PlayerProperty;
            if (value <= prop.shield)
            {
                prop.SetShieldOnServer(prop.shield - value);
            }
            else
            {
                value -= prop.shield;
                prop.SetShieldOnServer(0f);
                if (value < prop.health)
                {
                    prop.SetHealthOnServer(prop.health - value);
                }
                else
                {
                    prop.SetHealthOnServer(0f);
                    prop.DieOnServer();
                }
            }
        }

        if (destination.type == Type.Bomb)
        {
            Bomb bomb = destination as Bomb;
            bomb.DieOnServer();
        }
    }


    public virtual void Die()
    {
        print($"{type} Die");
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


    [ServerCallback]
    public void DealDamageOnServer(Unit destination, float value)
    {
        HandleDamageOnServer(this, destination, value);
    }


    [ServerCallback]
    public void TakeDamageOnServer(Unit source, float value)
    {
        HandleDamageOnServer(source, this, value);
    }
}