using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMVFinalRender : MonoBehaviour
{
    public Material camMat;
    [HideInInspector]
    public RenderTexture rt;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(rt, dest, camMat);
    }
}
