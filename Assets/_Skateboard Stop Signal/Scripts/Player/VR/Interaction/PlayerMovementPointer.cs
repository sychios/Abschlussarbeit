using Photon.Pun;
using UnityEngine;


/**
 * Pointer for player movement (teleporting) and rotation
 */
public class PlayerMovementPointer : MonoBehaviour
{
    // corresponding oculus touch controller to check input from
    public OVRInput.Controller controller;
    // Transform to shoot ray from
    public Transform rayTransform;
    // LineRenderer to display Ray
    [SerializeField] private LineRenderer lineRenderer;
    
    private bool controllerIndexPressed; // Teleporting is done with Index Button
    private bool controllerIndexReleased; // Wait for release to not constantly teleport
    
    private bool controllerPrimaryButtonPressed; // Reset position in case of bugging out of area
    private bool controllerPrimaryButtonReleased; // Wait for release to not constantly reset position
    

    // Transform which is to be moved with teleportation
    public Transform BodyTransform;
    [SerializeField] private GameObject teleportMarker;

    public float LineRendererStartWidth = 0.5f;
    public float LineRendererEndWidth = 0.15f;
    public float RayOffset = 1.5f;

    private const string DefaultPath = "Materials/LineRendererDefault";
    private const string HighlightPath = "Materials/LineRendererHighlight";

    private Material _lineRendererHighlightMaterial;
    public Material LineRendererHighlightMaterial
    {
        get => _lineRendererHighlightMaterial;
        set => _lineRendererHighlightMaterial = value;
    }

    private Material _lineRendererDefaultMaterial;
    public Material LineRendererDefaultMaterial
    {
        get => _lineRendererDefaultMaterial;
        set => _lineRendererDefaultMaterial = value;
    }

    public float DefaultLength = 60f;
    
    // Rotation values
    private bool readyToSnapTurn = true;
    private float snapRotationAmount = 15f;
    
    // Reset place
    private Vector3 positionResetPoint;
    
    // Photon view to synchronize assistant
    private PhotonView view;

    private PhotonView trackingView;

    private GameObject centerEye;
    
    private void Awake()
    {
        view = GetComponent<PhotonView>();

        if (!view.IsMine)
            return;
        
        _lineRendererDefaultMaterial = Resources.Load<Material>(DefaultPath);
        _lineRendererHighlightMaterial = Resources.Load<Material>(HighlightPath);

        lineRenderer.material = _lineRendererDefaultMaterial;
        lineRenderer.startWidth = LineRendererStartWidth;
        lineRenderer.endWidth = LineRendererEndWidth;
        teleportMarker.SetActive(false);

        positionResetPoint = GameObject.FindGameObjectWithTag("Respawn").transform.position;
    }
    
    
    private void Start()
    {
        if (!view.IsMine)
            return;
        
        Vector3[] startPositions = {Vector3.zero, Vector3.zero};
        lineRenderer.SetPositions(startPositions);
        lineRenderer.enabled = true;

        trackingView = PhotonView.Find(1);
        
        controllerIndexReleased = true;
        controllerPrimaryButtonReleased = true;
        
        centerEye = GameObject.Find("CenterEyeAnchor");
    }

    void Update()
    {
        if (!view.IsMine)
            return;
        
        // handle rotation
        Vector3 euler = BodyTransform.rotation.eulerAngles;
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft, controller))
        { 
            if (readyToSnapTurn) 
            { 
                euler.y -= snapRotationAmount;
                
                //_view.RPC("RPC_SetRotation", RpcTarget.All, euler);
                //synchronizeTransform.Call_SetRotation(euler);
                
                BodyTransform.rotation = Quaternion.Euler(euler);
                
                readyToSnapTurn = false; 
                if (CSVWriter.Instance) 
                    CSVWriter.Instance.AddEntryToGeneral("Player_ROT", "LEFT");
            }
        }
        else if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight, controller)) 
        { 
            if (readyToSnapTurn) 
            { 
                euler.y += snapRotationAmount;
                
                //_view.RPC("RPC_SetRotation", RpcTarget.All, euler);
                //synchronizeTransform.Call_SetRotation(euler);
                
                BodyTransform.rotation = Quaternion.Euler(euler);
                
                readyToSnapTurn = false; 
                if (CSVWriter.Instance) 
                    CSVWriter.Instance.AddEntryToGeneral("Player_ROT", "RIGHT");
            }
        }
        else
        {
            readyToSnapTurn = true;
        }
        

        // handle teleporting
        if (!lineRenderer.enabled) return;
        
        controllerIndexPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);

        //TODO: Why "NOT" in the if-clause?
        if (!OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller))
            controllerIndexReleased = true;
        
        // formerly updateLine was here
        
        if (teleportMarker.activeSelf && controllerIndexPressed && controllerIndexReleased)
        {
            Teleport();
            controllerIndexReleased = false;
        }
        
        controllerPrimaryButtonPressed = OVRInput.GetDown(OVRInput.Button.One, controller);

        if (OVRInput.GetUp(OVRInput.Button.One, controller))
            controllerPrimaryButtonReleased = true;

        if (controllerPrimaryButtonPressed && controllerPrimaryButtonReleased)
        {
            BodyTransform.position = positionResetPoint;
            controllerPrimaryButtonReleased = false;
        }
        
        // Update raycast
        UpdateLine(rayTransform.position+rayTransform.forward*RayOffset);

        trackingView.gameObject.transform.rotation = centerEye.transform.rotation;
    }

    private void UpdateLine(Vector3 targetPosition)
    {
        RaycastHit hit;
        
        Ray lineOut = new Ray(targetPosition, rayTransform.forward);

        Vector3 endPosition = targetPosition + DefaultLength * rayTransform.forward;

        if (Physics.Raycast(lineOut, out hit, DefaultLength))
        {
            endPosition = hit.point;
            
            GameObject collisionObj = hit.collider.gameObject;

            if (collisionObj.CompareTag("Floor"))
            {
                if (!teleportMarker.activeSelf)
                {
                    teleportMarker.SetActive(true);
                    lineRenderer.material = _lineRendererHighlightMaterial;
                }
                teleportMarker.transform.position = endPosition;
                teleportMarker.transform.rotation = Quaternion.identity;
            }
            else
            {
                teleportMarker.SetActive(false);
                lineRenderer.material = _lineRendererDefaultMaterial;
            }
        }
        else
        {
            teleportMarker.SetActive(false);
            lineRenderer.material = _lineRendererDefaultMaterial;
        }
        lineRenderer.SetPosition(0, targetPosition);
        lineRenderer.SetPosition(1, endPosition);
    }

    private void Teleport()
    {
        var markerPosition = teleportMarker.transform.position;
        var currentPosition = BodyTransform.position;
        var newPos = new Vector3(markerPosition.x, BodyTransform.position.y, markerPosition.z);
        //_view.RPC("RPC_SetPosition", RpcTarget.All, newPos);
        //synchronizeTransform.Call_SetPosition(newPos);

        BodyTransform.position = newPos;
        trackingView.transform.position = newPos;

        if (CSVWriter.Instance)
        {
            var curr = new Vector2(currentPosition.x, currentPosition.z);
            var marker = new Vector2(markerPosition.x, markerPosition.z);
            CSVWriter.Instance.AddEntryToGeneral("Player_MOVE", curr + "#" + marker);
        }
    }

    public void SetActive(bool active)
    {
        lineRenderer.enabled = active;
    }
}