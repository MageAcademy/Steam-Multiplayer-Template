#pragma warning disable 108

using NVIDIA;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class GraphicsQuality : MonoBehaviour
{
    public static GraphicsQuality Instance = null;

    private const string KEY_GRAPHICS_QUALITY = "GraphicsQuality";

    public PostProcessLayer layer = null;

    public Light light = null;

    public Reflex reflex = null;

    public PostProcessVolume volume = null;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        Change(PlayerPrefs.HasKey(KEY_GRAPHICS_QUALITY) ? PlayerPrefs.GetInt(KEY_GRAPHICS_QUALITY) : 1);
    }


    public void Change(int quality)
    {
        PlayerPrefs.SetInt(KEY_GRAPHICS_QUALITY, quality);
        Application.targetFrameRate = 120;
        layer.antialiasingMode = quality > 1
            ? PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing
            : PostProcessLayer.Antialiasing.None;
        layer.enabled = quality > 1;
        layer.subpixelMorphologicalAntialiasing.quality = quality > 2
            ? SubpixelMorphologicalAntialiasing.Quality.High
            : SubpixelMorphologicalAntialiasing.Quality.Low;
        light.shadows = quality > 1 ? LightShadows.Soft : LightShadows.None;
        reflex.enabled = quality > 1;
        volume.enabled = quality > 2;
        switch (quality)
        {
            case 1:
                light.shadowResolution = LightShadowResolution.Low;
                break;
            case 2:
                light.shadowResolution = LightShadowResolution.High;
                break;
            case 3:
                light.shadowResolution = LightShadowResolution.VeryHigh;
                break;
        }
    }
}