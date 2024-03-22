using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Outline : MonoBehaviour
{
    public Color outlineColor = Color.red;

    public float outlineHardness = 0.8f;

    public float outlineWidth = 0.2f;

    public List<Renderer> rendererList = new List<Renderer>();

    private Camera m_Camera = null;

    private CommandBuffer m_CommandBuffer = null;

    private RenderTexture m_Mask = null;

    private RenderTexture m_Outline = null;

    private Material m_OutlineMaterial = null;


    private void Start()
    {
        Initialize();
        Draw();
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        m_OutlineMaterial.SetFloat("_OutlineWidth", outlineWidth * 10f);
        m_OutlineMaterial.SetFloat("_OutlineHardness", 8.99f * (1f - outlineHardness) + 0.01f);
        m_OutlineMaterial.SetColor("_OutlineColor", outlineColor);
        m_OutlineMaterial.SetTexture("_Mask", m_Mask);
        Graphics.Blit(source, m_Outline, m_OutlineMaterial, 0);
        m_OutlineMaterial.SetTexture("_Outline", m_Outline);
        Graphics.Blit(source, destination, m_OutlineMaterial, 1);
    }


    private void Initialize()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.depthTextureMode = DepthTextureMode.None;
        m_OutlineMaterial = new Material(Shader.Find("Outline/PostprocessOutline"));
        m_Mask = new RenderTexture(m_Camera.pixelWidth, m_Camera.pixelHeight, 0, RenderTextureFormat.R8);
        m_Outline = new RenderTexture(m_Camera.pixelWidth, m_Camera.pixelHeight, 0, RenderTextureFormat.R8);
        m_CommandBuffer = new CommandBuffer { name = "Outline Command Buffer" };
        m_CommandBuffer.SetRenderTarget(m_Mask);
        m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_CommandBuffer);
    }


    public void Clear()
    {
        m_CommandBuffer.ClearRenderTarget(true, true, Color.black);
        Graphics.ExecuteCommandBuffer(m_CommandBuffer);
        m_CommandBuffer.Clear();
    }


    public void Draw()
    {
        m_CommandBuffer.SetRenderTarget(m_Mask);
        m_CommandBuffer.ClearRenderTarget(true, true, Color.black);
        for (int a = 0; a < rendererList.Count; ++a)
        {
            m_CommandBuffer.DrawRenderer(rendererList[a], new Material(Shader.Find("Outline/Target")));
            Graphics.ExecuteCommandBuffer(m_CommandBuffer);
        }
    }
}