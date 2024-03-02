using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public AudioClip[] clips = null;

        public string name = null;
    }

    public static AudioManager Instance = null;

    public AudioListener audioListener = null;

    public List<Data> dataList = null;

    public AudioSource prefab = null;


    private void Awake()
    {
        Instance = this;
    }


    public AudioSource Play(string name, Transform parent = null, Vector3 position = default, float duration = -1f,
        ulong delay = 0L)
    {
        Data data = dataList.Find(data => data.name == name);
        if (data == null)
        {
            return null;
        }

        int randomIndex = Random.Range(0, data.clips.Length);
        AudioSource audioSource = Instantiate(prefab, parent);
        audioSource.transform.localPosition = position;
        AudioClip clip = data.clips[randomIndex];
        audioSource.clip = clip;
        audioSource.PlayDelayed(delay);
        if (duration < 0f)
        {
            duration = clip.length;
        }

        Destroy(audioSource.gameObject, duration);
        return audioSource;
    }
}