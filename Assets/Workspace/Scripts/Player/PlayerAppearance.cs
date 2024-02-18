using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [SyncVar] public Color networkColor = new Color();
    
    public List<MeshRenderer> meshRendererList = new List<MeshRenderer>();


    public void Initialize()
    {
        foreach (MeshRenderer meshRenderer in meshRendererList)
        {
            meshRenderer.material.color = networkColor;
        }
    }
}