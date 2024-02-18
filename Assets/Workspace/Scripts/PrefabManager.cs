using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PrefabManager : MonoBehaviour
{
    public static Dictionary<string, PrefabEntry> PrefabMap = new Dictionary<string, PrefabEntry>();

    public List<PrefabEntry> prefabEntryList = new List<PrefabEntry>();


    private void Awake()
    {
        PrefabMap.Clear();
        foreach (PrefabEntry prefabEntry in prefabEntryList)
        {
            prefabEntry.Initialize(this);
            PrefabMap.Add(prefabEntry.prefab.name, prefabEntry);
        }
    }


    public void Release(float delay, ObjectPool<GameObject> pool, GameObject element)
    {
        StartCoroutine(ReleaseAsync(delay, pool, element));
    }


    private IEnumerator ReleaseAsync(float delay, ObjectPool<GameObject> pool, GameObject element)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(element);
    }
}