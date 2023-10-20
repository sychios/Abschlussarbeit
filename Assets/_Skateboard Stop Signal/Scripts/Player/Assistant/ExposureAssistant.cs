using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ExposureAssistant : MonoBehaviour
{
    private PhotonView view;

    private GameObject trackingObject;
    
    [SerializeField] private Camera localCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject eventSystem;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    private Transform localTransform;

    private void Awake()
    {
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            if (GameManager.Instance.Condition != "A")
            {
                GetComponent<AgoraLauncher>();
                Destroy(GameObject.Find("Drone"));
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (view.IsMine)
        {
            //Destroy(localCamera);
            //Destroy(canvas.gameObject);
            //canvas.gameObject.SetActive(true);
            localCamera.enabled = true;
            trackingObject = PhotonView.Find(2).gameObject;
            localTransform = transform;
            canvas.gameObject.SetActive(true);
            GetComponent<AudioSource>().enabled = true;
            //GetComponent<AudioListener>().enabled = true;
            eventSystem.SetActive(true);
            graphicRaycaster.enabled = true;
            //_gameManager = PhotonView.Find(2).gameObject.GetComponent<GameManager>();
            //canvas.GetComponent<StandaloneInputModule>().enabled = true;
            //canvas.GetComponent<EventSystem>().enabled = true;
        }
        else
        {
            Destroy(localCamera);
            Destroy(canvas.gameObject);
            
            Destroy(GetComponent<AudioSource>());

            //Destroy(GetComponent<AudioListener>());

            Destroy(eventSystem);
            Destroy(graphicRaycaster);

            //Destroy(canvas.GetComponent<StandaloneInputModule>());
            //Destroy(canvas.GetComponent<EventSystem>());
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine) return;

        localTransform.position = trackingObject.transform.position;
        localTransform.rotation = trackingObject.transform.rotation;

    }
}
