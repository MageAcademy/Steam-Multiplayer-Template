#pragma warning disable 108

using Mirror;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public static bool IsEnabled = false;

    [Range(0f, 10f)] public float speed = 4f;

    private bool isInitialized = false;

    private Player player = null;

    private Rigidbody rigidbody = null;


    private void Update()
    {
        Move();
    }


    private void FixedUpdate()
    {
        ClearAngularVelocity();
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
    public void TeleportClientRPC(Vector3 position)
    {
        if (!hasAuthority)
        {
            return;
        }

        transform.position = position;
    }
}