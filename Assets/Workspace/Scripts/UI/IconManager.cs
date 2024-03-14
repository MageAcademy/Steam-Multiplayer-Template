using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    public class Data
    {
        public int id = 0;

        public string key = null;

        public string name = null;
    }

    public static IconManager Instance = null;

    public TextAsset json = null;

    public Transform parentBuffIcons = null;

    public Transform parentSkillIcons = null;

    public IconHud prefab = null;

    private AssetBundle bundle = null;

    private Dictionary<string, string> iconMap = new Dictionary<string, string>();


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "icon"));
        Data[] data = JsonConvert.DeserializeObject<Data[]>(json.text);
        foreach (Data element in data)
        {
            iconMap[element.name] = element.key;
        }
    }


    public GameObject GetInstance(Describable target, string iconName, Transform parent, bool isIncreased)
    {
        if (bundle == null)
        {
            return null;
        }

        string iconKey = iconMap[iconName];
        if (iconKey == null)
        {
            return null;
        }

        IconHud hud = Instantiate(prefab, parent);
        hud.imageBackground.sprite = bundle.LoadAsset<Sprite>(iconKey + "_desaturated");
        hud.imageForeground.sprite = bundle.LoadAsset<Sprite>(iconKey);
        hud.imageForeground.fillAmount = isIncreased ? 0f : 1f;
        hud.imageForeground.fillClockwise = isIncreased;
        hud.isIncreased = isIncreased;
        hud.target = target;
        hud.text.text = "";
        return hud.gameObject;
    }
}