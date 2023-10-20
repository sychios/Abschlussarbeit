using Photon.Pun;

/**
 * Class holding basic information persistently
 */
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private string _participantID;
    public string ParticipantID
    {
        get => _participantID;
        set => _participantID = value;
    }

    private string _condition;
    public string Condition
    {
        get => _condition;
        set => _condition = value;
    }

    public enum Languages
    {
        Deutsch,
        English
    }
    
    private Languages _language;

    public Languages Language
    {
        get => _language;
        set => _language = value;
    }

    private bool _controllersSwitched;
    public bool ControllersSwitched
    {
        get => _controllersSwitched;
        set => _controllersSwitched = value;
    }

    private void Awake()
    {
        CreateInstance();
    }


    private void CreateInstance()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(this);
        }
    }
    

    //So we stop loading scenes if we quit app
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}
