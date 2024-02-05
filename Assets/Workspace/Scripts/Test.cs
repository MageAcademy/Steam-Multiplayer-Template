using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayerIdentity.Local.player.GenerateMapServerRPC();
        }
    }
}