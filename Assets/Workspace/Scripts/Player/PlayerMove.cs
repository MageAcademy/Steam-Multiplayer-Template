#pragma warning disable 108

using Mirror;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public static bool IsEnabled = false;

    [SyncVar] public Vector2Int networkCoordinate = new Vector2Int(-1, -1);

    private bool isInitialized = false;

    private Player player = null;

    private Rigidbody rigidbody = null;


    private void Update()
    {
        RefreshCoordinateOnServer();
        RefreshPositionYOnLocalPlayer();
    }


    private void FixedUpdate()
    {
        ClearAngularVelocityOnLocalPlayer();
        MoveOnLocalPlayer();
    }


    private void ClearAngularVelocityOnLocalPlayer()
    {
        if (!hasAuthority || !isInitialized)
        {
            return;
        }

        rigidbody.angularVelocity = Vector3.zero;
    }


    public void Initialize(Player player)
    {
        isInitialized = true;
        this.player = player;
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = !hasAuthority;
    }


    private void MoveOnLocalPlayer()
    {
        if (!IsEnabled || !hasAuthority || !isInitialized)
        {
            return;
        }

        Vector3 direction = InputManager.MoveValue;
        if (InputManager.IsMoveValid)
        {
            rigidbody.velocity = direction * player.prop.moveSpeed;
            transform.forward = direction;
        }
        else
        {
            rigidbody.velocity = Vector3.zero;
        }
    }


    [ClientRpc]
    public void TeleportClientRPC(Vector3 position, bool requireAuthority)
    {
        if (requireAuthority && !hasAuthority)
        {
            return;
        }

        transform.position = position;
    }


    [ServerCallback]
    private void RefreshCoordinateOnServer()
    {
        if (!isInitialized || !isServer)
        {
            return;
        }

        networkCoordinate = MapManager.Instance.GetCoordinateByPosition(transform.position);
    }


    private void RefreshPositionYOnLocalPlayer()
    {
        if (!hasAuthority || !isInitialized)
        {
            return;
        }

        transform.position = MapManager.Instance.GetPositionOnFloor(transform.position);
    }
}