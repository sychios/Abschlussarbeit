using UnityEngine;

public class Border : MonoBehaviour
{
    public GameObject borderWall;
    
    //public GameObject[] borderCanvases;

    public void ShowBorder()
    {
        borderWall.SetActive(true);
    }
}
