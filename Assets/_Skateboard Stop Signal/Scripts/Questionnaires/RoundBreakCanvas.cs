using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoundBreakCanvas : MonoBehaviourPunCallbacks
{
    [SerializeField] private bool isSinglePlayer = false;
    
    private PhotonView view;

    [SerializeField] private RoundManager roundManager;

    [SerializeField] private GameObject canvasScreenTextGameObject;
    private TMP_Text canvasScreenText;

    [SerializeField] private GameObject feedbackButtonGameObject;
    private Button feedbackButton;
    
    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnButton;
    
    private int _instructionsCounter;
    private readonly Dictionary<int, string> instructionsDictionaryGer = new Dictionary<int, string>
    {
        {0, "Sehr gut, die Pizza wurde ausgeliefert. Zeit für zwei kurze Zwischenfragen bevor es weitergeht."}
    };
    private readonly Dictionary<int, string> instructionsDictionaryEng = new Dictionary<int, string>
    {
        {0, "Very good, the pizza is delievered. Before we continue it is time for two short questions."}
    };

    private Dictionary<int, string> currentInstructionsDictionary;
    
    private bool isLanguageGerman;
    
    // Phases for better flow control
    private enum Phase
    {
        Instructions, // text instructions in the beginning and at the end
        Questionnaire, // questionnaire
        Feedback, // uni-directional (optional, depending on condition) feedback
        Finish // Used to end break session
    }

    private Phase currentPhase;

    private bool feedbackReceivedThisBreak;

    private Hashtable roomProperties = new Hashtable();
    
    // Questionnaire stuff below
    private Questionnaire questionnaire;
    private Question[] questions;
    private Question currentQuestion;
    private int questionCounter;

    public TextAsset[] questionnaireFile = new TextAsset[1];
    private string currentCode;

    // Elements for visualising a question
    [SerializeField] private GameObject likertScaleGameObject;
    private LikertScale _likertScale;
    [SerializeField] private TMP_Text instruction;
    [SerializeField] private TMP_Text statement;
    [SerializeField] private TMP_Text note;
    
    // Timer visualisation
    [SerializeField] private GameObject timerGameObject;
    [SerializeField] private TMP_Text timerText;
    
    // Start is called before the first frame update
    private void Start()
    {
        view = GetComponentInParent<PhotonView>();

        if (!view.IsMine)
        {
            Destroy(gameObject);
            return;
        }
        
        
        canvasScreenText = canvasScreenTextGameObject.GetComponent<TMP_Text>();
        isLanguageGerman = GameManager.Instance.Language == GameManager.Languages.Deutsch;

        currentInstructionsDictionary = isLanguageGerman ? instructionsDictionaryGer : instructionsDictionaryEng;

        feedbackButton = feedbackButtonGameObject.GetComponent<Button>();
        _likertScale = likertScaleGameObject.GetComponent<LikertScale>();
    }

    public void StartBreak()
    {
        isLanguageGerman = GameManager.Instance.Language == GameManager.Languages.Deutsch;
        
        timerGameObject.SetActive(false);

        currentInstructionsDictionary = isLanguageGerman ? instructionsDictionaryGer : instructionsDictionaryEng;
        
        if (!canvasScreenText)
            canvasScreenText = canvasScreenTextGameObject.GetComponent<TMP_Text>();
        SwitchCanvasModeToQuestionnaire(false);
        _instructionsCounter = 0;
        feedbackButtonGameObject.SetActive(false);
        currentPhase = Phase.Instructions;
        SetInstruction();
    }

    public void ContinueButtonPressed()
    {
        switch (currentPhase)
        {
            case Phase.Instructions:
                // Reached the final instruction, start questionnaire
                if (_instructionsCounter == currentInstructionsDictionary.Count - 1)
                {
                    StartQuestionnaire();
                }
                else
                {
                    _instructionsCounter++;
                    SetInstruction();
                }
                break;
            case Phase.Questionnaire:
                questionCounter++;
                // Reached final question, user can confirm questions or review questions again
                if (questionCounter == questions.Length)
                {
                    ConfirmFinishingQuestionnaire();
                } 
                else if (questionCounter > questions.Length) // Finishing questionnaire has been confirmed
                {
                    FinishQuestionnaire();
                }
                else // Otherwise set new question
                {
                    SetQuestion();
                }
                break;
            case Phase.Feedback:
                if (!feedbackReceivedThisBreak)
                {
                    StartFeedbackSegment();
                }
                else
                {
                    //TODO: else part moved to OnRoomPropertiesUpdate
                }
                break;
            case Phase.Finish:
                if (roundManager.RoundCounter <= 2)
                {
                    StartCoroutine(nameof(BreakTimer));
                }
                else
                {
                    roundManager.StartGame();
                }
                
                break;
        }
    }

    private IEnumerator BreakTimer()
    {
        //Disable Buttons
        continueButton.interactable = false;
        returnButton.interactable = false;
        timerGameObject.SetActive(true);
        
        var time = 15f;
        canvasScreenText.SetText( isLanguageGerman
            ? "Gleich geht es weiter mit der nächsten Pizza! Denke daran:\n1. Reagiere so schnell du kannst auf die Richtungsanweisung des Navi.\n \n2. Versuche nicht, den Ton abzuwarten.\n \n3. Drücke nicht wenn du den Ton hörst."
            : "You will continue with the next pizza soon! Remember:\n1.React as fast as possible to the directions of the navi.\n \n2. Don't try to await the sound.\n \n3. Don't react when you hear the sound."
        );

        var postFix = isLanguageGerman 
            ? "\nSekunden." 
            : "\nseconds.";
        
        while (time >= 0f)
        {
            if (time > 5f)
            {
                timerText.SetText((int) time + postFix);
            }
            else
            {
                timerText.SetText(time.ToString("F2") + postFix);
            }
            
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }

        var props = new Hashtable {{RoomProperty.CurrentCanvas, AssistantExposureUI.CanvasMode.None}};
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        
        timerGameObject.SetActive(false);
        roundManager.StartGame();
    }

    public void ReturnButtonPressed()
    {
        switch (currentPhase)
        {
            case Phase.Instructions:
                _instructionsCounter--;
                SetInstruction();
                break;
            case Phase.Questionnaire:
                questionCounter--;
                SetQuestion();
                break;
            default:
                Debug.LogWarning("ReturnButtonPressed when neither instructions phase nor questionnaire was active. Have a look into this!");
                break;
        }
    }

    public void FeedbackButtonPressed()
    {
        feedbackButton.interactable = false;
        StartCoroutine(DemandVerbalFeedback());
    }

    private void ConfirmFinishingQuestionnaire()
    {
        instruction.text = "";
        note.text = "";
        statement.text = isLanguageGerman 
            ? "Das waren alle Fragen.\nDu kannst sie dir noch einmal angucken oder auf \"Weiter\" klicken.\nWenn du auf \"Weiter\" klickst werden deine Antworten gespeichert und du kannst sie nicht mehr bearbeiten." 
            :"That's about it.\nYou can check your answers or press \"Continue\".\nIf you press \"Contine\" your answers are saved and you cannot edit them.";
        likertScaleGameObject.SetActive(false);

        returnButton.interactable = true;
        continueButton.interactable = true;
    }

    private void SetInstruction()
    {
        //TODO: use to set other instructions too, after the questionnaire
        //TODO: or use other function with own file? <-- Yes
        continueButton.interactable = _instructionsCounter <= currentInstructionsDictionary.Count - 1;
        returnButton.interactable = _instructionsCounter != 0;

        if (currentInstructionsDictionary.TryGetValue(_instructionsCounter, out var textValue))
        {
            canvasScreenText.SetText(textValue);
        }
    }

    private void StartFeedbackSegment()
    {
        returnButton.interactable = false;
        continueButton.interactable = false;

        string newScreenText;
        switch (roundManager.RoundCounter)
        {
            case 1:
                newScreenText = isLanguageGerman 
                    ? "Bevor du die zweite Pizza auslieferst ist es an der Zeit, deine Leistung im Umgang mit dem Navi zu bewerten.\n \nDer Beobachter wird dir dazu etwas Feedback geben. Klicke auf \"Feedback\" um es dir anzuhören. Du wirst es dir nur einmal anhören können."
                    : "It is time to rate you performance with the navi before you deliver the second pizza.\n \nThe observer will give you some feedback. Press \"Feedback\" to hear it. You can only listen to it once.";
                break;
            case 2:
                newScreenText = isLanguageGerman
                    ? "Bevor du die letzte Pizza auslieferst wird deine Leistung ein zweites Mal bewertet.\n \nDer Beobachter wird dir wieder etwas Feedback geben. Klicke auf \"Feedback\" um es dir anzuhören. Du wirst es dir nur einmal anhören können."
                    : "It is time to rate your performance a seconds time before you deliver the last pizza.\n \nThe observer will give you some feedback again. Press \"Feedback\" to hear it. You can only listen to it once.";
                break;
            case 3:
                newScreenText = isLanguageGerman
                    ? "Der Beobachter wird dir nun ein letztes Mal Feedback geben.\n \nKlicke auf \"Feedback\" um es dir anzuhören. Du wirst es dir nur einmal anhören können."
                    : "The observer will give you some feedback for the last time.\n \nPress \"Feedback\" to hear it. You can only listen to it once.";
                break;
            default:
                newScreenText = isLanguageGerman
                    ? "Zeit für eine Bewertung deiner Leistung durch den Beobachter..\n \nKlicke auf \"Feedback\" um es dir anzuhören. Du wirst es dir nur einmal anhören können."
                    : "The observer will rate you performance with the navi.\n \nPress \"Feedback\" to hear it. You can only hear it once.";
                break;
        }
        
        canvasScreenText.SetText(newScreenText);

        feedbackButtonGameObject.SetActive(true);
        feedbackButton.enabled = true;
        feedbackButton.interactable = true;
    }

    private IEnumerator DemandVerbalFeedback()
    {
        var properties = new Hashtable
        {
            {RoomProperty.FeedbackGiven, false}
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

        AgoraLauncher.App.MuteRemoteUser(false);
        
        //TODO: Get Feedback, roomProperty bool?
        //TODO: set room property "Feedback" to true from assistant
        yield return new WaitUntil(() => feedbackReceivedThisBreak || isSinglePlayer);
        //yield return new WaitForSeconds(5f);
        feedbackReceivedThisBreak = true;
        
        AgoraLauncher.App.MuteRemoteUser(true);
        feedbackButtonGameObject.SetActive(false);
        continueButton.interactable = true;
    }

    private void SwitchCanvasModeToQuestionnaire(bool qrActive)
    {
        // full canvas text not active in questionnaire
        canvasScreenText.enabled = !qrActive;
        
        // elements for questionnaire
        instruction.enabled = qrActive;
        statement.enabled = qrActive;
        note.enabled = qrActive;
        likertScaleGameObject.SetActive(qrActive);
    }

    private void StartQuestionnaire()
    {
        var eventString = "Q1." + roundManager.RoundCounter + "Start";
        CSVWriter.Instance.AddEntryToGeneral(eventString, "none");

        currentPhase = Phase.Questionnaire;

        returnButton.interactable = false;
        SwitchCanvasModeToQuestionnaire(true);
        
        questionnaire = new Questionnaire
        {
            condition = GameManager.Instance.Condition,
            participantId = GameManager.Instance.ParticipantID,
            questions = Utilities.Parser.ParseQuestionsFromFiles(questionnaireFile),
            startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        questions = questionnaire.questions;
        questionCounter = 0;
        
        roomProperties = new Hashtable
        {
            {RoomProperty.QuestionnaireIsOn, true},
            {RoomProperty.Questionnaire, "b"},
            {RoomProperty.QuestionCounter, questionCounter}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        currentQuestion = questions[questionCounter]; // set first question
        _likertScale.SetAllLabels(currentQuestion.scaleLabels);
        SetQuestion();
    }
    
    private void FinishQuestionnaire()
    {
        questionnaire.endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var questionnaireId = "q1" + roundManager.RoundCounter;

        var entryString = questionnaireId + "Finished";
        CSVWriter.Instance.AddEntryToGeneral(entryString, "none");

        // Enable full canvas text again
        SwitchCanvasModeToQuestionnaire(false);
        
        // Set button interactables
        continueButton.interactable = true;
        returnButton.interactable = false;

        roomProperties = new Hashtable
        {
            {RoomProperty.QuestionnaireIsOn, false}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        
        // Save questionnaire

        questionnaire.condition = GameManager.Instance.Condition;
        questionnaire.participantId = GameManager.Instance.ParticipantID;

        var filenamePrefix = GameManager.Instance.ParticipantID + "_" + GameManager.Instance.Condition + "_";
        CreationPlayerManager.LocalPlayerInstance.GetComponent<DataPersistence>().Call_SaveFile("Questionnaires\\Questionnaire_1\\", filenamePrefix + questionnaireId + "_" + questionnaire.startTime + "_" + questionnaire.endTime + ".csv", questionnaire.ToString());
        
        
        // Enter feedback phase if condition fits, else continue
        if (GameManager.Instance.Condition == "A")
        {
            
            canvasScreenText.SetText(isLanguageGerman 
                ? "Vielen Dank für deine Antworten. Drücke \"Weiter\" um fortzufahren." 
                : "Thanks for your answers. Press \"Continue\" to go on.");
            currentPhase = Phase.Feedback;
            feedbackReceivedThisBreak = false;
            return;
        } 
        if (roundManager.RoundCounter <= 2) // If no feedback necessary, move directly into finish phase
        { 
            canvasScreenText.SetText(isLanguageGerman
            ? "Vielen Dank für deine Antworten. Drücke nun auf \"Weiter\" um die nächste Pizza auszuliefern."
            : "Thanks for your answers. Press \"Continue\" to deliver the next pizza.");
        }
        else 
        { 
            canvasScreenText.SetText(isLanguageGerman
            ? "Vielen Dank für deine Antworten. Du hast alle Pizzen ausgeliefert! Drücke \"Weiter\" um weiter zu machen."
            : "Thanks for your answers. You delivered all pizzas! Press \"Continue\" to go on.");
        }

        currentPhase = Phase.Finish;
    }

    private void SetQuestion()
    {
        roomProperties = new Hashtable();
        if(!likertScaleGameObject.activeSelf)
            likertScaleGameObject.SetActive(true);
        if(currentQuestion.scale != questions[questionCounter].scale)
            _likertScale.SetAllLabels(questions[questionCounter].scaleLabels);
        currentQuestion = questions[questionCounter];
        if(currentQuestion.showTime == 0)
            currentQuestion.showTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        instruction.text = currentQuestion.instruction;
        statement.text = currentQuestion.question;
        note.text = currentQuestion.note;
        if (currentQuestion.IsAnswered)
        {
            _likertScale.SetAnswer(currentQuestion.answer);
        }
        else
        {
            _likertScale.Reset();
        }
        
        continueButton.interactable = currentQuestion.IsAnswered;
        returnButton.interactable = questionCounter != 0;
        roomProperties.Add(RoomProperty.QuestionCounter, questionCounter);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }
    
    public void SetAnswerToCurrentQuestion(bool value)
    {
        roomProperties = new Hashtable();
        currentQuestion.IsAnswered = value;
        if (currentQuestion.IsAnswered)
        {
            currentQuestion.answer = _likertScale.GetAnswer();
            int activeToggleIndex = _likertScale.GetAnswerAsInt();
            if (activeToggleIndex != -1)
            {
                roomProperties.Add(RoomProperty.LikertValue, _likertScale.GetAnswerAsInt());
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
            }
        }
        continueButton.interactable = value;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue(RoomProperty.FeedbackGiven, out var tmp))
        {
            if (tmp is bool received)
            {
                if (received)
                {
                    if (roundManager.RoundCounter <= 2)
                    {
                        canvasScreenText.SetText(
                            isLanguageGerman ? "Der Beobachter ist mit dem Feedback fertig. Drücke nun auf \"Weiter\" um die nächste Pizza auszuliefern."
                                : "The observer has finished giving you feedback. Press \"Continue\" to deliver the next pizza.");
                    }
                    else 
                    { 
                        canvasScreenText.SetText(
                            isLanguageGerman ? "Der Beobachter ist mit dem Feedback fertig. Da du alle Pizzen ausgeliefert hast drücke nun \"Weiter\" um ein paar finale Fragen zu beantworten."
                                :"The observer has finished giving you feedback. As you have delivered all pizzas you can now press press \"Continue\" to answer some final questions.");
                    }

                    currentPhase = Phase.Finish;
                    
                    feedbackReceivedThisBreak = true;
                    
                }
            }
        }
    }
}
