using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class AssistantManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Camera localCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScreenShot screenShot;
    
    public Transform bodyTransform;
    
    private PhotonView view;

    private GameObject trackingObject;
    
    private void Start()
    {
        view = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;

        if (view.IsMine)
        {
            localCamera.enabled = true;
            trackingObject = PhotonView.Find(1).gameObject;
            gameObject.GetComponent<SynchronizeInformation>().enabled = true;
            gameObject.GetComponent<AssistantQuestionManager>().enabled = true;

            object valueAsObject;
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomProperty.Condition, out valueAsObject))
                    GameManager.Instance.Condition = valueAsObject as string;
        }
        else
        {
            Destroy(localCamera);
            Destroy(canvas.gameObject);
            
            Destroy(canvas.GetComponent<StandaloneInputModule>());
            Destroy(canvas.GetComponent<EventSystem>());
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!view.IsMine) return;
        bodyTransform.position = trackingObject.transform.position;
        bodyTransform.rotation = trackingObject.transform.rotation;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object valueAsObject;
        if (propertiesThatChanged.TryGetValue(RoomProperty.ScreenShot, out valueAsObject))
        {
            if ((bool) valueAsObject)
            {
                screenShot.TakeScreenShotsAndSaveToPath(Application.persistentDataPath + "\\Studie\\Pictures\\");
            }
        }
    }
}
