using System.Linq;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridElement : MonoBehaviour
{

    public Material DefaultMaterial;
    public Material HighlightMaterial;

    public PhysicsPointer PlayerPhysicsPointer { get; set; }

    private MeshRenderer meshRenderer;

    private GameObject chunk;

    public bool IsSight { get; private set; }

    public bool IsChunkSet { get; private set; }

    private GridManager gridManager;

    private readonly int[] rotations = {0, 90, 180, 270};
    
    
    // Starting from here are the variables for refurbished chunk prefabs
    private readonly Vector3 localChunkScale = new Vector3(1.075f, 1.075f, 1.075f);
    private readonly Vector3 localChunkScaleEdgeCase = new Vector3(1.075f, 0.5375f, 1.075f);
    private readonly Vector3 chunkPositionDefaultOffset = new Vector3(0, 2.2f, 0);
    private readonly Vector3 chunkPositionHighlightOffset = new Vector3(0, 3.5f, 0);
    
    private readonly string[] sightChunks = {
        "goldsight_ariane",
        "goldsight_haus_der_wissenschaft",
        "goldsight_mountains",
        "goldsight_old_port",
        "goldsight_city_hall" 
    };
    
    private string refurbishedChunksPrefabsFolder = "Chunks/Refurbished/";

    private readonly string[] refurbishedChunks =
    {
        "campus_0",
        "campus_1",
        "campus_2",
        "residential_quarter_0",
        "residential_quarter_1",
        "residential_quarter_2",
        "residential_quarter_3",
        "industrial_district_0",
        "industrial_district_1",
        "industrial_district_2",
        "nature_0",
        "nature_1",
        "nature_2",
        "canal_0",
        "canal_1",
        "canal_2",
        "canal_3",
        "skyscraper_0",
        "skyscraper_1",
        "skyscraper_2",
        "skyscraper_3",
        "spaceport_0",
        "spaceport_1",
        "cultural_district_0",
        "cultural_district_1",
        "goldsight_ariane",
        "goldsight_haus_der_wissenschaft",
        "goldsight_mountains",
        "goldsight_old_port",
        "goldsight_city_hall"
    };

    private readonly string[] scaleEdgeCases = // prefabs which have a default scale of (1, 0.5, 1) instead of (1, 1, 1)
    {
        "cultural_district_0",
        "cultural_district_1",
        "goldsight_ariane",
        "skyscraper_0",
        "skyscraper_1",
        "skyscraper_2",
        "skyscraper_3",
        "spaceport_0",
        "spaceport_1",
        "canal_1"
    };

    // Start is called before the first frame update
    void Start()
    {
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = DefaultMaterial;

        IsChunkSet = false;
    }

    public void SetChunk(string chunkID)
    {
        if (!refurbishedChunks.Contains(chunkID)) return;

        var matrixId = GetMatrixIDFromName();

        var logAction = "ADD#";

        var rot = RandomRotation();
        if (IsChunkSet)
        {
            if (IsSight)
                IsSight = false;
            PhotonNetwork.Destroy(chunk);
            logAction = "REPL#";
        }
        else
        {
            IsChunkSet = true;
        }

        if (sightChunks.Contains(chunkID))
        {
            IsSight = true;
        }
        
        chunk = PhotonNetwork.InstantiateRoomObject(refurbishedChunksPrefabsFolder+chunkID, transform.position + chunkPositionDefaultOffset, Quaternion.Euler(0f, rot, 0f));

        chunk.transform.localScale = scaleEdgeCases.Contains(chunkID) ? localChunkScaleEdgeCase : localChunkScale;
        
        CSVWriter.Instance.AddEntryToCreation(logAction + chunkID, matrixId.ToString());
        
        CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().ActiveControllerCanvasGameObject.SetActive(false);
        gridManager.SetGridField((int) matrixId.x, (int) matrixId.y, chunkID, rot);
        
        
        PlayerPhysicsPointer.DeselectGridElement();
    }

    private int RandomRotation()
    {
        return rotations[Random.Range(0, rotations.Length)];
    }


    public void DeleteChunk()
    {
        if (!IsChunkSet) return;
        
        PhotonNetwork.Destroy(chunk);

        if (IsSight)
            IsSight = false;
        
        IsChunkSet = false;

        Vector2 matrixId = GetMatrixIDFromName();

        gridManager.DeleteGridField((int) matrixId.x, (int) matrixId.y);
        
        CSVWriter.Instance.AddEntryToCreation("DEL", matrixId.ToString());
    }

    public void RotateChunk(float direction)
    {
        if (!IsChunkSet) return;

        float yRotation = 90 * direction;
        
        chunk.transform.Rotate(new Vector3(0, yRotation, 0));
            
        Vector2 matrixId = GetMatrixIDFromName();

        gridManager.UpdateRotation((int) matrixId.x, (int) matrixId.y,
                (int)chunk.transform.rotation.y + (int)yRotation);

        string dir = direction < 0 ? "l" : "r"; 
        CSVWriter.Instance.AddEntryToCreation("ROT#" + dir, matrixId.ToString());
    }

    public void Select()
    {
        CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().ActiveControllerCanvasGameObject.SetActive(true);
        gridManager.currentGridElement = this;
    }

    public void Touch()
    {
        meshRenderer.material = HighlightMaterial;

        if (!IsChunkSet) return;
        
        chunk.transform.position = transform.position + chunkPositionHighlightOffset;
    }

    public void UnTouch()
    {
        meshRenderer.material = DefaultMaterial;
        
        if (!IsChunkSet) return;

        chunk.transform.position = transform.position + chunkPositionDefaultOffset;
    }

    private Vector2 GetMatrixIDFromName()
    {
        var id = gameObject.name.Split('#')[1].Split('_');
        var x = id[0];
        var y = id[1];
        return new Vector2(int.Parse(x), int.Parse(y));
    }
}
