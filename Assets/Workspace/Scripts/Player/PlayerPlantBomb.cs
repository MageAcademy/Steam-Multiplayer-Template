using Mirror;
using UnityEngine;

public class PlayerPlantBomb : NetworkBehaviour
{
    public static bool IsEnabled = false;

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
        bomb.networkUnitName = "炸弹";
        bomb.InitializeOnServer(count, duration, player);
        if (PlayerIdentity.Local != null && PlayerIdentity.Local.player != null)
        {
            PlayerIdentity.Local.player.PlayAudioClientRPCLocalPlayerOnly("炸弹安放", bomb.transform.position);
        }

        NetworkServer.Spawn(bomb.gameObject);
    }
}