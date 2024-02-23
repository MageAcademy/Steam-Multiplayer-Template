using Mirror;
using Newtonsoft.Json;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public class Data
    {
        public int id = 0;

        public string name = null;

        public int quality = 0;

        public float weight = 0f;
    }

    public static LootManager Instance = null;

    public Color[] colorQuality = null;

    public Data[] data = null;

    public TextAsset json = null;

    public Transform parentLoot = null;

    private LootEntry prefab = null;

    private RandomManager.IntType[] probabilities = null;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        Initialize();
    }


    public LootEntry GetRandomInstance(int id = -1)
    {
        if (id == -1)
        {
            id = RandomManager.Get(probabilities).value;
        }

        if (id < 2)
        {
            return null;
        }

        LootEntry entry = Instantiate(prefab, parentLoot);
        entry.networkID = id;
        return entry;
    }


    private void Initialize()
    {
        data = JsonConvert.DeserializeObject<Data[]>(json.text);
        prefab = NetworkManager.singleton.spawnPrefabs.Find(gameObject => gameObject.name == "Loot")
            .GetComponent<LootEntry>();
        probabilities = new RandomManager.IntType[data.Length];
        for (int a = 0; a < data.Length; ++a)
        {
            probabilities[a] = new RandomManager.IntType { value = data[a].id, weight = data[a].weight };
        }
    }
}