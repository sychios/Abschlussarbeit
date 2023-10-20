using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFader : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private float fadingDistanceZ = 50f; // Distance from which to start fading the canvas
    [SerializeField] private float fadingDistanceX = 50f; // Distance from which to start fading the canvas
    
    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            this.enabled = false;
        }
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_cam) return;
        Vector3 viewPos = _cam.WorldToViewportPoint(transform.position);
        if (viewPos.z < fadingDistanceZ || viewPos.x < fadingDistanceZ)
        {
            float fadeOutZ = (viewPos.z - fadingDistanceZ * 1.5f) * -1 / (fadingDistanceZ / 2);
            float fadeOutX = (viewPos.z - fadingDistanceX * 1.5f) * -1 / (fadingDistanceX / 2);
            if (fadeOutZ < 0 || fadeOutX < 0)
            {
                this.gameObject.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            }
            else
            {
                float fade;
                float dif;
                if (fadeOutX > fadeOutZ)
                {
                    dif = fadeOutX - fadeOutZ;
                    dif /= 2;
                    fade = fadeOutZ + dif;
                } else if (fadeOutZ > fadeOutX)
                {
                    dif = fadeOutZ - fadeOutX;
                    dif /= 2;
                    fade = fadeOutX + dif;
                }
                else
                {
                    fade = fadeOutZ;
                }
                this.gameObject.GetComponent<CanvasRenderer>().SetAlpha(fade);
            }
        }
        else
        {
            this.gameObject.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        }
    }
}
