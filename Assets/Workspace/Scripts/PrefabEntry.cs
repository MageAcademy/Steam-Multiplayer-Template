using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

[Serializable]
public class PrefabEntry
{
    public bool autoRelease = false;

    public float autoReleaseDelay = 0f;

    public ObjectPool<GameObject> pool = null;

    public GameObject prefab = null;

    private PrefabManager manager = null;


    public void Initialize(PrefabManager manager)
    {
        this.manager = manager;
        pool = new ObjectPool<GameObject>(Create, Get, Release, UDestroy, true, 10, 20);
    }


    private GameObject Create()
    {
        return Object.Instantiate(prefab, manager.transform);
    }


    private void Get(GameObject element)
    {
        element.SetActive(true);
        if (autoRelease)
        {
            manager.Release(autoReleaseDelay, pool, element);
        }
    }


    private void Release(GameObject element)
    {
        element.SetActive(false);
    }


    private void UDestroy(GameObject element)
    {
        Object.Destroy(element);
    }
}