using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgoraLauncher : MonoBehaviour
{
    private static AgoraUnityVideoApp app;
    public static AgoraUnityVideoApp App
    {
        get => app;
    }

    private string channelName = "sstChannel";
    
    // KEEP THIS AppID IN SAFE PLACE
    private readonly string appID = "c84674cdc53b4841a30842d1d0df6fd4";

    private PhotonView view;

    private bool _channelLeft;
    public bool ChannelLeft
    {
        get => _channelLeft;
        set => _channelLeft = value;
    }

    private void Start()
    {
        view = GetComponent<PhotonView>();

        if (!view.IsMine || Application.platform == RuntimePlatform.WindowsEditor 
                          || Application.platform == RuntimePlatform.OSXEditor
                          || Application.platform == RuntimePlatform.LinuxEditor
                          || (GameManager.Instance.Condition != "B" && SceneManager.GetActiveScene().name == "Exposure"))
        {
            Destroy(this);
            return;
        }
        
        Initialize();
    }

    public void Initialize()
    {
        CheckAppId();
    }

    private void CheckAppId()
    {
        if (string.IsNullOrEmpty(appID))
        {
            Debug.Log("AppId is undefined or not valid, please fill in a valid AppId on the AgoraController object.");
        }

        JoinChannel();
    }

    public void JoinChannel()
    {
        if (ReferenceEquals(app, null))
        {
            app = new AgoraUnityVideoApp();
            app.LoadEngine(appID);
        }

        app.JoinChannel(channelName);

        ChannelLeft = false;
    }

    public void LeaveChannel()
    {
        if (!ReferenceEquals(app, null))
        {
            app.LeaveChannel();
            app.UnloadEngine();
            app = null;
        }
        
        ChannelLeft = true;
    }

    public void Call_LeaveChannelRPC()
    {
        if (view.IsMine)
        {
            view.RPC("LeaveChannelRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    public void LeaveChannelRPC()
    {
        LeaveChannel();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!ReferenceEquals(app, null))
        {
            app.EnableVideo(pauseStatus);
        }
    }

    private void OnApplicationQuit()
    {
        if (!ReferenceEquals(app, null))
        {
            app.UnloadEngine();
        }
    }
}
