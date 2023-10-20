using UnityEngine;

public class QuestMarkerController : MonoBehaviour
{
    private Vector3 _anchorPosition;
    public Vector3 AnchorPosition
    {
        set
        {
            if (_anchorPosition == value)
                return;
            _anchorPosition = value;
            targetPosition = _anchorPosition;
            targetPosition.y += offset;
            isGoingUp = true;
            if (!transform)
                transform = base.transform;
            transform.position = _anchorPosition;
        }
    }

    private Vector3 targetPosition;
    private float offset = 4f;
    private float verticalSpeed = 0.03f;
    private Vector3 eulerRotation = new Vector3(0, .5f, 0);
    private bool isGoingUp;
    private new Transform transform;
    
    // Start is called before the first frame update
    void Start()
    {
        transform = base.transform;
        _anchorPosition = transform.position;
        isGoingUp = true;
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        if (isGoingUp)
        {
            pos.y += verticalSpeed;
            if (pos.y > targetPosition.y)
            {
                isGoingUp = false;
                targetPosition = _anchorPosition;
                targetPosition.y -= offset;
            }
        }
        else
        {
            pos.y -= verticalSpeed;

            if (pos.y < targetPosition.y)
            {
                isGoingUp = true;
                targetPosition = _anchorPosition;
                targetPosition.y += offset;
            }
        }

        transform.position = pos;
        
        transform.Rotate(eulerRotation);
    }
}
