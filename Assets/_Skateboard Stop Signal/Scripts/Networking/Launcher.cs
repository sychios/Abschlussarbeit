using System;
using System.Collections;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private bool singlePlayer;
    
    public GameObject ovrPlayerController;
    public GameObject localCamera;
    public GameObject canvasGameObject;

    public GameObject pizzeriaLabelCanvas;
    
    [SerializeField] private TMP_Dropdown conditionDropdown;
    [SerializeField] private InputField participantIdInput;
    [SerializeField] private TMP_Text parseResult;
    
    private Hashtable roomProperties = new Hashtable();

    private const string RoomName = "sst_study";

    private bool triesToConnectToMaster;
    private bool triesToConnectToRoom;
    private bool connectedToRoom;
    
    private readonly RoomOptions _roomOptions = new RoomOptions
    {
        MaxPlayers = 2
    };

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Destroy(localCamera);
            Destroy(canvasGameObject);
            ovrPlayerController.SetActive(true);
        }
        else
        {
            Destroy(ovrPlayerController);
            localCamera.SetActive(true);
        }
    }
    
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Allow master-client to load level with PhotonNetwork.LoadLevel
    }

    private void Update()
    {
        if (!PhotonNetwork.IsConnected && !triesToConnectToMaster)
        {
            ConnectToMaster();
        }

        if (PhotonNetwork.IsConnected && !triesToConnectToMaster && !triesToConnectToRoom)
        {
            StartCoroutine(WaitFrameAndConnect());
        }
    }

    private void ConnectToMaster()
    {
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.GameVersion = "v1";

        triesToConnectToMaster = true;
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        
        triesToConnectToMaster = false;
        triesToConnectToRoom = false;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        
        triesToConnectToMaster = false;
    }

    private IEnumerator WaitFrameAndConnect()
    {
        triesToConnectToRoom = true;
        yield return new WaitForEndOfFrame();
        ConnectToRoom();
    }

    private void ConnectToRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("PhotonNetwork not connect, cannot connect to room!");
            return;
        }
        triesToConnectToRoom = true;

        PhotonNetwork.JoinOrCreateRoom(RoomName, _roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.OSXEditor
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.LinuxEditor
            || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            canvasGameObject.SetActive(true);
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer); // Set observer to master client for first scene load
        }
        
        PhotonNetwork.AutomaticallySyncScene = true; // Allow master-client to load level with PhotonNetwork.LoadLevel

        pizzeriaLabelCanvas.SetActive(true);

        connectedToRoom = true;
        
        if (Application.platform == RuntimePlatform.Android && singlePlayer)
        {
            PhotonNetwork.LoadLevel("Creation");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        
        PhotonNetwork.JoinOrCreateRoom(RoomName, _roomOptions, new TypedLobby(RoomName, LobbyType.Default));
    }

    public void ButtonClicked()
    {
        if(Application.platform == RuntimePlatform.Android)
            return;
        
        roomProperties = new Hashtable();

        var condition = conditionDropdown.options[conditionDropdown.value].text;
        
        roomProperties.Add(RoomProperty.Condition, condition);

        if (int.TryParse(participantIdInput.text, out var result))
        {
            parseResult.color = Color.green;
            
            participantIdInput.enabled = false;
            
            roomProperties.Add(RoomProperty.ParticipantId, result.ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            parseResult.SetText("Error: Could not parse " + participantIdInput.text + " into an integer.");
            participantIdInput.SetTextWithoutNotify("");
            parseResult.color = Color.red;
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomProperty.ParticipantId))
        {
            Debug.Log($"Properties Update On {Application.platform}, Loading Level.");
            PhotonNetwork.LoadLevel("Creation");
        }
    }
}
