using Mirror;
using UnityEngine;

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

    public Type type = Type.Null;


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
    }


    [ServerCallback]
    public void DieOnServer()
    {
        Debug.LogError($"[{type}] Die!");
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