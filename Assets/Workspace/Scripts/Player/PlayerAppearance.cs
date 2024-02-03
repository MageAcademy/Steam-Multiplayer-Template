using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [SyncVar] public Color color = new Color();
    
    public List<MeshRenderer> meshRendererList = new List<MeshRenderer>();


    public void Initialize()
    {
        foreach (MeshRenderer meshRenderer in meshRendererList)
        {
            meshRenderer.material.color = color;
        }
    }
}