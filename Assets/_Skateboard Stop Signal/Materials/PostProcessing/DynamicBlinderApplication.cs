using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicBlinderApplication : MonoBehaviour
{
    // Start is called before the first frame update

    public Material mat;
    private Camera _cam;
    private static readonly int LeftEyeToWorld = Shader.PropertyToID("_LeftEyeToWorld");
    private static readonly int RightEyeToWorld = Shader.PropertyToID("_RightEyeToWorld");
    private static readonly int LeftEyeProjection = Shader.PropertyToID("_LeftEyeProjection");
    private static readonly int RightEyeProjection = Shader.PropertyToID("_RightEyeProjection");

    void Start()
    {
        _cam = GetComponent<Camera>();
    }
        
    private void OnPreRender()
    {
        if (_cam.stereoEnabled) {
            Matrix4x4 leftToWorld = _cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
            Matrix4x4 rightToWorld = _cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
            
            mat.SetMatrix(LeftEyeToWorld, leftToWorld);
            mat.SetMatrix(RightEyeToWorld, rightToWorld);

            Matrix4x4 leftEye = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left); // ohne .inverse??
            Matrix4x4 rightEye = _cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            // Compensate for RenderTexture
            leftEye = GL.GetGPUProjectionMatrix(leftEye, true).inverse; // .inverse??
            rightEye = GL.GetGPUProjectionMatrix(rightEye, true).inverse;
            
            // Negate [1,1] to reflect Unity's CBuffer state
            leftEye[1, 1] *= -1;
            rightEye[1, 1] *= -1;

            

            mat.SetMatrix(LeftEyeProjection, leftEye);
            mat.SetMatrix(RightEyeProjection, rightEye);

        }
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
