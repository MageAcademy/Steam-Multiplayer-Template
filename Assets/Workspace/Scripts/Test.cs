using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            PlayerIdentity.Local?.player?.GenerateMapServerRPC();
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            GraphicsQuality.Instance.Change(1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            GraphicsQuality.Instance.Change(2);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            GraphicsQuality.Instance.Change(3);
        }
    }
}