using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssistantExposureUI : MonoBehaviour
{
    private PhotonView _view;
    
    // Fields for general visualization
    [SerializeField] private GameObject fullCanvasTextGameObject;
    private TMP_Text fullCanvasText;
    
    // Fields for visualization of the task
    [SerializeField] private GameObject leftArrowGameObject;
    [SerializeField] private GameObject rightArrowGameObject;
    private float _arrowDisplayTime;
    public float ArrowDisplayTime
    {
        set => _arrowDisplayTime = value;
    }
    [SerializeField] private GameObject signalImageGameObject;

    [SerializeField] private GameObject resultGoodGameObject;
    [SerializeField] private GameObject resultBadGameObject;
    [SerializeField] private GameObject resultErrorMessageBackgroundGameObject;
    [SerializeField] private GameObject resultErrorMessageGameObject;
    private TMP_Text resultErrorMessage;
    private float _resultDisplayTime;
    public float ResultDisplayTime
    {
        set => _resultDisplayTime = value;
    }

    [SerializeField] private AudioSource audioSource;
    
    //Fields for visualization of questionnaire
    [SerializeField] private GameObject statementGameObject;
    private TMP_Text statement;
    [SerializeField] private GameObject likertScaleGameObject;
    private AssistantLikertScale likertScale;

    // Fields for feedback
    [SerializeField] private Sprite feedbackEnabledSymbol;
    [SerializeField] private Sprite feedbackDisabledSymbol;
    [SerializeField] private GameObject feedbackGivenButtonGameObject;
    private Button feedbackGivenButton;
    [SerializeField] private GameObject feedbackSymbolGameObject;
    private Image feedbackSymbolImage;
    
    
    // Different modes enable different canvas objects. Buttons are only visualized on the participant-side so we don't care about them
    // Basic: only one full-canvas text element, used for explanations
    // Questionnaire: multiple small ui elements covering the canvas
    public enum CanvasMode
    {
        Basic,
        Questionnaire,
        None
    }

    private CanvasMode _currentCanvasMode;
    public CanvasMode CurrentCanvasMode
    {
        get => _currentCanvasMode;
    } 

    private void Awake()
    {
        _view = GetComponent<PhotonView>();

        if (!_view.IsMine)
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        statement = statementGameObject.GetComponent<TMP_Text>();
        fullCanvasText = fullCanvasTextGameObject.GetComponent<TMP_Text>();
        
        if (resultErrorMessageGameObject != null && resultErrorMessageGameObject.GetComponent<TMP_Text>())
        {
            resultErrorMessage = resultErrorMessageGameObject.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogError("GameObject for result message not set or missing component! Fix or null-pointer exceptions will occur.");
        }

        if (likertScaleGameObject != null && likertScaleGameObject.GetComponent<AssistantLikertScale>())
        {
            likertScale = likertScaleGameObject.GetComponent<AssistantLikertScale>();
        }
        else
        {
            Debug.LogError("GameObject for likert-scale not set or missing component! Fix or null-pointer exceptions will occur.");
        }

        if (feedbackSymbolGameObject != null && feedbackSymbolGameObject.GetComponent<Image>())
        {
            feedbackSymbolImage = feedbackSymbolGameObject.GetComponent<Image>();
        }
        else
        {
            Debug.LogError("GameObject for feedback symbol not set or missing component! Fix or null-pointer exceptions will occur.");
        }
        
        if (feedbackGivenButtonGameObject != null && feedbackGivenButtonGameObject.GetComponent<Button>())
        {
            feedbackGivenButton = feedbackGivenButtonGameObject.GetComponent<Button>();
        }
        else
        {
            Debug.LogError("GameObject for feedback button not set or missing component! Fix or null-pointer exceptions will occur.");
        }
        
        SetCanvasMode(CanvasMode.None);
    }
    
    /// <summary>
    /// Activates/enables components necessary for effectively using mode. May disable objects and components if mode is CanvasMode.Questionnaire. 
    /// </summary>
    /// <param name="mode">The mode you want to activate.</param>
    public void SetCanvasMode(CanvasMode mode)
    {
        if (_currentCanvasMode == mode)
        {
            Debug.LogWarning($"Canvas mode {mode} is already active.");
            return;
        }

        _currentCanvasMode = mode;
        switch (mode)
        {
            case CanvasMode.Questionnaire:
                fullCanvasTextGameObject.SetActive(false);
                statementGameObject.SetActive(true);
                likertScaleGameObject.SetActive(true);
                likertScale.Reset();
                break;
            case CanvasMode.Basic:
                fullCanvasTextGameObject.SetActive(true);
                statementGameObject.SetActive(false);
                likertScaleGameObject.SetActive(false);
                break;
            case CanvasMode.None:
                fullCanvasTextGameObject.SetActive(false);
                statementGameObject.SetActive(false);
                likertScaleGameObject.SetActive(false);
                break;
            default:
                Debug.LogError("Canvas mode could not be set as it is not in the list of possible modes!");
                break;
        }
    }

    public void SetTextBasicMode(string canvasText)
    {
        if (_currentCanvasMode != CanvasMode.Basic)
            SetCanvasMode(CanvasMode.Basic);
            
        fullCanvasText.SetText(canvasText);
        
    }

    public void SetTextQuestionnaireMode(string questionStatement, int toggleIndex=-1)
    {
        if (_currentCanvasMode != CanvasMode.Questionnaire)
            SetCanvasMode(CanvasMode.Questionnaire);
        statement.SetText(questionStatement);
        if(toggleIndex >= 0 && toggleIndex < 7)
            SetLikertValueQuestionnaireMode(toggleIndex);
        
    }

    public void SetLikertValueQuestionnaireMode(int toggleIndex)
    {
        if (_currentCanvasMode != CanvasMode.Questionnaire)
            SetCanvasMode(CanvasMode.Questionnaire);
        likertScale.SetAnswer(toggleIndex);
    }

    public void ResetLikertScale()
    {
        likertScale.Reset();
    }
    
    private void PlaySignalWithDelay(float delay)
    {
        signalImageGameObject.SetActive(true);
        if(audioSource.isPlaying)
            audioSource.Stop();
        audioSource.PlayDelayed(delay);
        signalImageGameObject.SetActive(false);
    }

    public void ShowArrowForTime(char arrow, bool signal=false, float signalDelay=0.0f)
    {
        StartCoroutine(signal 
            ? ShowArrowForTimeWithSignal(arrow, signalDelay) 
            : ShowArrowForTimeWithoutSignal(arrow));
    }

    private IEnumerator ShowArrowForTimeWithoutSignal(char arrow)
    {
        if (arrow == 'l')
        {
            leftArrowGameObject.SetActive(true);
            yield return new WaitForSeconds(_arrowDisplayTime);
            leftArrowGameObject.SetActive(false);
        }
        else
        {
            
            rightArrowGameObject.SetActive(true);
            yield return new WaitForSeconds(_arrowDisplayTime);
            rightArrowGameObject.SetActive(false);
        }
    }

    private IEnumerator ShowArrowForTimeWithSignal(char arrow, float signalDelay)
    {
        if (arrow == 'l')
        {
            leftArrowGameObject.SetActive(true);
            PlaySignalWithDelay(signalDelay);
            yield return new WaitForSeconds(_arrowDisplayTime);
            leftArrowGameObject.SetActive(false);
        }
        else
        {
            
            leftArrowGameObject.SetActive(true);
            PlaySignalWithDelay(signalDelay);
            yield return new WaitForSeconds(_arrowDisplayTime);
            leftArrowGameObject.SetActive(false);
        }
    }

    public void ShowResult(bool isGood, string message = "")
    {
        StartCoroutine(ShowResultForTimeWithMessage(isGood, message));
    }

    private IEnumerator ShowResultForTime(bool isResultGood)
    {
        var resultGameObject = isResultGood ? resultGoodGameObject : resultBadGameObject;
        resultGameObject.SetActive(true);
        yield return new WaitForSeconds(_resultDisplayTime);
        resultGameObject.SetActive(false);
    }

    // Default result value is set to false as apart from introduction sessions its only used for bad tasks
    private IEnumerator ShowResultForTimeWithMessage(bool isResultGood, string message)
    {
        var resultGameObject = isResultGood ? resultGoodGameObject : resultBadGameObject;
        resultGameObject.SetActive(true);
        
        resultErrorMessageBackgroundGameObject.SetActive(true);
        resultErrorMessageGameObject.SetActive(true);
        resultErrorMessage.SetText(message);

        yield return new WaitForSeconds(_resultDisplayTime);

        resultErrorMessageBackgroundGameObject.SetActive(false);
        resultErrorMessageGameObject.SetActive(false);
        
        resultGameObject.SetActive(false);
    }

    /// <summary>
    /// Makes feedback button interactable and sets symbol. Symbol makes clear whether audio is enabled and pressing the feedback button alerts the participant that feedback is finished
    /// </summary>
    /// <param name="interactable">Symbolize and enable audio feedback if true</param>
    public void SetFeedbackButton(bool interactable)
    {
        feedbackGivenButton.interactable = interactable;
        feedbackSymbolImage.sprite = interactable ? feedbackEnabledSymbol : feedbackDisabledSymbol;
    }
}
