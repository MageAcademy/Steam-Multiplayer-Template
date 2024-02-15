#pragma warning disable 108

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Bomb : Unit
{
    public static List<Bomb> InstanceList = new List<Bomb>();

    public Vector2Int coordinate = new Vector2Int();

    public LayerMask layerPlayer = new LayerMask();

    private Collider collider = null;

    private int count = 0; // server only

    private float duration = 0f; // server only

    private Player player = null; // server only


    private void Start()
    {
        collider = GetComponent<Collider>();
    }


    private void Update()
    {
        CheckPlayer();
        CountdownOnServer();
    }


    private void CheckPlayer()
    {
        Collider[] hitInfo = Physics.OverlapSphere(transform.position, 0.5f, layerPlayer);
        if (hitInfo.Length > 0)
        {
            return;
        }

        collider.isTrigger = false;
    }


    [ServerCallback]
    private void CountdownOnServer()
    {
        if (!isServer)
        {
            return;
        }

        duration -= Time.deltaTime;
        if (duration < 0f)
        {
            ExplodeOnServer();
        }
    }


    [ServerCallback]
    public override void DieOnServer()
    {
        base.DieOnServer();
        FinalizeOnServer();
    }


    [ServerCallback]
    private void ExplodeOnServer()
    {
        TakeDamageOnServer(null, 0f);
    }


    [ServerCallback]
    public void FinalizeOnServer()
    {
        InstanceList.Remove(this);
        player.prop.SetRemainingBombCountOnServer(player.prop.remainingBombCount + count);
        Destroy(gameObject);
        NetworkServer.UnSpawn(gameObject);
    }


    [ServerCallback]
    public void InitializeOnServer(int count, float duration, Player player)
    {
        InstanceList.Add(this);
        this.count = count;
        this.duration = duration;
        this.player = player;
        player.prop.SetRemainingBombCountOnServer(player.prop.remainingBombCount - count);
    }
}