using agora_gaming_rtc;
using UnityEngine;
using Object = UnityEngine.Object;

public class AgoraQuestInterface : MonoBehaviour
{
    [SerializeField] private string appId = "c84674cdc53b4841a30842d1d0df6fd4";
    [SerializeField] private string roomName = "sstChannel";
    
    private bool connected;

    private static AgoraQuestInterface _agoraInstance;
    public static AgoraQuestInterface Instance => _agoraInstance;
    
    // instance of agora engine
    public IRtcEngine mRtcEngine;

    private void Awake()
    {
        if (_agoraInstance != null && _agoraInstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _agoraInstance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!connected)
        {
            connected = true;
            loadEngine();
        }

        join(roomName);
    }

    public void Leave()
    {
        if (connected)
        {
            leave();
            unloadEngine();
            connected = false;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (IRtcEngine.QueryEngine() != null)
            {
                IRtcEngine.QueryEngine().DisableVideo();
            }
        }
        else
        {
            if (IRtcEngine.QueryEngine() != null)
            {
                IRtcEngine.QueryEngine().EnableVideo();
            }
        }
    }

    private void OnApplicationQuit()
    {
        IRtcEngine.Destroy();
    }

    public void loadEngine()
    {
        if (mRtcEngine != null) {
            Debug.Log ("Engine exists. Please unload it first!");
            return;
        }

        // init engine
        mRtcEngine = IRtcEngine.getEngine (appId);

        // enable log
        mRtcEngine.SetLogFilter (LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
    }
    
    public void unloadEngine()
    {
        // delete
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }
    
    public void join(string channel)
    {
        if (mRtcEngine == null) 
            return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
        mRtcEngine.OnUserJoined = onUserJoined;
        mRtcEngine.OnUserOffline = onUserOffline;

        // enable video
        mRtcEngine.EnableVideo();

        // allow camera output callback
        mRtcEngine.EnableVideoObserver();

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0);
    }
    
    public void leave()
    {
        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
        // deregister video frame observers in native-c code
        mRtcEngine.DisableVideoObserver();
    }
    
    public string getSdkVersion()
    {
        return IRtcEngine.GetSdkVersion();
    }

    private void onJoinChannelSuccess (string channelName, uint uid, int elapsed)
    {
        Debug.Log ("JoinChannelSuccessHandler: uid = " + uid);
        //GameObject textVersionGameObject = GameObject.Find ("VersionText");
        //DebugQuest.Instance.Log(textVersionGameObject.GetComponent<Text> ().text = "Version : " + getSdkVersion ());
    }
    
    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    private void onUserJoined(uint uid, int elapsed)
    {
        VideoSurface videoSurface;
        
        GameObject display = GameObject.FindWithTag("Display");
        if (ReferenceEquals(display, null))
        {
            display = GameObject.Find("AssistantScreen");
            videoSurface = display.AddComponent<VideoSurface>();
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
        }
        else
        {
            videoSurface = display.GetComponent<VideoSurface>();
        }
        
        AssignShader(display);
        
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure video surface
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                videoSurface.SetForUser(0);
            }
            else
            {
                videoSurface.SetForUser(uid);
            }
            videoSurface.SetEnable(true);
            //videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer); // change type to RawImage when using makeImageSurface
            videoSurface.SetGameFps(20);
        }
    }
    
    void AssignShader(GameObject obj)
    {
        Material material = Resources.Load<Material>("Materials/AgoraVideoSurfaceMaterial");
        MeshRenderer mesh = obj.GetComponent<MeshRenderer>();

        if (mesh != null)
        {
            mesh.material = material;
        }
    }
    
    private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }
    
    
}
