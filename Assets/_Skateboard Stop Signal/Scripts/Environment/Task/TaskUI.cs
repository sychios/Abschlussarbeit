using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class TaskUI : MonoBehaviour
{
    private PhotonView _view;
    
    // File for instructions of the task introduction
    [SerializeField] private TextAsset exposureIntroductionInstructionsFile;
    [SerializeField] private TextAsset taskResultMessagesFile;
    private Dictionary<int, string> exposureIntroductionInstructionsGer;
    private Dictionary<int, string> exposureIntroductionInstructionsEng;
    private Dictionary<int, string> currentInstructionsDictionary;
    private Dictionary<int, string> taskResultMessagesGer;
    private Dictionary<int, string> taskResultMessagesEng;
    private Dictionary<int, string> currentTaskResultMessages;
    
    // Settings file
    
    
    // UI elements for task visualization
    [SerializeField] private GameObject leftArrowGameObject;
    [SerializeField] private GameObject rightArrowGameObject;

    [SerializeField] private GameObject resultGood;
    [SerializeField] private GameObject resultBad;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject resultTextBackground;

    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject instructionTextBackground;
    
    [SerializeField] private GameObject fixationGameObject;
    [SerializeField] private AudioSource signalAudioSource;

    //private readonly Color _resultSuccessColor = Color.green;
    //private readonly Color _resultMistakeColor = Color.red;

    private readonly float fixationDuration = 0.5f;
    private readonly float arrowDuration = 1f;

    private bool isLanguageGerman;
    
    private string instruction = "";

    private bool _exampleRunning;
    public bool ExampleRunning
    {
        get => _exampleRunning;
        set => _exampleRunning = value;
    }

    private void Awake()
    {
        signalAudioSource = GetComponent<AudioSource>();

        _view = gameObject.GetComponent<PhotonView>();

        if (!_view.IsMine)
        {
            Destroy(this);
            return;
        }

        var dicts = Utilities.Parser.ParseBilingualDictionariesFromFile(exposureIntroductionInstructionsFile);
        var errorDicts = Utilities.Parser.ParseBilingualDictionariesFromFile(taskResultMessagesFile);
        exposureIntroductionInstructionsGer = dicts[0];
        exposureIntroductionInstructionsEng = dicts[1];
        taskResultMessagesGer = errorDicts[0];
        taskResultMessagesEng = errorDicts[1];

        if (GameObject.Find("GameManager").GetComponent<GameManager>().Language == GameManager.Languages.Deutsch)
        {
            currentInstructionsDictionary = exposureIntroductionInstructionsGer;
            currentTaskResultMessages = taskResultMessagesGer;
            isLanguageGerman = true;
        }
        else
        {
            currentInstructionsDictionary = exposureIntroductionInstructionsEng;
            currentTaskResultMessages = taskResultMessagesEng;
            isLanguageGerman = false;
        }
    }

    public void HideArrowAndFixation()
    {
        HideFixation();
        HideLeftArrow();
        HideRightArrow();
    }

    public void HideAll()
    {
        HideFixation();
        HideLeftArrow();
        HideRightArrow();
        HideResult();
    }

    public void SetInstruction(int i)
    {
        currentInstructionsDictionary.TryGetValue(i, out instruction);
        
        var table = new Hashtable
        {
            {RoomProperty.CanvasInstructionCounter, i}
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(table);

        instructionText.text = instruction;
        if (i == currentInstructionsDictionary.Count)
        {
            instructionText.gameObject.SetActive(false);
            instructionTextBackground.SetActive(false);
            table = new Hashtable
            {
                {RoomProperty.CurrentCanvas, AssistantExposureUI.CanvasMode.None}
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
    }
    public void Call_PlaySignalWithDelay(float pDelay, bool pTrial)
    {
        StartCoroutine(PlaySignalWithDelay(pDelay, pTrial));
    }
    
    public IEnumerator PlaySignalWithDelay(float delay, bool trial)
    {
        yield return new WaitForSeconds(delay);
        signalAudioSource.Play();
        
        if(trial)
            ExampleRunning = false;
    }

    public void ShowFixation()
    {
        fixationGameObject.gameObject.SetActive(true);
    }

    public void HideFixation()
    {
        fixationGameObject.gameObject.SetActive(false);
    }

    public void ShowLeftArrow()
    {
        leftArrowGameObject.gameObject.SetActive(true);
    }

    public void ShowRightArrow()
    {
        rightArrowGameObject.gameObject.SetActive(true);
    }

    public void HideLeftArrow()
    {
        leftArrowGameObject.gameObject.SetActive(false);
    }

    public void HideRightArrow()
    {
        rightArrowGameObject.gameObject.SetActive(false);
    }

    public void Call_ShowFixationAndArrow()
    {
        StartCoroutine(ShowFixationAndArrow());
    }
    private IEnumerator ShowFixationAndArrow()
    {
        yield return new WaitForSeconds(1f);
        ShowFixation();
        yield return new WaitForSeconds(fixationDuration);
        HideFixation();

        var r = Random.Range(0f, 1f);

        if (r <= 0.5f)
        {
            ShowLeftArrow();
            yield return new WaitForSeconds(arrowDuration);
            HideLeftArrow();
        }
        else
        {
            ShowRightArrow();
            yield return new WaitForSeconds(arrowDuration);
            HideRightArrow();
        }

        ExampleRunning = false;
    }

    /*public void Call_ShowResult(float time, bool success)
    {
        StartCoroutine(ShowResultWithoutMessage(time, success));
    }*/

    public void Call_ShowResult(float time, bool success, int pResultMessageID, bool isTrial, bool isGo)
    {
        
        var goSubString = isGo ? "y" : "n";
        var successSubString = success ? "t" : "f";
        var propString = goSubString + successSubString + pResultMessageID;
        var properties = new Hashtable
        {
            {RoomProperty.Result, propString}
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        
        
        if (isTrial)
        {
            var message = currentTaskResultMessages[pResultMessageID];

            StartCoroutine(ShowResultWithMessage(time, success, message ));
        }
        else
        {
            StartCoroutine(ShowResultWithoutMessage(time, success));
        }
    }
    
    private IEnumerator ShowResultWithoutMessage(float time, bool success)
    {
        if (success)
        {
            resultGood.SetActive(true);
            yield return new WaitForSeconds(time);
            resultGood.SetActive(false);
        } 
        else
        {
            resultBad.SetActive(true);
            yield return new WaitForSeconds(time);
            resultBad.SetActive(false);
        }
    }
    private IEnumerator ShowResultWithMessage(float time, bool success, string resultMessage)
    {
        var currentResult = success ? resultGood : resultBad;
        currentResult.SetActive(true);

        resultText.text = resultMessage;
        resultText.color = success ? Color.green : Color.red;
        resultTextBackground.SetActive(true);
        
        yield return new WaitForSeconds(time);

        HideResult();
    }

    public void HideResult()
    {
        resultGood.SetActive(false);
        resultBad.SetActive(false);
        resultText.text = "";
        resultTextBackground.SetActive(false);
    }

    public void DisableScreenText()
    {
        instructionText.gameObject.SetActive(false);
        instructionTextBackground.SetActive(false);
    }

    public void SetInstructionTextManually(string text)
    {
        instructionText.SetText(text);
    }
}
