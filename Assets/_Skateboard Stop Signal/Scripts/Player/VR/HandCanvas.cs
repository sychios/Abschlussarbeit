using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCanvas : MonoBehaviour
{
    public PhysicsPointer physicsPointer;
    
    private GridManager gridManager;

    [SerializeField] private Canvas spacePortCanvas; 
    [SerializeField] private Canvas campusCanvas; 
    [SerializeField] private Canvas industryCanvas; 
    [SerializeField] private Canvas residentialQuarterCanvas; 
    [SerializeField] private Canvas canalCanvas; 
    [SerializeField] private Canvas natureCanvas; 
    [SerializeField] private Canvas culturalCanvas; 
    [SerializeField] private Canvas skyscraperCanvas;
    [SerializeField] private Canvas sightsCanvas;

    private Canvas currentSubCanvas;

    [SerializeField] private Button[] buttons;

    [SerializeField] private EventSystem eventSystem;
    
    // Start is called before the first frame update
    void Start()
    {
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
    }

    public void SetChunk(string id)
    {
        gridManager.SetChunk(id);
        Hide();
    }

    public void Hide()
    {
        if (currentSubCanvas)
        {
            currentSubCanvas.gameObject.SetActive(false);
            currentSubCanvas = null;
        }
        
        physicsPointer.DeselectGridElement();

        eventSystem.SetSelectedGameObject(null);
        gameObject.SetActive(false);
    }

    public void ShowSubCanvas(int id)
    {
        if(currentSubCanvas)
            currentSubCanvas.gameObject.SetActive(false);
        switch (id)
        {
            case 0: // industrial
                industryCanvas.gameObject.SetActive(true);
                currentSubCanvas = industryCanvas;
                break;
            case 1: // residential
                residentialQuarterCanvas.gameObject.SetActive(true);
                currentSubCanvas = residentialQuarterCanvas;
                break;
            case 2: // cultural
                culturalCanvas.gameObject.SetActive(true);
                currentSubCanvas = culturalCanvas;
                break;
            case 3: // skyscraper
                skyscraperCanvas.gameObject.SetActive(true);
                currentSubCanvas = skyscraperCanvas;
                break;
            case 4: // nature
                natureCanvas.gameObject.SetActive(true);
                currentSubCanvas = natureCanvas;
                break;
            case 5: // canal
                canalCanvas.gameObject.SetActive(true);
                currentSubCanvas = canalCanvas;
                break;
            case 6: // campus
                campusCanvas.gameObject.SetActive(true);
                currentSubCanvas = campusCanvas;
                break;
            case 7: // space
                spacePortCanvas.gameObject.SetActive(true);
                currentSubCanvas = spacePortCanvas;
                break;
            case 8: // sights
                sightsCanvas.gameObject.SetActive(true);
                currentSubCanvas = sightsCanvas;
                break;
        }
    }
}
