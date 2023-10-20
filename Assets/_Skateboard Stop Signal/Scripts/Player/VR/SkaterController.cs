using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class SkaterController : MonoBehaviour
{
    public bool singlePlayer;
    
    // is the sst introduction finished?
    private bool sstIntroductionIsFinished;

    [SerializeField] private TaskLogic taskLogic;

    [SerializeField] private GameObject droneRidingPosition;
    [SerializeField] private GameObject droneCanvasPosition;
    
    private bool firstTaskIgnored = false;
    private GameObject currentChunk;
    private bool taskSolvedForChunk;
    
    private GameObject spawnPointGameObject;
    private Vector3 spawnPoint = new Vector3(160, 0, 160);

    private const float Speed = 16f;

    private bool _ridingEnabled;
    public bool RidingEnabled
    {
        set => _ridingEnabled = value;
    }

    private bool isTurning;
    private Vector3 dest;

    private float lerpDuration = 1.5f;

    // "UP" (z++), "DOWN"(z--), "LEFT"(x--), "RIGHT"(x++) in world space
    private String state;

    private new Transform transform;

    private PhotonView view;

    private GameObject drone;

    private bool displayObserver;
    
    private void Awake()
    {
        view = GetComponent<PhotonView>();
        displayObserver = GameManager.Instance.Condition == "A";
    }
    
    public void FinishIntroduction()
    {
        sstIntroductionIsFinished = true;
    }
    
    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (!view.IsMine || Application.platform != RuntimePlatform.Android)
        {
            Destroy(this);
            yield return null;
        }

        if (displayObserver)
        {
            drone = GameObject.Find("Drone");
            drone.transform.position = droneCanvasPosition.transform.position;
        }
        else
        {
            Destroy(drone);
        }
        transform = base.transform;
        
        spawnPointGameObject = GameObject.Find("Spawnpoint");
        spawnPoint = spawnPointGameObject.transform.position;
        
        yield return new WaitUntil(() => sstIntroductionIsFinished);

        CSVWriter.Instance.PhaseCounter = 6;
        CSVWriter.Instance.AddEntryToGeneral("ExposureStarted", "none");
        
        taskLogic.InitializeExposure();
        
        state = "UP";
    }

    private void FixedUpdate()
    {
        if (!view.IsMine)
            return;
        if (!_ridingEnabled)
            return;
        if (isTurning)
        {
            transform.position = Vector3.Lerp(transform.position, dest, 0.1f);
            if (transform.position == dest)
              isTurning = false;
        }
        else
        {
            transform.Translate(Vector3.forward * (Speed * Time.deltaTime));
        }
    }

    private void LateUpdate()
    {
        if (!_ridingEnabled) return;
        if(!isTurning && displayObserver)
            SetDronePosition();
    }

    private void SetDronePosition()
    {
        if (_ridingEnabled)
        {
            drone.transform.position = droneRidingPosition.transform.position;
        }
        else if(displayObserver)
        {
            drone.transform.position = droneCanvasPosition.transform.position;
        }
        drone.transform.LookAt(transform.position); // formerly in drone script
        drone.transform.Rotate(new Vector3(-100, 0, 0));
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chunk") && other.gameObject != currentChunk)
        {
            if (!firstTaskIgnored)
            {
                taskSolvedForChunk = true;
                firstTaskIgnored = true;
            }
            else
            {
                currentChunk = other.gameObject;
                taskSolvedForChunk = false;
            }
        }

        if (other.CompareTag("Task") && !taskSolvedForChunk && firstTaskIgnored)
        {
            StartCoroutine(taskLogic.StartTask(false, true));
            taskSolvedForChunk = true;
        }
    }

    public void ResetPlayerPosition()
    {
        transform.position = spawnPoint;
        state = "UP";
        transform.rotation = Quaternion.Euler(0, 0, 0);
        
        if (displayObserver)
        {
            SetDronePosition();
        }
    }
    
    private float RoundToNearestMultipleOfEighty(float toRound)
    {
        int n = 80;
        
        int round = Mathf.RoundToInt(toRound);

        int remainder = round % n;
        int divs = n / 2;
        
        return remainder > divs ? round + n - remainder : round - remainder;
    }

    public void MoveLeft()
    {
        isTurning = true;

        switch (state)
        {
            case "UP":
                dest = new Vector3(transform.localPosition.x-10, 0, RoundToNearestMultipleOfEighty(transform.position.z));
                state = "LEFT";
                break;
            case "DOWN":
                dest = new Vector3(transform.localPosition.x+10, 0, RoundToNearestMultipleOfEighty(transform.position.z));
                state = "RIGHT";
                break;
            case "LEFT":
                dest = new Vector3(RoundToNearestMultipleOfEighty(transform.position.x), 0, transform.localPosition.z-10);
                state = "DOWN";
                break;
            case "RIGHT":
                dest = new Vector3(RoundToNearestMultipleOfEighty(transform.position.x), 0, transform.localPosition.z+10);
                state = "UP";
                break;
        }
        
        StartCoroutine(LerpPosition(dest, new Vector3(0f, -90f, 0f)));
    }

    public void MoveRight()
    {
        isTurning = true;

        switch (state)
        {
            case "UP":
                dest = new Vector3(transform.localPosition.x+10, 0, RoundToNearestMultipleOfEighty(transform.position.z));
                state = "RIGHT";
                break;
            case "DOWN":
                dest = new Vector3(transform.localPosition.x - 10, 0, RoundToNearestMultipleOfEighty(transform.position.z));
                state = "LEFT";
                break;
            case "LEFT":
                dest = new Vector3(RoundToNearestMultipleOfEighty(transform.position.x), 0, transform.position.z+10);
                state = "UP";
                break;
            case "RIGHT":
                dest = new Vector3(RoundToNearestMultipleOfEighty(transform.position.x), 0, transform.position.z-10);
                state = "DOWN";
                break;
        }
        
        StartCoroutine(LerpPosition(dest, new Vector3(0f, 90f, 0f)));
    }

    private IEnumerator LerpPosition(Vector3 target, Vector3 rotation)
    {
        float time = 0;
        var startPosition = transform.position;
        
        var startRotation = transform.rotation;
        var targetRotation = startRotation * Quaternion.Euler(rotation);

        while (time < lerpDuration)
        {
            transform.position = Vector3.Lerp(startPosition, target, time/ lerpDuration);
            transform.rotation = Quaternion.Lerp(startRotation, startRotation * Quaternion.Euler(rotation), time / lerpDuration);
            time += Time.deltaTime;
            
            if(displayObserver)
                SetDronePosition();
            yield return null;
        }

        transform.position = target;
        transform.rotation = targetRotation;

        isTurning = false;
    }
    
    
}
