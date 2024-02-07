#pragma warning disable 108

using Mirror;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public static bool IsEnabled = false;

    public LayerMask layerFloor = new LayerMask();

    [SyncVar] public Vector2Int networkCoordinate = new Vector2Int(-1, -1);

    [Range(0f, 10f)] public float speed = 4f;

    private bool isInitialized = false;

    private Player player = null;

    private Rigidbody rigidbody = null;


    private void Update()
    {
        RefreshCoordinateOnServer();
        RefreshPositionY();
    }


    private void FixedUpdate()
    {
        ClearAngularVelocity();
        Move();
    }


    private void ClearAngularVelocity()
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


    private void Move()
    {
        if (!IsEnabled || !hasAuthority || !isInitialized)
        {
            return;
        }

        Vector3 direction = InputManager.MoveValue;
        if (InputManager.IsMoveValid)
        {
            rigidbody.velocity = direction * speed;
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


    private void RefreshPositionY()
    {
        Vector3 position = transform.position;
        position.y += 10f;
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hitInfo, 100f, layerFloor))
        {
            transform.position = hitInfo.point;
        }
        else
        {
            position = transform.position;
            position.y = 0f;
            transform.position = position;
        }
    }
}