#pragma warning disable 108

using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Bomb : Unit
{
    public class Info
    {
        public Vector2Int coordinate = new Vector2Int();

        public Player damageSource = null;
    }

    public static List<Info> InfoList = new List<Info>(); // server owner only

    public static Dictionary<Vector2Int, Bomb> InstanceMap = new Dictionary<Vector2Int, Bomb>(); // server only

    public Vector2Int coordinate = new Vector2Int(); // server only

    public Player damageSource = null; // server only

    public LayerMask layerPlayer = new LayerMask();

    private Collider collider = null;

    private int count = 0; // server only

    private float duration = 0f; // server only

    private Player player = null; // server only

    private int range = 0;


    private void Start()
    {
        InstanceList.Add(this);
        collider = GetComponent<Collider>();
    }


    private void Update()
    {
        CheckPlayer();
        CountdownOnServer();
    }


    private void AddInfoOnServer(Vector2Int coordinate)
    {
        if (InfoList.Exists(info => info.coordinate == coordinate))
        {
            return;
        }

        InfoList.Add(new Info { coordinate = coordinate, damageSource = damageSource });
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
        duration -= Time.deltaTime;
        if (duration < 0f)
        {
            damageSource = player;
            ExplodeOnServer();
        }
    }


    public override void Die()
    {
        base.Die();
        Destroy(gameObject);
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
        TakeDamageOnServer(damageSource.prop, 0f);
        AddInfoOnServer(coordinate);
        MapManager.Type cellType = MapManager.Type.Null;
        bool[] flags = new bool[4]; // left, right, back, front
        Vector2Int newCoordinate = new Vector2Int();
        for (int a = 1; a <= range; ++a)
        {
            newCoordinate = new Vector2Int(coordinate.x - a, coordinate.y);
            cellType = MapManager.Instance.GetCell(newCoordinate);
            if (!flags[0])
            {
                if (InstanceMap.TryGetValue(newCoordinate, out Bomb bomb0))
                {
                    bomb0.damageSource = damageSource;
                    bomb0.ExplodeOnServer();
                    flags[0] = true;
                }
                else if (cellType == MapManager.Type.BlockDestructible)
                {
                    flags[0] = true;
                    AddInfoOnServer(newCoordinate);
                }
                else if (cellType == MapManager.Type.BlockIndestructible)
                {
                    flags[0] = true;
                }
                else
                {
                    AddInfoOnServer(newCoordinate);
                }
            }

            newCoordinate = new Vector2Int(coordinate.x + a, coordinate.y);
            cellType = MapManager.Instance.GetCell(newCoordinate);
            if (!flags[1])
            {
                if (InstanceMap.TryGetValue(newCoordinate, out Bomb bomb1))
                {
                    bomb1.damageSource = damageSource;
                    bomb1.ExplodeOnServer();
                    flags[1] = true;
                }
                else if (cellType == MapManager.Type.BlockDestructible)
                {
                    flags[1] = true;
                    AddInfoOnServer(newCoordinate);
                }
                else if (cellType == MapManager.Type.BlockIndestructible)
                {
                    flags[1] = true;
                }
                else
                {
                    AddInfoOnServer(newCoordinate);
                }
            }

            newCoordinate = new Vector2Int(coordinate.x, coordinate.y - a);
            cellType = MapManager.Instance.GetCell(newCoordinate);
            if (!flags[2])
            {
                if (InstanceMap.TryGetValue(newCoordinate, out Bomb bomb2))
                {
                    bomb2.damageSource = damageSource;
                    bomb2.ExplodeOnServer();
                    flags[2] = true;
                }
                else if (cellType == MapManager.Type.BlockDestructible)
                {
                    flags[2] = true;
                    AddInfoOnServer(newCoordinate);
                }
                else if (cellType == MapManager.Type.BlockIndestructible)
                {
                    flags[2] = true;
                }
                else
                {
                    AddInfoOnServer(newCoordinate);
                }
            }

            newCoordinate = new Vector2Int(coordinate.x, coordinate.y + a);
            cellType = MapManager.Instance.GetCell(newCoordinate);
            if (!flags[3])
            {
                if (InstanceMap.TryGetValue(newCoordinate, out Bomb bomb3))
                {
                    bomb3.damageSource = damageSource;
                    bomb3.ExplodeOnServer();
                    flags[3] = true;
                }
                else if (cellType == MapManager.Type.BlockDestructible)
                {
                    flags[3] = true;
                    AddInfoOnServer(newCoordinate);
                }
                else if (cellType == MapManager.Type.BlockIndestructible)
                {
                    flags[3] = true;
                }
                else
                {
                    AddInfoOnServer(newCoordinate);
                }
            }
        }
    }


    [ServerCallback]
    public void FinalizeOnServer()
    {
        InstanceMap.Remove(coordinate);
        player.prop.SetRemainingBombCountOnServer(player.prop.remainingBombCount + count);
    }


    [ServerCallback]
    public void InitializeOnServer(int count, float duration, Player player)
    {
        InstanceMap.Add(coordinate, this);
        this.count = count;
        this.duration = duration;
        this.player = player;
        player.prop.SetRemainingBombCountOnServer(player.prop.remainingBombCount - count);
        range = player.prop.bombRange;
    }
}