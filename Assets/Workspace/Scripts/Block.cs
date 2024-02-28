using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector2Int coordinate = new Vector2Int();

    public bool isDestructible = false;

    public MeshRenderer meshRenderer = null;


    public void Initialize()
    {
        meshRenderer.material.color = isDestructible ? Color.yellow : Color.white;
        transform.position = MapManager.Instance.GetPositionOnFloor(coordinate);
    }
}