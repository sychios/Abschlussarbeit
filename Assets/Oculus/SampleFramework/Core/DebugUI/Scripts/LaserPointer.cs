/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class LaserPointer : OVRCursor
{
    public enum LaserBeamBehavior
    {
        On,        // laser beam always on
        Off,        // laser beam always off
        OnWhenHitTarget,  // laser beam only activates when hit valid target
    }

    public GameObject cursorVisual;
    public float maxLength = 10.0f;

    [SerializeField] private LaserBeamBehavior _laserBeamBehavior;
    bool m_restoreOnInputAcquired = false;

    public LaserBeamBehavior laserBeamBehavior
    {
        set
        {
            _laserBeamBehavior = value;
            if (laserBeamBehavior == LaserBeamBehavior.Off || laserBeamBehavior == LaserBeamBehavior.OnWhenHitTarget)
            {
                _lineRenderer.enabled = false;
            }
            else
            {
                _lineRenderer.enabled = true;
            }
        }
        get
        {
            return _laserBeamBehavior;
        }
    }
    private Vector3 _startPoint;
    private Vector3 _forward;
    private Vector3 _endPoint;
    private bool _hitTarget;
    
    
    private LineRenderer _lineRenderer;
    
    private string _lineRendererDefaultMaterialPath = "Materials/LineRendererDefault";
    private string _lineRendererHighlightMaterialPath = "Materials/LineRendererHighlight";
    
    private Material _defaultLineRendererMaterial;
    private Material _highlightLineRendererMaterial;
    private readonly float _startWidth = 0.5f;
    private readonly float _endWidth = 0.15f;
    
    //private readonly float _defaultLength = 30f;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            _lineRenderer.startWidth = _startWidth*0.1f;
            _lineRenderer.endWidth = _endWidth*0.1f;
        }
        else
        {
            _lineRenderer.startWidth = _startWidth;
            _lineRenderer.endWidth = _endWidth;
        }

        _defaultLineRendererMaterial = Resources.Load<Material>(_lineRendererDefaultMaterialPath);
        _highlightLineRendererMaterial = Resources.Load<Material>(_lineRendererHighlightMaterialPath);
    }

    private void Start()
    {
        _lineRenderer.material = _defaultLineRendererMaterial;
        if (cursorVisual) cursorVisual.SetActive(false);
        OVRManager.InputFocusAcquired += OnInputFocusAcquired;
        OVRManager.InputFocusLost += OnInputFocusLost;
    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
    {
        _startPoint = start;
        _endPoint = dest;
        _hitTarget = true;
    }

    public override void SetCursorRay(Transform t)
    {
        _startPoint = t.position;
        _forward = t.forward;
        _hitTarget = false;
    }

    private void Update() //TODO: formerly in LateUpdate
    {
        var start = _startPoint + _forward * 0.05f;
        _lineRenderer.SetPosition(0, start);
        if (_hitTarget)
        {
            _lineRenderer.SetPosition(1, _endPoint);
            UpdateLaserBeam(start, _endPoint);
            if (cursorVisual)
            {
                cursorVisual.transform.position = _endPoint;
                cursorVisual.SetActive(true);
            }
        }
        else
        {
            UpdateLaserBeam(start, start + maxLength * _forward);
            _lineRenderer.SetPosition(1, start + maxLength * _forward);
            if (cursorVisual) cursorVisual.SetActive(false);
        }
    }

    // make laser beam a behavior with a prop that enables or disables
    private void UpdateLaserBeam(Vector3 start, Vector3 end)
    {
        if (laserBeamBehavior == LaserBeamBehavior.Off)
        {
            return;
        }
        else if (laserBeamBehavior == LaserBeamBehavior.On)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }
        else if (laserBeamBehavior == LaserBeamBehavior.OnWhenHitTarget)
        {
            if (_hitTarget)
            {
                if (!_lineRenderer.enabled)
                {
                    _lineRenderer.enabled = true;
                    _lineRenderer.SetPosition(0, start);
                    _lineRenderer.SetPosition(1, end);
                }
            }
            else
            {
                if (_lineRenderer.enabled)
                {
                    _lineRenderer.enabled = false;
                }
            }
        }
        _lineRenderer.material = _hitTarget ? _highlightLineRendererMaterial : _defaultLineRendererMaterial;
    }

    void OnDisable()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
    }
    public void OnInputFocusLost()
    {
        if (gameObject && gameObject.activeInHierarchy)
        {
            m_restoreOnInputAcquired = true;
            gameObject.SetActive(false);
        }
    }

    public void OnInputFocusAcquired()
    {
        if (m_restoreOnInputAcquired && gameObject)
        {
            m_restoreOnInputAcquired = false;
            gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
        OVRManager.InputFocusLost -= OnInputFocusLost;
    }
}
