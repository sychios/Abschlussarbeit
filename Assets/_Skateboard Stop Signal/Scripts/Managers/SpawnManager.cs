using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviourPun
{
    private GameObject gridManager;
    private Vector2[,] grid;
    
    // step between x/z-coordinates when spawning chunks
    private readonly int step = 80;
    
    private const string InstantiationSubfolder = "Chunks/Refurbished/ExposureChunks/";

    private readonly Dictionary<int, string> chunkIdToInstantiationString = new Dictionary<int, string>
    {
        {0, "campus_0"},
        {1, "campus_1"},
        {2, "campus_2"},
        {3, "residential_quarter_0"},
        {4, "residential_quarter_1"},
        {5, "residential_quarter_2"},
        {6, "residential_quarter_3"},
        {7, "industrial_district_0"},
        {8, "industrial_district_1"},
        {9, "industrial_district_2"},
        {10, "nature_0"},
        {11, "nature_1"},
        {12, "nature_2"},
        {13, "canal_0"},
        {14, "canal_1"},
        {15, "canal_2"},
        {16, "canal_3"},
        {17, "skyscraper_0"},
        {18, "skyscraper_1"},
        {19, "skyscraper_2"},
        {20, "skyscraper_3"},
        {21, "spaceport_0"},
        {22, "spaceport_1"},
        {23, "cultural_district_0"},
        {24, "cultural_district_1"},
        {25, "goldsight_ariane"},
        {26, "goldsight_haus_der_wissenschaft"},
        {27, "goldsight_mountains"},
        {28, "goldsight_old_port"},
        {29, "goldsight_city_hall"}
    };

    private void Start()
    { 
        if (Application.platform == RuntimePlatform.Android)
        {
            SpawnWorld();
        }
    }

    private void SpawnWorld()
    {
        gridManager = GameObject.Find("GridManager");
        grid = gridManager ? gridManager.GetComponent<GridManager>().GetFullGrid() : GenerateRandomMatrix();
        
        var z = 0;

        for(var i = 0; i < 5; i++, z += step)
        {
            var x = 0;
            
            for(var j = 0; j < 5; j++, x+= step)
            {
                if (i == 2 && j == 2) //ignore pizza tile
                {
                    continue;
                }
                var id = (int) grid[i, j].y;
                if (id < 0 || id >= chunkIdToInstantiationString.Count)
                    id = Random.Range(0, chunkIdToInstantiationString.Count);
                var rot = grid[i, j].x;
                if (rot % 90 != 0)
                    rot = 90f;
                // ignore x:160, z:160
                PhotonNetwork.Instantiate(InstantiationSubfolder+chunkIdToInstantiationString[id], new Vector3(x, 0, z), Quaternion.Euler(0, rot, 0));
            }
        }
    }
    
    
    // Generating matrix as example of user creation
    private Vector2[,] GenerateRandomMatrix()
    {
        Vector2[,] matrix = new Vector2[5, 5];

        float[] rots = {90f, 180f, 270f};

        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                float id = Random.Range(0, chunkIdToInstantiationString.Count);
                var rot = Random.Range(0, 3);
                
                matrix[i, j] = new Vector2(rots[rot], id);
            }
        }

        return matrix;

    }
}
