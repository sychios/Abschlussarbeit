using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysicsPointer : MonoBehaviour
{
    // corresponding controller to check input from
    public OVRInput.Controller controller;

    public Transform rayTransform;
    public float rayOffset = 1.5f;

    [SerializeField] private LineRenderer _lineRenderer;

    // is pointer active and raycasting for collisions?
    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetPointer(value);
    }
    
    // lineRenderer to visually express raycast
    public LineRenderer LineRenderer
    {
        get => _lineRenderer;
        set => _lineRenderer = value;
    }
    
    public float LineRendererStartWidth = 0.5f;
    public float LineRendererEndWidth = 0.15f;
    
    public Material lineRendererHighlightMaterial;
    public Material lineRendererDefaultMaterial;

    private GridElement _currentGridElement; // Currently selected GridElement. It display canvas to place a chunk onto it. At most one GridElement is selected at any time.
    private GameObject _touchedObject; // GridElement is highlighted when being hit by this laserpointer. It can than be selected. At most one GridElement is touched at any time.
    
    private bool fieldTouched = false;
    private bool _fieldSelected = false;

    public bool FieldSelected
    {
        get => _fieldSelected;
        set => _fieldSelected = value;
    }

    private bool controllerIndexPressed;

    private bool controllerPrimaryButtonPressed;
    private bool controllerPrimaryButtonReleased;

    private bool controllerThumbstickLeft;
    private bool controllerThumbstickRight;
    private bool controllerThumbstickReset;

    public float defaultLength = 80f;

    private Keyboard keyboard;

    private PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();

        if (!view.IsMine)
            return;
        
        _lineRenderer.material = lineRendererDefaultMaterial;
        _lineRenderer.startWidth = LineRendererStartWidth;
        _lineRenderer.endWidth = LineRendererEndWidth;

        controllerThumbstickReset = true;
        controllerPrimaryButtonReleased = true;
        
        SetPointer(false);
    }

    private void Start()
    {
        if (!view.IsMine)
            return;
        
        if(SceneManager.GetActiveScene().name == "Creation")
            keyboard = GameObject.FindWithTag("Keyboard").GetComponent<Keyboard>();
    }

    private void Update()
    {
        if (!view.IsMine)
            return;

        if (!_isActive)
            return;
        
        controllerIndexPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);
        //_controllerIndexReleased = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller);
        
        CastRay();

        if (controllerIndexPressed && fieldTouched && !_fieldSelected) // field touched and index down from controller
        {
            SelectGridElement();
        }
        
        if (fieldTouched || _fieldSelected)
        {
            controllerPrimaryButtonPressed = OVRInput.GetDown(OVRInput.Button.One, controller);
            
            if(OVRInput.GetUp(OVRInput.Button.One, controller))
                controllerPrimaryButtonReleased = true;

            controllerThumbstickLeft = OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft, controller);
            controllerThumbstickRight = OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight, controller);
            
            if(!controllerThumbstickLeft && !controllerThumbstickRight)
                controllerThumbstickReset = true;
        
            if (controllerThumbstickLeft && controllerThumbstickReset)
            {
                RotateChunk(-1);
                controllerThumbstickReset = false;

            } else if (controllerThumbstickRight && controllerThumbstickReset)
            {
                RotateChunk(1);
                controllerThumbstickReset = false;
            }

            if (controllerPrimaryButtonPressed && controllerPrimaryButtonReleased)
            {
                DeleteChunk();
                controllerPrimaryButtonReleased = false;
            }
        }
    }

    private void CastRay()
    {
        if (!_lineRenderer.enabled) return;
        
        _lineRenderer.SetPosition(0, rayTransform.position); // + rayTransform.forward*rayOffset
        _lineRenderer.SetPosition(1, CalculateEnd());
    }

    private Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        Vector3 endPosition = DefaultEnd(defaultLength);

        if (hit.collider)
            endPosition = hit.point;

        return endPosition;
    }

    private RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(rayTransform.position, rayTransform.forward);

        if (Physics.Raycast(ray, out hit, defaultLength) && !_fieldSelected)
        {
            GameObject collisionObject = hit.transform.gameObject;

            if (collisionObject.CompareTag("GridElement"))
            {
                _lineRenderer.material = lineRendererHighlightMaterial;

                if (!fieldTouched) // no field touched
                {
                    fieldTouched = true;
                } 
                else if (_touchedObject.GetInstanceID() != collisionObject.GetInstanceID())
                {
                    if(fieldTouched)
                        _currentGridElement.UnTouch();
                }
                
                _touchedObject = collisionObject;
                _currentGridElement = _touchedObject.GetComponent<GridElement>();
                _currentGridElement.Touch();
            } 
            else if (collisionObject.CompareTag("Keyboard"))
            {
                keyboard.Touch();
                if(controllerIndexPressed)
                    keyboard.FinishCreation();
            }
            else if (fieldTouched)
            {
                keyboard.Untouch();
                _currentGridElement.UnTouch();
                fieldTouched = false;
            }
            else
            {
                keyboard.Untouch();
            }
        }
        else
        {
            if (fieldTouched)
            {
                _currentGridElement.UnTouch();
                fieldTouched = false;
            }

            keyboard.Untouch();
            _lineRenderer.material = lineRendererDefaultMaterial;
        }

        return hit;
    }

    private Vector3 DefaultEnd(float length)
    {
        return rayTransform.position + (rayTransform.forward * length);
    }

    private void RotateChunk(float direction)
    {
        _currentGridElement.RotateChunk(direction);
    }

    private void DeleteChunk()
    {
        _currentGridElement.DeleteChunk();
    }
    
    private void SelectGridElement()
    {
        //PlayerManager.ParticipantPunInstance.GetComponent<OVRPlayer>().SetUiInteraction(true);
        CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().CurrentInteractionMode = OVRPlayer.InteractionMode.Canvas;
        //_selectedObject = _touchedObject;
        _touchedObject.GetComponent<GridElement>().Select();
        _fieldSelected = true;
        SetPointer(false);
    }

    public void DeselectGridElement()
    {
        _fieldSelected = false;
        SetPointer(true);

        //PlayerManager.ParticipantPunInstance.GetComponent<OVRPlayer>().SetUiInteraction(false);
        CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().CurrentInteractionMode =
            OVRPlayer.InteractionMode.Physic;
    }
    
    public void SetPointer(bool active)
    {
        _lineRenderer.enabled = active;
        _isActive = active;
    }
}
