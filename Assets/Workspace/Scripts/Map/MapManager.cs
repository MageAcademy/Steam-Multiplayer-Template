using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    #region
    
    public class Cell
    {
        public Type type = Type.Null;

        public int x = -1;

        public int y = -1;
    }

    public class Data
    {
        public Cell[,] cells = null;

        public int height = 0;

        public int width = 0;
    }

    public enum Type
    {
        Null = 0,
        BlockDestructible = 1,
        BlockIndestructible = 2,
        EmptyInside = 3,
        EmptyOutside = 4
    }
    
    #endregion

    public static MapManager Instance = null;

    public float cellSize = 1f;

    public Transform parentGrids = null;

    public GameObject prefabCube = null;

    public GameObject prefabImage = null;

    private Data data = null;


    private void Awake()
    {
        Instance = this;
    }


    public void GenerateOnClient(string json)
    {
        data = JsonConvert.DeserializeObject<Data>(json);
        Debug.LogError(json);
    }


    public string GenerateOnServer(int width, int height)
    {
        data = new Data
        {
            cells = new Cell[width, height],
            height = height,
            width = width
        };
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                data.cells[x, y] = new Cell
                {
                    type = (Type)Random.Range(1, 5),
                    x = x,
                    y = y
                };
            }
        }

        List<PlayerIdentity> playerList = PlayerIdentity.InstanceList;
        float maxRadian = Mathf.PI * 2;
        int playerCount = playerList.Count;
        float deltaRadian = maxRadian / playerCount;
        float startRadian = Random.Range(0f, maxRadian);
        float radius = (Mathf.Min(height, width) - 0.5f) * cellSize / 2f;
        for (int a = 0; a < playerCount; ++a)
        {
            float radian = startRadian + deltaRadian * a;
            Vector2Int coordinate =
                GetCoordinateByPosition(new Vector3(Mathf.Cos(radian), 0f, Mathf.Sin(radian)) * radius);
            playerList[a].player.playerMove.TeleportClientRPC(GetPositionByCoordinate(coordinate));
            SetCell(coordinate.x - 1, coordinate.y, Type.EmptyInside);
            SetCell(coordinate.x, coordinate.y - 1, Type.EmptyInside);
            SetCell(coordinate.x, coordinate.y, Type.EmptyInside);
            SetCell(coordinate.x, coordinate.y + 1, Type.EmptyInside);
            SetCell(coordinate.x + 1, coordinate.y, Type.EmptyInside);
        }

        return JsonConvert.SerializeObject(data, Formatting.None);
    }


    #region

    public Type GetCell(int x, int y)
    {
        return IsCoordinateValid(x, y) ? data.cells[x, y].type : Type.Null;
    }


    public Vector2Int GetCoordinateByPosition(Vector3 position)
    {
        return new Vector2Int((int)((position.x + data.width * cellSize / 2f) / cellSize),
            (int)((position.z + data.height * cellSize / 2f) / cellSize));
    }


    public Vector3 GetPositionByCoordinate(int x, int y)
    {
        return new Vector3(x - data.width / 2f + 0.5f, 0f, y - data.height / 2f + 0.5f) * cellSize;
    }


    public Vector3 GetPositionByCoordinate(Vector2Int coordinate)
    {
        return GetPositionByCoordinate(coordinate.x, coordinate.y);
    }


    public bool IsCoordinateValid(int x, int y)
    {
        return x >= 0 && x < data.width && y >= 0 && y < data.height;
    }


    public bool IsCoordinateValid(Vector2Int coordinate)
    {
        return IsCoordinateValid(coordinate.x, coordinate.y);
    }


    public bool IsPositionValid(Vector3 position)
    {
        return IsCoordinateValid(GetCoordinateByPosition(position));
    }


    public void SetCell(int x, int y, Type type)
    {
        if (!IsCoordinateValid(x, y))
        {
            return;
        }

        data.cells[x, y].type = type;
    }

    #endregion
}