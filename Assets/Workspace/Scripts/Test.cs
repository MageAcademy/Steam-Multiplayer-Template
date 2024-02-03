using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        //InvokeRepeating(nameof(Teleport), 3f, 3f);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            PlayerIdentity.Local.player.GenerateMapServerRPC();
        }
    }


    private void Teleport()
    {
        if (PlayerIdentity.Local == null)
        {
            return;
        }

        PlayerIdentity.Local.player.transform.position = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
    }
}