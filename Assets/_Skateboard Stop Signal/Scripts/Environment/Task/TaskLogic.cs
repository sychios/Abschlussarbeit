using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TaskLogic : MonoBehaviour
{
    // values to determine the outcome of trial
    private bool buttonPressed;
    private bool reactedToTrial;
    private bool currentTaskFinished;
    private bool interruptTask; // use when time ran out or input detected

    private readonly float initialDelay = 0.25f;

    private float currentDelay;

    private float delayOptimiseValue = 0.05f;

    private float delayUpperBound = 1.5f-0.05f;
    private float delayLowerBound = 0f;
    
    /// <summary>
    /// Index for the signal-delay-ladder.
    ///
    /// Incremented after successful no-go task and decremented after failed no-go task
    /// </summary>
    private int signalDelayLadderIndex;
    
    /// <summary>
    /// Time in milliseconds the arrow is shown
    /// </summary>
    private readonly float taskDuration = 1.5f;

    /// <summary>
    /// Time in milliseconds the result is shown
    /// </summary>
    private readonly float resultDuration = 1f;

    // Describes the error codes which determine what result message and color to display
    private enum ResultCodes
    {
        GoodGo, // Successful Go-Task
        GoodNoGo, // Successful No-Go-Task
        GoodThenBad,
        None, // No input despite expected while the arrows were shown
        Early, // Input detected before arrow was shown
        Late, // Input detected after time to react ran out
        Multiple, // Input detected at least twice (after good try or already failed task)
        Direction, // Input on a button other than expected detected
        Signal // Input despite signal being played detected
    }
    // If cell is true, error code is already active on current task instance
    private bool[] activeResultCodes;
    private int currentResultCode;


    private int taskAmount = 16; // has to be an even number for the reason of making exactly 25% to No-Go tasks!
    private bool[] taskArray;
    private int taskArrayIndex;

    private PhotonView view;
    
    // Coroutine waiting for input during a task
    private IEnumerator inputCoroutine;
    
    // Coroutine used as a timer for the task duration, interrupts task instance when time ran out
    private IEnumerator timerCoroutine;
    
    // Reference to Skater GameObject to steer to the left or right after a task
    private SkaterController skaterController;

    // Reference to introduction logic
    [SerializeField] private SstIntroduction sstIntroduction;

    private TaskUI taskUI;

    [SerializeField] private RoundManager roundManager;
    
    [SerializeField] private GameObject timer;
    private ExposureTimer exposureTimer;

    [SerializeField] private GameObject pizzaHeatSlider;
    [SerializeField] private Transform handleTransform;

    private readonly float[] handlePositions =
    {
        -75f,
        -37.5f,
        0,
        37.5f,
        75f
    };
    private int handlePositionIndex;

    // used to detect if there was any controller input after a task
    private bool anyInput;
    private bool anyTimeRanOut;

    private TaskInstance currentTaskInstance;
    private float globalTaskCounter;
    private bool successfulTaskInstance;

    private long taskStartTime; // Time of arrow display
    private long taskReactionTime; // Time of first reaction
    
    
    // Coroutines necessary to stop to handle task instance
    private Coroutine taskTimerCoroutine;
    private Coroutine taskInputCoroutine;
    private Coroutine taskDisplayCoroutine;
    private Coroutine inputAfterTaskCoroutine;
    private Coroutine taskResultCoroutine;
    
    // Controller variables
    private bool primaryIndexPressed;
    private bool primaryIndexReleased;

    private bool secondaryIndexPressed;
    private bool secondaryIndexReleased;
    
    // Feedback variables
    private List<float> reactionTimes = new List<float>(); // list of all reactions times
    private List<bool> goTrials = new List<bool>(); // list of all go trials, where value is True if trials was good and False when trial was bad
    private List<bool> signalTrials = new List<bool>(); // same as with go-trials list
    
    private void Start()
    {
        view = gameObject.GetComponent<PhotonView>();

        if (!view.IsMine)
        {
            Destroy(this);
            return;
        }

        currentDelay = initialDelay;

        taskUI = gameObject.GetComponent<TaskUI>();
        
        skaterController = gameObject.GetComponent<SkaterController>();

        exposureTimer = timer.GetComponent<ExposureTimer>();
        
        taskUI.HideAll();

        signalDelayLadderIndex = 0;
        
        // controller handling
        primaryIndexReleased = true;
        secondaryIndexReleased = true;
    }

    private void Update()
    {
        primaryIndexPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
        if (!OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            primaryIndexReleased = true;
        
        secondaryIndexPressed = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        if (!OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            secondaryIndexReleased = true;
    }

    public IEnumerator StartTasks(int amount, bool showSignal)
    {
        if (!view.IsMine)
            yield return null;
        taskArray = GetTaskArray(showSignal, amount, 0.25f);
        taskArrayIndex = 0;

        yield return new WaitForSeconds(2f);

        while (taskArrayIndex < taskArray.Length-1)
        {
            StartCoroutine(StartTask(true, showSignal));
            currentTaskFinished = false;
            
            yield return new WaitUntil(() => currentTaskFinished);
            var intertrialTime = Random.Range(1.0f, 2.0f);
            yield return new WaitForSeconds(intertrialTime); //TODO: former 2f
        }

        sstIntroduction.TasksFinished = true;
        //_taskUI.ExampleRunning = false;
    }

    public IEnumerator StartTasksWithModifiedSignalProbability(int amount, float signalProbability)
    {
        if (!view.IsMine)
            yield return null;
        taskArray = GetTaskArray(true, amount, signalProbability);
        taskArrayIndex = 0;
        yield return new WaitForSeconds(2f);
        
        while (taskArrayIndex < taskArray.Length-1)
        {
            StartCoroutine(StartTask(true, true));
            currentTaskFinished = false;
            
            yield return new WaitUntil(() => currentTaskFinished);
            var intertrialTime = Random.Range(1.0f, 2.0f);
            yield return new WaitForSeconds(intertrialTime); //TODO: former 2f
        }

        sstIntroduction.TasksFinished = true;
        
    }


    // If trial is true, the task is for practise
    public IEnumerator StartTask(bool trial, bool signalActive)
    {
        // Increase global task index
        globalTaskCounter++;

        // Refresh some attributes for new task
        var playSignal = taskArray[taskArrayIndex] && signalActive;
        // Random arrow direction -> Right: 1, Left: -1
        var stimulus = Random.Range(0f, 1f) > 0.5 ? 1 : -1;
        reactedToTrial = false;
        currentTaskFinished = false;
        interruptTask = false;
        successfulTaskInstance = false;
        activeResultCodes = new bool[Enum.GetNames(typeof(ResultCodes)).Length];
        currentResultCode = -1;

        taskStartTime = -1;
        taskReactionTime = -1;

        // Initialise new logging task object
        // Setup logging instance
        currentTaskInstance = new TaskInstance(trial, playSignal, (int) globalTaskCounter, taskArrayIndex , stimulus, currentDelay);

        var propertySubString = stimulus == -1 ? "l" : "r";
        propertySubString = playSignal ? propertySubString + "t" : propertySubString + "f"; 
        var properties = new Hashtable
        {
            {RoomProperty.Arrow, propertySubString}
        };
        if (playSignal)
            properties.Add(RoomProperty.SSD, signalDelayLadderIndex);

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        
        //_inputCoroutine = WaitForInput(randomArrow, playSignal, trial);
        //_timerCoroutine = TaskInstanceDurationTimer();
        taskDisplayCoroutine = StartCoroutine(DisplayTaskElements(stimulus, playSignal, trial));
        
        //TODO: both coroutine under here are now started after successful fixation. Keep it like that?
        //_taskInputCoroutine = StartCoroutine(WaitForInput(randomArrow, playSignal, trial));
        //_taskTimerCoroutine = StartCoroutine(nameof(TaskInstanceDurationTimer));
        
        
        
        //StartCoroutine(DisplayTaskElements(randomArrow, playSignal, trial));
        //StartCoroutine(_inputCoroutine);
        //StartCoroutine(_timerCoroutine);

        yield return new WaitUntil(() => interruptTask); // wait until input detected or time ran out

        if (currentResultCode != (int) ResultCodes.Early)
        {
            if (!reactedToTrial)
            {
                if (!playSignal)
                {
                    activeResultCodes[(int) ResultCodes.None] = true;
                    currentResultCode = (int) ResultCodes.None;
                }
                else
                {
                    activeResultCodes[(int) ResultCodes.GoodNoGo] = true;
                    currentResultCode = (int) ResultCodes.GoodNoGo;
                }
            }
        } 
        
        //StopCoroutine(_inputCoroutine);
        //StopCoroutine(_timerCoroutine);
        
        // All following coroutines finished at this point
        // - timer
        // - input
        // - display
        // So it is time to show result, wait for further input 
        
        // Case 1: Signal showed, no reaction => Good
        // Case 2: Signal showed, reaction => Bad
        // Case 3: No Signal, no reaction => Bad
        // Case 4: No Signal, wrong reaction => Bad
        // Case 5: No Signal, right reaction => Good
        
        // Right now _currentResultCode can only be on of (0,1,3,4,7,8)
        switch (currentResultCode)
        {
            case 0: // Good-GO
                successfulTaskInstance = true;

                break;
            case 1: // Good-No-Go
                successfulTaskInstance = true;
                IncreaseDelay();

                break;
            case (int) ResultCodes.Early when playSignal://Bad-Early with signal
                DecreaseDelay();
                break;
                
            case 3: case 7: // Bad-None,  Bad-Wrong-Direction
                break;
            case 8: // Bad-No-Go

                break;
        }
        
        // Bad task and no trial, decrease pizza handler
        if (!successfulTaskInstance && !trial)
        {
            if (handlePositionIndex != 0)
                handlePositionIndex--;
            
            Vector3 handlePosition = handleTransform.localPosition;
            handlePosition.x = handlePositions[handlePositionIndex];
            handleTransform.localPosition = handlePosition;
        }
        
        //Display result
        taskUI.Call_ShowResult(resultDuration, successfulTaskInstance, currentResultCode, trial, !playSignal);
        
        //TODO: Remove input after task?
        // Coroutine to detect input after already showing result
        IEnumerator inputAfterTask = InputAfterTask(trial);
        StartCoroutine(inputAfterTask);
        
        yield return new WaitForSeconds(resultDuration);
        
        taskUI.HideResult();
        StopCoroutine(inputAfterTask);
        
        taskArrayIndex++;
        currentTaskInstance.FinalResultCode = currentResultCode;
        currentTaskInstance.trialIsCorrect = successfulTaskInstance;

        if (taskStartTime != -1 && taskReactionTime != -1)
        {
            float rt = DistanceBetweenDatesInMs(taskStartTime, taskReactionTime);
            reactionTimes.Add(rt);
            currentTaskInstance.ReactionTime = rt;
        }
        else
        {
            currentTaskInstance.ReactionTime = -1;
        }

        if (playSignal)
        {
            signalTrials.Add(successfulTaskInstance);
        }
        else
        {
            goTrials.Add(successfulTaskInstance); 
        }
        
        
        //TODO: Trial, GlobalTaskCount, LocalTaskCount, Stimulus, StimulusDisplay, Signal, SSD, Reaction, ReactionTime, Correct
        
        CSVWriter.Instance.AddEntryToSst(currentTaskInstance.ToCsvString());

        currentTaskFinished = true;

        if (taskArrayIndex > taskArray.Length - 1 && !trial)
        {
            roundManager.UserIsDelivering = false;
            timer.SetActive(false);
            pizzaHeatSlider.SetActive(false);
        }
    }

    private IEnumerator TaskInstanceDurationTimer()
    {
        yield return new WaitForSeconds(taskDuration);
        
        StopCoroutine(taskInputCoroutine);
        StopCoroutine(taskDisplayCoroutine);
        taskUI.HideArrowAndFixation();

        currentTaskInstance.Reaction = "none";
        
        interruptTask = true;
    }

    private IEnumerator InputAfterTask(bool isTrial) // Formerly signal and result as parameters ??
    {
        anyInput = false;
        anyTimeRanOut = false;
        IEnumerator inputCoroutine = WaitForAnyInput();
        IEnumerator timerCoroutine = AnyInputTimer(0.5f);
        
        StartCoroutine(inputCoroutine);
        StartCoroutine(timerCoroutine);

        yield return new WaitUntil(() => anyInput || anyTimeRanOut);
        
        //int[] codesClass = {0, 4, 7, 8};//TODO: Correct class? Good tasks can only be 0 and 1 -> {0,4,7,8} are all codes with previous input, so i guess it is ok
        int[] codesClass = {0, 1}; // good class
        
        int resultCode;
        
        if (anyInput)
        {
            if (currentResultCode == (int) ResultCodes.None)
            {
                //_activeResultCodes[(int) ResultCodes.GoodThenBad] = true; //TODO: Why true? "None" refers to bad tasks where no input where given despite required.
                resultCode = (int) ResultCodes.Late;
                taskUI.Call_ShowResult(resultDuration - 0.5f, false, resultCode, isTrial, !currentTaskInstance.SignalAppeared);
            } 
            else if (codesClass.Contains(currentResultCode)) // there was input before
            {
                if(GetTrueIndicesFromArray(activeResultCodes).Count == 1 && activeResultCodes[(int) ResultCodes.GoodGo])
                    activeResultCodes[(int) ResultCodes.GoodThenBad] = true;
                resultCode = (int) ResultCodes.Multiple;
                taskUI.Call_ShowResult(resultDuration-0.5f, false, resultCode, isTrial, !currentTaskInstance.SignalAppeared); 
            }
            successfulTaskInstance = false;
        }
        else
        {
            StopCoroutine(inputCoroutine);
            StopCoroutine(timerCoroutine);
        }
    }
    
    private List<int> GetTrueIndicesFromArray(bool[] arr)
    {
        var indicesList = new List<int>();
        for (int index = 0; index < arr.Length; index++)
        {
            if(arr[index])
                indicesList.Add(index);
        }

        return indicesList;
    }


    private IEnumerator AnyInputTimer(float time)
    {
        yield return new WaitForSeconds(time);
        anyTimeRanOut = true;
    }
    private IEnumerator WaitForAnyInput()
    {
        /*yield return new WaitUntil(() => 
            OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)  
            | OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));*/
            
        yield return new WaitUntil(() => 
            primaryIndexPressed && primaryIndexReleased
            || secondaryIndexPressed && secondaryIndexReleased);

        int direction;
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            direction = -1;
            primaryIndexReleased = false;
        }
        else
        {
            direction = 1;
            secondaryIndexReleased = false;
        }
        
        currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), direction));

        anyInput = true;
    }
    

    private void IncreaseDelay()
    {
        currentDelay += delayOptimiseValue;
        if (currentDelay > delayUpperBound)
            currentDelay = delayUpperBound;
    }

    private void DecreaseDelay()
    {
        currentDelay -= delayOptimiseValue;
        if (currentDelay <= 0.001)
            currentDelay = 0;
    }

    private IEnumerator DisplayTaskElements(int arrowDirection, bool playSignal, bool isTrial)
    {
        currentTaskInstance.StimulusDisplay = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        StartCoroutine(nameof(InputDuringFixation));
        // Show Fixation for 0.5s 
        taskUI.ShowFixation();
        yield return new WaitForSeconds(0.5f);
        taskUI.HideFixation();
        StopCoroutine(nameof(InputDuringFixation));
        
        //function can only get here if there was no input during fixation
        taskInputCoroutine = StartCoroutine(WaitForInput(arrowDirection, playSignal, isTrial));
        taskTimerCoroutine = StartCoroutine(nameof(TaskInstanceDurationTimer));

        taskStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // Show Left or Right Arrow
        if (arrowDirection == -1)
        {
            taskUI.ShowLeftArrow();
        }
        else
        {
            taskUI.ShowRightArrow();
        }
        if (playSignal)
        {
            taskUI.Call_PlaySignalWithDelay(currentDelay, isTrial);
        }
        
        yield return new WaitUntil(() => interruptTask);
        
        if (arrowDirection == -1)
        {
            taskUI.HideLeftArrow();
        } 
        else
        {
            taskUI.HideRightArrow();
        }
    }

    // Detect input during fixation
    private IEnumerator InputDuringFixation() //TODO: pressing early results in wrong error code, Multiple instead of Early
    {
        yield return new WaitUntil(() => secondaryIndexPressed && secondaryIndexReleased || primaryIndexPressed && primaryIndexReleased );
        
        StopCoroutine(taskInputCoroutine);
        StopCoroutine(nameof(WaitForInput));
        StopCoroutine(taskTimerCoroutine);
        StopCoroutine(nameof(TaskInstanceDurationTimer));
        StopCoroutine(taskDisplayCoroutine);
        StopCoroutine(nameof(DisplayTaskElements));
        taskUI.HideArrowAndFixation();
        currentResultCode = (int) ResultCodes.Early;
        interruptTask = true; // input detected, start showing result
    }

    private IEnumerator WaitForInput(int arrowDirection, bool signalTask, bool isTrial)
    {
        /*yield return new WaitUntil(() => 
            OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)  
            | OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));*/
        
        
        yield return new WaitUntil(() => 
            (primaryIndexPressed && primaryIndexReleased) ||
            (secondaryIndexPressed && secondaryIndexReleased));

        taskReactionTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        reactedToTrial = true;

        
        
        /*
        // Jumps into if-clause if button was pressed during fixation: bad
        if (_fixationIsShown)
        {
            _currentResultCode = (int) ResultCodes.Early;
            _activeResultCodes[(int) ResultCodes.Early] = true;
            _currentTaskInstance.reactions.Add(
                new ReactionEntry(CSVWriter.GetTimestamp()
                    , OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ? -1 : 1));
        }
        else*/

        // If task is GO-task
        if (!signalTask)
        {
            switch (arrowDirection)
            {
                // Left arrow GO-Task and left index trigger pressed: good
                case -1 when OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger):
                    primaryIndexReleased = false;
                    activeResultCodes[(int) ResultCodes.GoodGo] = true;
                    currentResultCode = (int) ResultCodes.GoodGo;
                    if (!isTrial)
                        skaterController.MoveLeft();
                    currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), -1));
                    currentTaskInstance.Reaction = "leftArrow";
                    break;
                // Left arrow GO-Task but right index trigger pressed: bad
                case -1 when OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger):
                    secondaryIndexReleased = false;
                    activeResultCodes[(int) ResultCodes.Direction] = true;
                    currentResultCode = (int) ResultCodes.Direction;
                    currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), 1));
                    currentTaskInstance.Reaction = "rightArrow";
                    break;

                // Right arrow GO-Task and right index trigger pressed: good
                case 1 when OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger):
                    secondaryIndexReleased = false;
                    activeResultCodes[(int) ResultCodes.GoodGo] = true;
                    currentResultCode = (int) ResultCodes.GoodGo;
                    if (!isTrial)
                        skaterController.MoveRight();
                    currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), 1));
                    currentTaskInstance.Reaction = "rightArrow";
                    break;
                // Right arrow GO-Task but left index trigger is pressed: bad
                case 1 when OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger):
                    primaryIndexReleased = false;
                    activeResultCodes[(int) ResultCodes.Direction] = true;
                    currentResultCode = (int) ResultCodes.Direction;
                    currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), -1));
                    currentTaskInstance.Reaction = "leftArrow";
                    break;
            }
        }
        else // Task is a No-GO task
        {
            activeResultCodes[(int) ResultCodes.Signal] = true;
            currentResultCode = (int) ResultCodes.Signal;
            DecreaseDelay();

            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                primaryIndexReleased = false;
                currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), 1));
                currentTaskInstance.Reaction = "leftArrow";
            }
            else
            {
                secondaryIndexReleased = false;
                currentTaskInstance.Reactions.Add(new ReactionEntry(CSVWriter.GetTimestamp(), 1));
                currentTaskInstance.Reaction = "rightArrow";
            }
        }
        
        
        StopCoroutine(taskTimerCoroutine);
        StopCoroutine(taskDisplayCoroutine);
        taskUI.HideArrowAndFixation();
        interruptTask = true; // input detected, start showing result
    }

    public void InitializeExposure()
    {
        roundManager.StartGame();
    }

    public void ResetTaskSeries()
    {
        taskArray = GetTaskArray(true, taskAmount, 0.25f);
        taskArrayIndex = 0;

        timer.SetActive(true);
        timer.transform.localPosition = new Vector3(12f,7f,0f);
        exposureTimer.StartCountDown(80, 35, 20);
        
        pizzaHeatSlider.SetActive(true);

        handlePositionIndex = handlePositions.Length - 1;
        Vector3 pos = handleTransform.localPosition;
        pos.x = handlePositions[handlePositionIndex];
        handleTransform.localPosition = pos;
        
        taskUI.HideAll();
        
        Hashtable props = new Hashtable
        {
            {RoomProperty.NewTaskArray, true}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

    }

    private bool[] GetTaskArray(bool includeStopSignal, int length, float signalProbability)
    {
        bool[] taskArray = new bool[length];
        var prob = 1 / signalProbability;
        var stopSignalTaskAmount = length / prob;

        for(var i = 0; i < taskArray.Length; i++)
        {
            taskArray[i] = false;
        }

        if (!includeStopSignal)
        {
            return taskArray;
        }

        while (stopSignalTaskAmount != 0)
        {
            var random = Random.Range(0, taskArray.Length-1);
            if (!taskArray[random])
            {
                taskArray[random] = true;
                stopSignalTaskAmount--;
            }
        }

        return taskArray;
    }
    
    private float DistanceBetweenDatesInMs(long start, long end)
    {
        DateTimeOffset dto1 = DateTimeOffset.FromUnixTimeMilliseconds(start);
        DateTimeOffset dto2 = DateTimeOffset.FromUnixTimeMilliseconds(end);

        DateTime dt1 = dto1.UtcDateTime;
        DateTime dt2 = dto2.UtcDateTime;

        TimeSpan span = dt2 - dt1;

        float ms = (float) span.TotalMilliseconds;

        return ms;
    }

    public float GetAverageReactionTime()
    {
        return reactionTimes.Sum() / (float) reactionTimes.Count;
    }

    public float GetGoTaskPercentage()
    {
        float counter = 0;
        foreach (var trial in goTrials)
        {
            if (trial)
                counter+=1f;
        }
        return counter / goTrials.Count;
    }

    public float GetStopTaskPercentage()
    {
        float counter = 0;
        foreach (var trial in signalTrials)
        {
            if (trial)
                counter+=1f;
        }
        return counter / signalTrials.Count;
    }
}
