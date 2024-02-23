using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
    }


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Keypad0))
        {
            GameManager.ResetGameOnServer();
        }
    }
}