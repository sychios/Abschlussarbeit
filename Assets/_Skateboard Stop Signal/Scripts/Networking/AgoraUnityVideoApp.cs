using agora_gaming_rtc;
using UnityEngine;

public class AgoraUnityVideoApp
{
    // instance of agora engine
    private IRtcEngine mRtcEngine;

    public void LoadEngine(string appID)
    {
        if (mRtcEngine != null)
        {
            Debug.LogWarning("AGORA: Engine exists. Unload first.");
            return;
        }
        
        mRtcEngine = IRtcEngine.GetEngine(appID);
        
        // Enable Log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.ERROR | LOG_FILTER.WARNING | LOG_FILTER.CRITICAL);
    }

    public void JoinChannel(string channel)
    {
        if (mRtcEngine == null)
        {
            Debug.LogError("AGORA: Engine needs to be initialised before joining a channel.");
            return;
        }

        mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
        mRtcEngine.OnUserJoined = onUserJoined;
        mRtcEngine.OnUserOffline = onUserOffline;
        mRtcEngine.OnWarning = (warn, msg) =>
        {
            Debug.LogWarningFormat("AGORA: Warning code: {0} msg: {1}", warn, IRtcEngine.GetErrorDescription(warn));
        };
        mRtcEngine.OnError = HandleError;
        
        // enable video
        mRtcEngine.EnableVideo();
        
        // enable audio
        // We only want audio to be sent from observer (Windows Application) to participan (Oculus Quest)
        // TODO: Enable Audio after SetChannelProfile(...) ?
        mRtcEngine.EnableAudio();

        // set mode to live broadcasting, one host and one (or more) audience members.
        // audience can only receive audio/video while host can send and receive
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);//TODO: COMMUNICATION vs. BROADCASTING

        // used to differ between laptop and oculus in this 1-to-1 broadcast scenario.
        uint uniqueId;
        
        // disable video streaming on all platforms except WindowsPlayer (my laptop device to stream webcam)
        if (Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            uniqueId = 1;
#if !UNITY_ANDROID
            mRtcEngine.EnableLocalVideo(true);
            mRtcEngine.EnableLocalAudio(true);
#endif
        }
        else // Oculus
        {
            mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
            mRtcEngine.MuteRemoteAudioStream(1, true); // TODO: unnecessary i think? default is "false" anyway
            
            //_mRtcEngine.MuteLocalVideoStream(true); // is line affecting receiving video?
            //_mRtcEngine.EnableLocalVideo(false); // necessary?
            uniqueId = 2;
        }
        // allow camerea output callback
        mRtcEngine.EnableVideoObserver();

        mRtcEngine.JoinChannel(channel, null, uniqueId);
    }

    public string getSdkVersion()
    {
        return IRtcEngine.GetSdkVersion();
    }

    public void LeaveChannel()
    {
        Debug.Log("AGORA: Leaving Channel.");
        
        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
        // deregister video frame observers
        mRtcEngine.DisableVideoObserver();
    }

    public void UnloadEngine()
    {
        Debug.Log("AGORA: Unloading Engine.");
        
        //delete instance
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }

    public void MuteRemoteUser(bool mute)
    {
        mRtcEngine.MuteRemoteAudioStream(1, mute);
    }

    public void EnableVideo(bool pauseVideo)
    {
        if (mRtcEngine != null)
        {
            if (!pauseVideo)
            {
                mRtcEngine.EnableVideo();
            }
            else
            {
                mRtcEngine.DisableVideo();
            }
        }
    }
    
    // Engine callbacks
    private void onJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        
    }
    
    // When a remote user joined, this will be called
    // 
    private void onUserJoined(uint uid, int elapsed)
    {
        // create or find gameobject and assign it to this new user
        //VideoSurface videoSurface = makePlaneSurface(uid.ToString());

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

            if (Application.platform == RuntimePlatform.WindowsPlayer 
                || Application.platform == RuntimePlatform.OSXPlayer
                || Application.platform == RuntimePlatform.LinuxPlayer)
            {
                videoSurface.SetForUser(0);
            }
            else
            {
                videoSurface.SetForUser(uid);
            }
            videoSurface.SetEnable(true);
            //videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer); // change type to RawImage when using makeImageSurface
            videoSurface.SetGameFps(30);
        }
    }

    void AssignShader(GameObject gameObject)
    {
        Material material = Resources.Load<Material>("Materials/AgoraVideoSurfaceMaterial");
        MeshRenderer mesh = gameObject.GetComponent<MeshRenderer>();

        if (mesh != null)
        {
            mesh.material = material;
        }
    }

    public VideoSurface makePlaneSurface(string goName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        if (go == null)
            return null;

        go.name = goName;
        
        // set up transforms
        go.transform.Rotate(-80.0f, -30.0f, 0.0f);
        go.transform.position = new Vector3(145f,5.8f,115f);
        go.transform.localScale = new Vector3(0.75f, 0.75f, 0.5f);
        
        go.transform.SetParent(GameObject.Find("Skater").transform);
        
        // configure video surface
        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
    
    // when remote user is offline, this will be called
    // delete gameobject for this user? or in my case
    private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }
    
    #region Error Handling
    private int LastError { get; set; }

    private void HandleError(int error, string msg)
    {
        if (error == LastError)
            return;

        msg = string.Format("AGORA: Error code: {0} msg: {1}", error, IRtcEngine.GetErrorDescription(error));

        switch (error)
        {
            case 101:
                msg += "\nPlease make sure your AppID is valid and it does not require a certificate for this demo.";
                break;
        }
        
        Debug.LogError(msg);
    }
    
    #endregion
    
}
