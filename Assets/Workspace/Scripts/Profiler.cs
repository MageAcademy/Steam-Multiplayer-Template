using Mirror;
using TMPro;
using UnityEngine;

public class Profiler : MonoBehaviour
{
    public int targetFrameRate = 120;

    public TextMeshProUGUI textProfiler = null;

    private int frameCount = 0;


    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        InvokeRepeating(nameof(Refresh), 1f, 1f);
    }


    private void Update()
    {
        ++frameCount;
    }

    private void Refresh()
    {
        string message = $"{frameCount} FPS";
        frameCount = 0;
        if (NetworkClient.isConnected)
        {
            message += $"\n{(int)(NetworkTime.rtt * 500.0)} ms";
        }

        textProfiler.text = message;
    }
}