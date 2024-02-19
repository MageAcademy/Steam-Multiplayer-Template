using Mirror;
using UnityEngine;

public class PlayerPlantBomb : NetworkBehaviour
{
    private Player player = null;

    private GameObject prefabBomb = null;


    public void Initialize(Player player)
    {
        this.player = player;
        prefabBomb = NetworkManager.singleton.spawnPrefabs.Find(gameObject => gameObject.name == "Bomb");
    }


    [Command(requiresAuthority = false)]
    public void PlantBombServerRPC(int count, float duration)
    {
        if (player.prop.remainingBombCount < count)
        {
            return;
        }

        Vector2Int coordinate = player.playerMove.networkCoordinate;
        if (!MapManager.Instance.IsCoordinateValid(coordinate))
        {
            return;
        }

        MapManager.Type cellType = MapManager.Instance.GetCell(coordinate);
        if (cellType != MapManager.Type.EmptyInside)
        {
            return;
        }

        if (Bomb.InstanceMap.ContainsKey(coordinate))
        {
            return;
        }

        Bomb bomb = Instantiate(prefabBomb).GetComponent<Bomb>();
        bomb.coordinate = coordinate;
        bomb.transform.position = MapManager.Instance.GetPositionOnFloor(coordinate);
        bomb.InitializeOnServer(count, duration, player);
        NetworkServer.Spawn(bomb.gameObject);
    }
}