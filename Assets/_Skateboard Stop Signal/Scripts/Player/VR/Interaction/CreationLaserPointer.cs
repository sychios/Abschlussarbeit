using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationLaserPointer : OVRCursor
{
    public enum LaserBeamBehavior
    {
        On, // laser beam always on
        Off, // laser beam always off
        OnWhenHitTarget, // laser beam only activates when hit valid target
    }

    public GameObject cursorVisual;
    public float maxLength = 80.0f;

    [SerializeField] private LaserBeamBehavior _laserBeamBehavior;
    private bool restoreOnInputAcquired = false;

    public LaserBeamBehavior laserBeamBehavior
    {
        set
        {
            _laserBeamBehavior = value;
            if (laserBeamBehavior == LaserBeamBehavior.Off || laserBeamBehavior == LaserBeamBehavior.OnWhenHitTarget)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                lineRenderer.enabled = true;
            }
        }
        get { return _laserBeamBehavior; }
    }

    private Vector3 startPoint;
    private Vector3 forward;
    private Vector3 endPoint;
    private bool hitTarget;


    private LineRenderer lineRenderer;

    private string lineRendererDefaultMaterialPath = "Materials/LineRendererDefault";
    private string lineRendererHighlightMaterialPath = "Materials/LineRendererHighlight";

    private Material defaultLineRendererMaterial;
    private Material highlightLineRendererMaterial;
    public float StartWidth = 0.5f;
    public float EndWidth = 0.15f;

    public float RayOffset = 1.5f;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = StartWidth;
        lineRenderer.endWidth = EndWidth;

        defaultLineRendererMaterial = Resources.Load<Material>(lineRendererDefaultMaterialPath);
        highlightLineRendererMaterial = Resources.Load<Material>(lineRendererHighlightMaterialPath);
    }

    private void Start()
    {
        lineRenderer.material = defaultLineRendererMaterial;
        if (cursorVisual) cursorVisual.SetActive(false);
        OVRManager.InputFocusAcquired += OnInputFocusAcquired;
        OVRManager.InputFocusLost += OnInputFocusLost;
    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
    {
        startPoint = start;
        endPoint = dest;
        hitTarget = true;
    }

    public override void SetCursorRay(Transform t)
    {
        startPoint = t.position;
        forward = t.forward;
        hitTarget = false;
    }

    private void LateUpdate()
    {
        var start = startPoint + forward * RayOffset;
        //_lineRenderer.SetPosition(0, start);
        if (hitTarget)
        {
            //_lineRenderer.SetPosition(1, _endPoint);
            UpdateLaserBeam(start, endPoint);
            if (cursorVisual)
            {
                cursorVisual.transform.position = endPoint;
                cursorVisual.SetActive(true);
            }
        }
        else
        {
            UpdateLaserBeam(start, start + maxLength * forward);
            //_lineRenderer.SetPosition(1, start + maxLength * _forward);
            if (cursorVisual) cursorVisual.SetActive(false);
        }
    }

    // make laser beam a behavior with a prop that enables or disables
    public void UpdateLaserBeam(Vector3 start, Vector3 end)
    {
        if (laserBeamBehavior == LaserBeamBehavior.Off)
        {
            return;
        }
        else if (laserBeamBehavior == LaserBeamBehavior.On)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        else if (laserBeamBehavior == LaserBeamBehavior.OnWhenHitTarget)
        {
            if (hitTarget)
            {
                if (!lineRenderer.enabled)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.SetPosition(0, start);
                    lineRenderer.SetPosition(1, end);
                }
            }
            else
            {
                if (lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                }
            }
        }

        lineRenderer.material = hitTarget ? highlightLineRendererMaterial : defaultLineRendererMaterial;
    }

    void OnDisable()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
    }
    
    public void OnInputFocusLost()
    {
        if (gameObject && gameObject.activeInHierarchy)
        {
            restoreOnInputAcquired = true;
            gameObject.SetActive(false);
        }
    }

    public void OnInputFocusAcquired()
    {
        if (restoreOnInputAcquired && gameObject)
        {
            restoreOnInputAcquired = false;
            gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
        OVRManager.InputFocusLost -= OnInputFocusLost;
    }
}
