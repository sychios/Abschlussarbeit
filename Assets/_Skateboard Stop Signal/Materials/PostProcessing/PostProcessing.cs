using UnityEngine;
using UnityEngine.XR;


[RequireComponent(typeof(Camera))]
public class PostProcessing : MonoBehaviour
{
    public Material postProcessingMat;
    
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        var desc = XRSettings.enabled ? XRSettings.eyeTextureDesc : new RenderTextureDescriptor(Screen.width, Screen.height);

        RenderTexture rt = RenderTexture.GetTemporary(desc);
        Graphics.Blit(src, rt, postProcessingMat, 0);
        Graphics.Blit(rt, dest, postProcessingMat, 1);
        RenderTexture.ReleaseTemporary(rt);
        
        //Graphics.Blit(src, dest, postProcessingMat);
    }
}
