using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; 

/// <summary>
/// Manages the playthrough of the "exposure". After each round of tasks a small questionnaire appears and depending on the condition the user receives feedback from the observer. 
/// </summary>
public class RoundManager : MonoBehaviour
{
    private PhotonView view;
    
    [SerializeField] private int amountOfRounds = 2;
    private int _roundCounter = 0;
    public int RoundCounter => _roundCounter;

    [SerializeField] private SkyboxTransition skyBoxTransition;
    
    [SerializeField] private TaskLogic taskLogic;

    [SerializeField] private GameObject roundBreakCanvasGameObject;
    private RoundBreakCanvas roundBreakCanvas;

    [SerializeField] private GameObject finalQuestionnaireCanvasGameObject;

    [SerializeField] private GameObject uiHelpers;

    private bool _userIsDelivering;
    public bool UserIsDelivering
    {
        get => _userIsDelivering;
        set => _userIsDelivering = value;
    }

    private bool _currentPlayThroughIsFinished;

    [SerializeField] private SkaterController skaterController;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
            return;
        }
        
        roundBreakCanvas = roundBreakCanvasGameObject.GetComponent<RoundBreakCanvas>();
    }

    public void StartGame()
    {
        roundBreakCanvasGameObject.SetActive(false);
        PlayGame();
    }

    private void PlayGame()
    {
        _roundCounter++;
        if (_roundCounter <= amountOfRounds)
        {
            StartCoroutine(PlaySingleRound());
        }
        else
        {
            FinishGame();
        }
        
    }

    private IEnumerator PlaySingleRound()
    {
        uiHelpers.SetActive(false);
        roundBreakCanvasGameObject.SetActive(false);
        
        skaterController.RidingEnabled = true;
        _userIsDelivering = true;
        
        taskLogic.ResetTaskSeries();
        
        yield return new WaitUntil(() => !_userIsDelivering);
        // user is done with current task amount
        // teleport back into middle and start break session


        // Smooth visual transition of resetting player and start break
        OVRScreenFade.instance.FadeOut(0.5f);
        yield return new WaitForSeconds(0.5f);
        skaterController.RidingEnabled = false;
        skaterController.ResetPlayerPosition();
        OVRScreenFade.instance.FadeIn(1.5f);


        Hashtable _props = new Hashtable
        {
            {RoomProperty.ReactionTimeAverage, taskLogic.GetAverageReactionTime()},
            {RoomProperty.GoTaskPerformance, taskLogic.GetGoTaskPercentage()},
            {RoomProperty.StopTaskPerformance, taskLogic.GetStopTaskPercentage()}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(_props);
        
        if (_roundCounter == amountOfRounds)
            skyBoxTransition.StartDayToNightTransition();
        
        uiHelpers.SetActive(true);
        
        roundBreakCanvasGameObject.SetActive(true);
        roundBreakCanvas.StartBreak();
    }

    private void FinishGame()
    {
        roundBreakCanvasGameObject.SetActive(false);
        finalQuestionnaireCanvasGameObject.SetActive(true);
    }
}
