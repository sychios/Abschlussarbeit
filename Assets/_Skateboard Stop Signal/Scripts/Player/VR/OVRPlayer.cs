using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class OVRPlayer : MonoBehaviourPunCallbacks
{
    [SerializeField] private Camera centerEyeAnchorCamera;
    
    private PhotonView view;
    
    private OVRScreenFade ovrScreenFade;

    [SerializeField] private GameObject canvasPointer;

    // Hand Transform to add pointer components
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;

    private PlayerMovementPointer movementPointerInstance;
    private PhysicsPointer physicsPointerInstance;

    [SerializeField] private GameObject leftControllerCanvasGameObject;
    [SerializeField] private GameObject rightControllerCanvasGameObject;
    private GameObject _activeControllerCanvasGameObject;
    public GameObject ActiveControllerCanvasGameObject
    {
        get => _activeControllerCanvasGameObject; 
        set => _activeControllerCanvasGameObject = value;
    }

    public enum InteractionMode  {Physic, Canvas}
    private InteractionMode _currentInteractionMode;
    public InteractionMode CurrentInteractionMode
    {
        get => _currentInteractionMode;
        set
        {
            _currentInteractionMode = value;
            SetInteractionMode(_currentInteractionMode);
        }
    }

    private string lineRendererDefaultMaterialPath = "Materials/LineRendererDefault";
    private string lineRendererHighlightMaterialPath = "Materials/LineRendererHighlight";

    private Material lineRendererDefaultMaterial;
    private Material lineRendererHighlightMaterial;

    [SerializeField] private OVRInputModule ovrInputModule;

    [SerializeField] private GameObject guiHelper;

    private GameObject[] _interactableCanvases;
    
    
    // activate only on start?
    // GUIHelpers?
    // OVRCameraRig?
    // OVRManager

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        // Do this early so we can delete OVRRaycast component on non-VR device
        _interactableCanvases = GameObject.FindGameObjectsWithTag("InteractableCanvas");

        if (!view.IsMine)
        {
            Camera[] cams = GetComponentsInChildren<Camera>();
            
            foreach (var cam in cams)
            {
                Destroy(cam);
            }
            OVRCameraRig rig = GetComponentInChildren<OVRCameraRig>();
            Destroy(rig);
            OVRManager manager = GetComponentInChildren<OVRManager>();
            Destroy(manager);

            foreach (var canvas in _interactableCanvases)
            {
                Destroy(canvas.GetComponent<OVRRaycaster>());
            }

            return;
            
        } else 
        {
            foreach (var canvas in _interactableCanvases)
            {
                canvas.GetComponent<Canvas>().enabled = true;
                canvas.GetComponent<Canvas>().worldCamera = centerEyeAnchorCamera;

                if (canvas.GetComponent<GridElementCanvas>())
                {
                    canvas.GetComponent<GridElementCanvas>().enabled = true;
                    canvas.GetComponent<GridElementCanvas>().PlayerPhysicsPointer = physicsPointerInstance;
                    canvas.GetComponent<GridElementCanvas>().RotationTargetTransform = gameObject.transform;
                }

                canvas.GetComponent<OVRRaycaster>().enabled = true;
                canvas.GetComponent<OVRRaycaster>().pointer = canvasPointer;
            }
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        // Do this early so we can delete OVRRaycast component on non-VR device
        _interactableCanvases = GameObject.FindGameObjectsWithTag("InteractableCanvas");
        
        if (!view.IsMine)
        {
            return;
        }
        centerEyeAnchorCamera.enabled = true;
        ovrScreenFade = GetComponentInChildren<OVRScreenFade>();

        lineRendererDefaultMaterial = Resources.Load<Material>(lineRendererDefaultMaterialPath);
        lineRendererHighlightMaterial = Resources.Load<Material>(lineRendererHighlightMaterialPath);

        movementPointerInstance = GetComponent<PlayerMovementPointer>();
        physicsPointerInstance = GetComponent<PhysicsPointer>();

        // Setup canvas interaction

        GameObject[] gridElements = GameObject.FindGameObjectsWithTag("GridElement");
        foreach (var elem in gridElements)
        {
            elem.GetComponent<GridElement>().PlayerPhysicsPointer = physicsPointerInstance;
        }

        string condition = PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.Condition] as string;
        string pID = PhotonNetwork.CurrentRoom.CustomProperties[RoomProperty.ParticipantId] as string;

        GameManager.Instance.Condition = condition;
        GameManager.Instance.ParticipantID = pID;
        
        CSVWriter.Instance.SetCondition(condition);
        CSVWriter.Instance.SetParticipantId(pID);

        CSVWriter.Instance.AddEntryToGeneral("Spawn", "none");
        
        ovrScreenFade.FadeIn();
        
        SetPointers(false);
        SetUiInteraction(true);
    }

    public void SetUiInteraction(bool setActive)
    {
        guiHelper.SetActive(setActive);

        /*
        foreach (var canvas in _interactableCanvases)
        {
            canvas.GetComponent<OVRRaycaster>().enabled = setActive;
        }*/
    }

    public void SetPointers(bool switchControllers)
    {
        if (switchControllers) // Interaction on the left, Movement on the right
        {
            // Assign controller to use input and cast raycast from for movement
            movementPointerInstance.controller = OVRInput.Controller.RTouch;
            movementPointerInstance.rayTransform = rightHandAnchor;

            // Assign controller to use input and cast raycast from for physical interaction (mainly platforms for placing chunks)
            physicsPointerInstance.controller = OVRInput.Controller.LTouch;
            physicsPointerInstance.rayTransform = leftHandAnchor;
            
            //Assign canvas gameobject to show while creating city
            ActiveControllerCanvasGameObject = rightControllerCanvasGameObject;
            
            // Assign input button for canvas "clicking" and raycast source transform
            ovrInputModule.rayTransform = leftHandAnchor;
            ovrInputModule.joyPadClickButton = OVRInput.Button.PrimaryIndexTrigger;
        }
        else // Interaction on the right, movement on the left
        {
            // Assign controller to use input and cast raycast from for movement
            movementPointerInstance.controller = OVRInput.Controller.LTouch;
            movementPointerInstance.rayTransform = leftHandAnchor;
            
            // Assign controller to use input and cast raycast from for physical interaction (mainly platforms for placing chunks)
            physicsPointerInstance.controller = OVRInput.Controller.RTouch;
            physicsPointerInstance.rayTransform = rightHandAnchor;
            
            // Assign canvas gameobject
            ActiveControllerCanvasGameObject = leftControllerCanvasGameObject;
            
            // Assign input button for canvas "clicking" and raycast source transform
            ovrInputModule.rayTransform = rightHandAnchor;
            ovrInputModule.joyPadClickButton = OVRInput.Button.SecondaryIndexTrigger;
        }
        
        // Update information canvas containing explanatory images about the user controls
        InformationCanvas.SetPointerInfo(switchControllers);
    }

    private void SetInteractionMode(InteractionMode mode)
    {
        //TODO: set pointers accordingly

        if (mode == InteractionMode.Canvas)
        {
            //PlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().SetUiInteraction(true);
            SetUiInteraction(true);
            CreationPlayerManager.LocalPlayerInstance.GetComponentInChildren<PhysicsPointer>().SetPointer(false);
                
            ovrInputModule.InteractionIsActive = true;
            //ovrInputModule.ActivateModule();
        }
        else
        {
            CreationPlayerManager.LocalPlayerInstance.GetComponentInChildren<PhysicsPointer>().SetPointer(true); // enable physics interaction
            //PlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().SetUiInteraction(false); // disable ui interaction
            SetUiInteraction(false); // disable ui interaction
            
            ovrInputModule.InteractionIsActive = false;
            //ovrInputModule.DeactivateModule();
        }
    }
    
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object value;
        if (propertiesThatChanged.TryGetValue(RoomProperty.Condition, out value))
        {
            GameManager.Instance.Condition = (string) value;
        }
        if (propertiesThatChanged.TryGetValue(RoomProperty.ParticipantId, out value))
        {
            var pId = value.ToString();
            GameManager.Instance.ParticipantID = pId;
        }
    }
}
