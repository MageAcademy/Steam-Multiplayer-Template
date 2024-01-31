using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static bool IsMoveValid = false;

    public static Vector3 MoveValue = new Vector3();

    public static bool IsLookValid = false;

    public static Vector3 LookValue = new Vector3();


    public void OnMove(InputAction.CallbackContext context)
    {
        IsMoveValid = !context.canceled;
        Vector2 rawValue = context.ReadValue<Vector2>();
        MoveValue = new Vector3(rawValue.x, 0f, rawValue.y);
    }


    public void OnLook(InputAction.CallbackContext context)
    {
        IsLookValid = !context.canceled;
        Vector2 rawValue = context.ReadValue<Vector2>();
        LookValue = new Vector3(rawValue.x, 0f, rawValue.y);
    }
}