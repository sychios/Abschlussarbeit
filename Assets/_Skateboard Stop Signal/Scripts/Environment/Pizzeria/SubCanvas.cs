using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCanvas : MonoBehaviour
{
    private HandCanvas handCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        handCanvas = gameObject.GetComponent<HandCanvas>();
    }

    public void SetChunk(string id)
    {
        handCanvas.SetChunk(id);
    }
}
