using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public class Cell
    {
        public int blockIndex = 0;

        public Type type = Type.Null;

        public int x = -1;

        public int y = -1;
    }

    public class Data
    {
        public Cell[,] cells = null;

        public int height = 0;

        public Vector3 safeZoneEndCenter = new Vector3();

        public float safeZoneStartScale = 0f;

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

    public static MapManager Instance = null;

    public float cellSize = 1f;

    public Color colorHighlightedGrid = new Color(1f, 1f, 1f, 100f / 255f);

    public Color colorNormalGrid = new Color(0f, 0f, 0f, 10f / 255f);

    public Data data = null;

    public LayerMask layerFloor = new LayerMask();

    public Transform parentEnvironment = null;

    public Transform parentGrids = null;

    public Block[] prefabsBlock = null;

    public GameObject prefabCube = null;

    public GameObject prefabEnvironment = null;

    public GameObject prefabImage = null;

    private GameObject backgroundMusic = null;

    private Block[,] blocks = null;

    private GameObject environment = null;

    private Image[,] grids = null;

    private bool isLastValid = false;

    private Vector2Int lastCoordinate = new Vector2Int();


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        RefreshGrid();
    }


    public void ClearOnClient()
    {
        GameManager.InGame = false;
        SafeZone.Instance.Hide();
        if (backgroundMusic != null)
        {
            Destroy(backgroundMusic);
            backgroundMusic = null;
        }

        if (environment != null)
        {
            Destroy(environment);
            environment = null;
        }

        blocks = null;
        data = null;
        grids = null;
        isLastValid = false;
        lastCoordinate = new Vector2Int();
        List<GameObject> list = new List<GameObject>();
        for (int a = 0; a < parentEnvironment.childCount; ++a)
        {
            list.Add(parentEnvironment.GetChild(a).gameObject);
        }

        for (int a = 0; a < parentGrids.childCount; ++a)
        {
            list.Add(parentGrids.GetChild(a).gameObject);
        }

        for (int a = 0; a < list.Count; ++a)
        {
            Destroy(list[a]);
        }
    }


    private void CreateBlock(int x, int y, int blockIndex)
    {
        Block block = Instantiate(prefabsBlock[blockIndex], parentEnvironment);
        block.coordinate = new Vector2Int(x, y);
        block.transform.position = GetPositionOnFloor(x, y);
        blocks[x, y] = block;
    }


    private void CreateGrid(int x, int y)
    {
        Transform grid = Instantiate(prefabImage, parentGrids).transform;
        grid.position = GetPositionByCoordinate(x, y) + new Vector3(0f, 0.01f, 0f);
        grid.rotation = Quaternion.Euler(90f, 0f, 0f);
        grid.localScale = new Vector3(0.9f, 0.9f, 1f) * cellSize;
        grids[x, y] = grid.GetComponent<Image>();
        grids[x, y].color = colorNormalGrid;
    }


    public void DestroyBlock(Vector2Int coordinate)
    {
        int x = coordinate.x;
        int y = coordinate.y;
        if (!IsCoordinateValid(x, y))
        {
            return;
        }

        Block block = blocks[x, y];
        if (block == null)
        {
            return;
        }

        Destroy(block.gameObject);
        SetCell(x, y, Type.EmptyInside);
    }


    public void GenerateOnClient(string json)
    {
        GameManager.InGame = true;
        PopupManager.Instance.HideStatisticsPanel();
        SafeZone.Instance.Show();
        AudioSource audioSource =
            AudioManager.Instance.Play("背景音乐", AudioManager.Instance.audioListener.transform);
        audioSource.volume = 0.4f;
        backgroundMusic = audioSource.gameObject;
        environment = Instantiate(prefabEnvironment);
        data = JsonConvert.DeserializeObject<Data>(json);
        float playerRadius = 0.2f;
        float wallThickness = 20f;
        Vector3 floorSize = new Vector3((data.width + 2) * cellSize, 1f, (data.height + 2) * cellSize);
        Transform wall = Instantiate(prefabCube, parentEnvironment).transform;
        wall.gameObject.name = "Bottom Wall";
        wall.position = new Vector3(0f, 0f, -floorSize.z / 2f - playerRadius - wallThickness / 2f);
        wall.localScale = new Vector3(floorSize.x + playerRadius * 2f + wallThickness * 2f, cellSize * 2f,
            wallThickness);
        wall.GetComponent<MeshRenderer>().material.color = new Color(0.4f, 0.2f, 0f, 1f);
        wall = Instantiate(prefabCube, parentEnvironment).transform;
        wall.gameObject.name = "Top Wall";
        wall.position = new Vector3(0f, 0f, +floorSize.z / 2f + playerRadius + wallThickness / 2f);
        wall.localScale = new Vector3(floorSize.x + playerRadius * 2f + wallThickness * 2f, cellSize * 2f,
            wallThickness);
        wall.GetComponent<MeshRenderer>().material.color = new Color(0.4f, 0.2f, 0f, 1f);
        wall = Instantiate(prefabCube, parentEnvironment).transform;
        wall.gameObject.name = "Left Wall";
        wall.position = new Vector3(-floorSize.x / 2f - playerRadius - wallThickness / 2f, 0f, 0f);
        wall.localScale =
            new Vector3(wallThickness, cellSize * 2f, floorSize.z + playerRadius * 2f + wallThickness * 2f);
        wall.GetComponent<MeshRenderer>().material.color = new Color(0.4f, 0.2f, 0f, 1f);
        wall = Instantiate(prefabCube, parentEnvironment).transform;
        wall.gameObject.name = "Right Wall";
        wall.position = new Vector3(floorSize.x / 2f + playerRadius + wallThickness / 2f, 0f, 0f);
        wall.localScale =
            new Vector3(wallThickness, cellSize * 2f, floorSize.z + playerRadius * 2f + wallThickness * 2f);
        wall.GetComponent<MeshRenderer>().material.color = new Color(0.4f, 0.2f, 0f, 1f);
        blocks = new Block[data.width, data.height];
        grids = new Image[data.width, data.height];
        for (int x = 0; x < data.width; ++x)
        {
            for (int y = 0; y < data.height; ++y)
            {
                Cell cell = data.cells[x, y];
                switch (cell.type)
                {
                    case Type.BlockDestructible:
                        CreateBlock(x, y, cell.blockIndex);
                        CreateGrid(x, y);
                        break;
                    case Type.BlockIndestructible:
                        CreateBlock(x, y, cell.blockIndex);
                        break;
                    case Type.EmptyInside:
                        CreateGrid(x, y);
                        break;
                }
            }
        }
    }


    public string GenerateOnServer(int width, int height)
    {
        data = new Data
        {
            cells = new Cell[width, height],
            height = height,
            width = width
        };
        data.safeZoneEndCenter =
            GetPositionByCoordinate(new Vector2Int(Random.Range(0, width), Random.Range(0, height)));
        data.safeZoneStartScale = (Mathf.Max(height, width) + 4f) * cellSize * Mathf.Sqrt(2f);
        RandomManager.IntType[] probabilities =
        {
            new RandomManager.IntType { value = 0, weight = 90f },
            new RandomManager.IntType { value = 1, weight = 10f },
            new RandomManager.IntType { value = 2, weight = 100f },
            new RandomManager.IntType { value = 3, weight = 200f }
        };
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int randomValue = RandomManager.Get(probabilities).value;
                data.cells[x, y] = new Cell
                {
                    blockIndex = randomValue,
                    x = x,
                    y = y
                };
                if (randomValue < 2)
                {
                    data.cells[x, y].type = Type.BlockDestructible;
                }
                else if (randomValue < 3)
                {
                    data.cells[x, y].type = Type.BlockIndestructible;
                }
                else if (randomValue == 3)
                {
                    data.cells[x, y].type = Type.EmptyInside;
                }
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
            playerList[a].player.playerMove.TeleportClientRPC(GetPositionByCoordinate(coordinate), false);
            SetCell(coordinate.x - 1, coordinate.y, Type.EmptyInside);
            SetCell(coordinate.x, coordinate.y - 1, Type.EmptyInside);
            SetCell(coordinate.x, coordinate.y, Type.EmptyInside);
            SetCell(coordinate.x, coordinate.y + 1, Type.EmptyInside);
            SetCell(coordinate.x + 1, coordinate.y, Type.EmptyInside);
        }

        return JsonConvert.SerializeObject(data, Formatting.None, new ConverterVector3());
    }


    public Block GetBlock(int x, int y)
    {
        return blocks[x, y];
    }


    public Block GetBlock(Vector2Int coordinate)
    {
        return GetBlock(coordinate.x, coordinate.y);
    }


    public Type GetCell(int x, int y)
    {
        return IsCoordinateValid(x, y) ? data.cells[x, y].type : Type.Null;
    }


    public Type GetCell(Vector2Int coordinate)
    {
        return GetCell(coordinate.x, coordinate.y);
    }


    public Vector2Int GetCoordinateByPosition(Vector3 position)
    {
        return data == null
            ? new Vector2Int(-1, -1)
            : new Vector2Int(Mathf.FloorToInt((position.x + data.width * cellSize / 2f) / cellSize),
                Mathf.FloorToInt((position.z + data.height * cellSize / 2f) / cellSize));
    }


    public Vector3 GetPositionByCoordinate(int x, int y)
    {
        return data == null
            ? Vector3.zero
            : new Vector3(x - data.width / 2f + 0.5f, 0f, y - data.height / 2f + 0.5f) * cellSize;
    }


    public Vector3 GetPositionByCoordinate(Vector2Int coordinate)
    {
        return GetPositionByCoordinate(coordinate.x, coordinate.y);
    }


    public Vector3 GetPositionOnFloor(int x, int y)
    {
        return GetPositionOnFloor(GetPositionByCoordinate(x, y));
    }


    public Vector3 GetPositionOnFloor(Vector2Int coordinate)
    {
        return GetPositionOnFloor(coordinate.x, coordinate.y);
    }


    public Vector3 GetPositionOnFloor(Vector3 position)
    {
        position.y = 100f;
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hitInfo, 200f, layerFloor))
        {
            position = hitInfo.point;
        }
        else
        {
            position.y = 0f;
        }

        return position;
    }


    public bool IsCoordinateValid(int x, int y)
    {
        return data != null && x >= 0 && x < data.width && y >= 0 && y < data.height;
    }


    public bool IsCoordinateValid(Vector2Int coordinate)
    {
        return IsCoordinateValid(coordinate.x, coordinate.y);
    }


    public bool IsPositionValid(Vector3 position)
    {
        return IsCoordinateValid(GetCoordinateByPosition(position));
    }


    private void RefreshGrid()
    {
        if (PlayerIdentity.Local == null || PlayerIdentity.Local.player == null)
        {
            if (isLastValid)
            {
                grids[lastCoordinate.x, lastCoordinate.y].color = colorNormalGrid;
                isLastValid = false;
            }

            return;
        }

        Vector2Int currentCoordinate = PlayerIdentity.Local.player.playerMove.networkCoordinate;
        bool isCurrentValid = IsCoordinateValid(currentCoordinate);
        Type cellType = isCurrentValid ? data.cells[currentCoordinate.x, currentCoordinate.y].type : Type.Null;
        isCurrentValid = isCurrentValid && (cellType == Type.BlockDestructible || cellType == Type.EmptyInside);
        if (isCurrentValid)
        {
            if (isLastValid)
            {
                if (currentCoordinate != lastCoordinate)
                {
                    grids[currentCoordinate.x, currentCoordinate.y].color = colorHighlightedGrid;
                    grids[lastCoordinate.x, lastCoordinate.y].color = colorNormalGrid;
                }
            }
            else
            {
                grids[currentCoordinate.x, currentCoordinate.y].color = colorHighlightedGrid;
            }
        }
        else
        {
            if (isLastValid)
            {
                grids[lastCoordinate.x, lastCoordinate.y].color = colorNormalGrid;
            }
        }

        isLastValid = isCurrentValid;
        lastCoordinate = currentCoordinate;
    }


    public void SetCell(int x, int y, Type type)
    {
        if (!IsCoordinateValid(x, y))
        {
            return;
        }

        data.cells[x, y].type = type;
    }
}