using UnityEngine;

public class GridElementCanvas : MonoBehaviour
{
    private Transform _rotationTargetTransform;
    public Transform RotationTargetTransform
    {
        get => _rotationTargetTransform;
        set => _rotationTargetTransform = value;
    }

    private PhysicsPointer _playerPhysicsPointer;
    public PhysicsPointer PlayerPhysicsPointer
    {
        get => _playerPhysicsPointer;
        set => _playerPhysicsPointer = value;
    }

    private Canvas canvas;

    private Transform _transform;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canvas.enabled) return;

        _transform = transform;
        
        _transform.localEulerAngles = new Vector3(0, _transform.localEulerAngles.y, 0);
        
        Vector3 targetDirection = _rotationTargetTransform.position - _transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        targetRotation.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, 0);
        transform.rotation = targetRotation;
    }

    public void Show()
    {
        canvas.enabled = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
        _playerPhysicsPointer.DeselectGridElement();
    }
}
