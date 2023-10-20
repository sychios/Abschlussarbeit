using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Photon.Pun;
using UnityEngine.EventSystems;
using Sigtrap.VrTunnellingPro;
using UnityEngine;

public class ExposureOvrPlayer : MonoBehaviour
{
    private Camera centerEyeAnchorCamera;
    private GameObject centerEye;

    private PhotonView view;
    
    [SerializeField] private GameObject canvasPointer;

    [SerializeField] private GameObject guiHelper;

    private GameObject cameraTrackingView;
    
    // Hand Transform to add pointer components
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;
    
    private string lineRendererDefaultMaterialPath = "Materials/LineRendererDefault";
    private string lineRendererHighlightMaterialPath = "Materials/LineRendererHighlight";

    private Material lineRendererDefaultMaterial;
    private Material lineRendererHighlightMaterial;
    
    [SerializeField] private OVRInputModule ovrInputModule;

    private bool areControllersSet;
    
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        if (!view.IsMine)
        {
            Camera[] cams = GetComponentsInChildren<Camera>();
            
            foreach (var cam in cams)
            {
                Destroy(cam);
            }

            OVRCameraRig rig = GetComponentInChildren<OVRCameraRig>();
            Destroy(rig);

            TunnellingMobile tunnellingMobile = GetComponentInChildren<TunnellingMobile>();
            Destroy(tunnellingMobile);

            OVRManager manager = GetComponentInChildren<OVRManager>();
            Destroy(manager);

            OVRScreenFade fade = GetComponentInChildren<OVRScreenFade>();
            Destroy(fade);

            Destroy(guiHelper);
            
            Destroy(this);
            
            return;
        }

        object conditionObject;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomProperty.Condition, out conditionObject))
        {
            if (conditionObject as string == "B")
                Destroy(GameObject.Find("Drone"));
        }
        
        lineRendererDefaultMaterial = Resources.Load<Material>(lineRendererDefaultMaterialPath);
        lineRendererHighlightMaterial = Resources.Load<Material>(lineRendererHighlightMaterialPath);
        
        centerEyeAnchorCamera = Camera.main;
        centerEye = GameObject.Find("CenterEyeAnchor");
        cameraTrackingView = PhotonView.Find(2).gameObject;
        
        // Set component OVRScreenFade of player on border colliders
        BorderCollider[] borderColliders = FindObjectsOfType<BorderCollider>();
        foreach (var borderCollider in borderColliders)
        {
            borderCollider.skaterController = GetComponent<SkaterController>();
            borderCollider.ovrScreenFade = OVRScreenFade.instance;
        }

        // Setup canvas interaction
        var canvases = GameObject.FindGameObjectsWithTag("InteractableCanvas");
        foreach(var canvas in canvases)
        {
            canvas.GetComponent<OVRRaycaster>().enabled = true;
        }
    }
    
    private void SetPointers(bool controllersSwitched)
    {
        // Assign input button for canvas "clicking" and raycast source transform
        ovrInputModule.rayTransform =  controllersSwitched ? leftHandAnchor : rightHandAnchor;
        ovrInputModule.joyPadClickButton = controllersSwitched ? OVRInput.Button.PrimaryIndexTrigger : OVRInput.Button.SecondaryIndexTrigger;
    }

    private void Update()
    {
        if (!view.IsMine) return;

        if (!areControllersSet)
        {
            SetPointers(GameManager.Instance.ControllersSwitched);
            areControllersSet = true;
        }
        
        cameraTrackingView.transform.position = centerEye.transform.position;
        cameraTrackingView.transform.rotation = centerEye.transform.rotation;
    }
}
