using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    private const int GridLength = 5;
    private Vector2[,] grid;

    
    [SerializeField]
    private  GameObject[] chunks;

    private CreationCanvas creationCanvas;

    public GridElement currentGridElement;

    public GameObject questMarker;
    
    private Dictionary<string, int> chunkIdStringToInt = new Dictionary<string, int>
    {
        {"campus_0",0},
        {"campus_1",1},
        {"campus_2",2},
        {"residential_quarter_0",3},
        {"residential_quarter_1",4},
        {"residential_quarter_2",5},
        {"residential_quarter_3",6},
        {"industrial_district_0",7},
        {"industrial_district_1",8},
        {"industrial_district_2",9},
        {"nature_0",10},
        {"nature_1",11},
        {"nature_2",12},
        {"canal_0",13},
        {"canal_1",14},
        {"canal_2",15},
        {"canal_3",16},
        {"skyscraper_0",17},
        {"skyscraper_1",18},
        {"skyscraper_2",19},
        {"skyscraper_3",20},
        {"spaceport_0",21},
        {"spaceport_1",22},
        {"cultural_district_0",23},
        {"cultural_district_1",24},
        {"goldsight_ariane", 25},
        {"goldsight_haus_der_wissenschaft", 26},
        {"goldsight_mountains", 27},
        {"goldsight_old_port", 28},
        {"goldsight_city_hall", 29}
    };

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        grid = new Vector2[GridLength,GridLength];
        for (var i = 0; i < grid.GetLength(0); i++)
        {
            for (var j = 0; j < grid.GetLength(1); j++)
            {
                grid[i,j] = new Vector2(-1,-1);
            }
        }

        creationCanvas = GameObject.Find("CreationCanvas").GetComponent<CreationCanvas>();
    }

    public void SetChunk(string id)
    {
        currentGridElement.SetChunk(id);
    }

    /// <summary>
    /// Set chunk of id to cell grid[x,y] with Rotation of y-axis with yRotation
    /// </summary>
    /// <param name="x">x-parameter of 2D-Matrix grid</param>
    /// <param name="y">y-parameter of 2D-Matrix grid</param>
    /// <param name="id">ID of the chunk to be placed on this grid field.</param>
    /// <param name="yRotation">Rotation of the y-axis, z- and x-axis are zero always.</param>
    /// <returns></returns>
    public void SetGridField(int x, int y, string id, int yRotation)
    {
        if (x < 0 || x >= grid.Length || y < 0 || y >= grid.Length) return;
        
        grid[x,y] = new Vector2(yRotation, chunkIdStringToInt[id]);

        //_chunkInstances[CalculateIndex(x,y)] = PhotonNetwork.Instantiate(_chunks[id], new Vector3(x * _step, 0, y * _step), Quaternion.Euler(0, yRotation, 0));
        //_chunkInstances[CalculateIndex(x,y)].transform.localScale = new Vector3(_scaleFactor,_scaleFactor,_scaleFactor);
        
        creationCanvas.IsTowNFinished(IsGridFull());
    }

    public void UpdateRotation(int x, int y, int yRotation)
    {
        if (x < 0 || x >= grid.Length || y < 0 || y >= grid.Length) return;

        //_chunkInstances[CalculateIndex(x,y)].transform.rotation = Quaternion.Euler(0, yRotation, 0);
        
        grid[x, y].x = yRotation;
    }

    public void DeleteGridField(int x, int y)
    {
        if (x < 0 || x >= grid.Length || y < 0 || y >= grid.Length) return;

        //PhotonNetwork.Destroy(_chunkInstances[CalculateIndex(x,y)]);
        
        grid[x, y] = new Vector2(-1,-1);
        creationCanvas.IsTowNFinished(IsGridFull());
    }

    private bool IsGridFull()
    {
        questMarker.GetComponent<QuestMarkerController>().AnchorPosition = new Vector3(-167.5f, 55f, 43f);
        foreach (var chunk in chunks)
        {
            var elem = chunk.GetComponent<GridElement>();
            if (!elem.IsChunkSet)
            {
                return false;
            }
        }
        
        questMarker.GetComponent<QuestMarkerController>().AnchorPosition = new Vector3(-88f, 56f, 137f);
        return true;
    }
    
    public Vector2[,] GetFullGrid()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i,j].y < 0)
                {
                    grid[i, j].y = Random.Range(0, 25);
                }
                
            }
        }
        
        return grid;
    }

    public string GetFullGridLogString()
    {
        string logString = "";
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                // foreach:
                // id, yRotation, gridCoordinate #
                logString += grid[i, j].y + "_" + grid[i, j].x + "_" + i + "," + j + "#";
            }
        }

        return logString;
    }
}
