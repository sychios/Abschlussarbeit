using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreationCanvas : MonoBehaviour
{
    // File for the instructions
    [SerializeField] private TextAsset creationCanvasInstructionsFile;
    
    // Language dictionaries containing all instructions
    private Dictionary<int, string> instructionsDictionaryGer;
    private Dictionary<int, string> instructionDictionaryEng;
    private Dictionary<int, string> instructionsDictionary;

    // Questionnaire elements
    private Questionnaire questionnaire;
    private Question[] questions;
    private Question currentQuestion;
    private int questionCounter;

    public TextAsset nasaTlxGerman;
    public TextAsset nasaTlxEnglish;
    public TextAsset[] files;
    
    // elements for after creation questionnaire
    [SerializeField] private GameObject sevenPointLikertScaleGameObject;
    private LikertScale _7PointLikertScale;
    [SerializeField] private GameObject twentyOnePointLikertScaleGameObject;
    private LikertScale _21PointLikertScale;
    private LikertScale currentLikertScale;
    [SerializeField] private TMP_Text screenText;
    [SerializeField] private TMP_Text instruction;
    [SerializeField] private TMP_Text statement;
    [SerializeField] private TMP_Text note;
    
    [SerializeField] private GameObject continueButtonGameObject;
    private Button continueButton;
    private TMP_Text continueButtonLabel;
    [SerializeField] private GameObject returnButtonGameObject;
    private Button returnButton;
    private TMP_Text returnButtonLabel;
    [SerializeField] private GameObject optionalButtonGameObject;
    private Button optionalButton;
    private TMP_Text optionalButtonLabel;
    
    private bool questionnaireActive;
    
    private int instructionCounter;

    private int instructionsLength;
    private string dictionaryValue;
    
    private bool creationFinished;

    private Keyboard keyboard;

    [SerializeField] private GameObject sliderGameObject;
    private Slider slider;
    private bool sliderAnswered;
    private float sliderValue;

    [SerializeField] private Skateboard skateboard;
    
    public GameObject questMarker;

    private bool isLanguageGerman;

    public Canvas introductionCanvas;
    private bool disableIntroductionCanvas;
    
    private Hashtable roomProperties = new Hashtable();
    
    private void Awake()
    {
        //_currentLikertScale = GetComponentInChildren<LikertScale>();
        _7PointLikertScale = sevenPointLikertScaleGameObject.GetComponent<LikertScale>();
        _21PointLikertScale = twentyOnePointLikertScaleGameObject.GetComponent<LikertScale>();
        sevenPointLikertScaleGameObject.SetActive(false);
        twentyOnePointLikertScaleGameObject.SetActive(false);

        keyboard = GameObject.FindWithTag("Keyboard").GetComponent<Keyboard>();

        var dicts = Utilities.Parser.ParseBilingualDictionariesFromFile(creationCanvasInstructionsFile);
        instructionsDictionaryGer = dicts[0];
        instructionDictionaryEng = dicts[1];

        instructionsLength = instructionsDictionaryGer.Count;
        instructionCounter = 0;

        continueButton = continueButtonGameObject.GetComponent<Button>();
        returnButton = returnButtonGameObject.GetComponent<Button>();
        optionalButton = optionalButtonGameObject.GetComponent<Button>();
        continueButtonLabel = continueButtonGameObject.GetComponentInChildren<TMP_Text>();
        returnButtonLabel = returnButtonGameObject.GetComponentInChildren<TMP_Text>();
        optionalButtonLabel = optionalButtonGameObject.GetComponentInChildren<TMP_Text>();

        continueButtonGameObject.SetActive(false);
        returnButtonGameObject.SetActive(false);
        optionalButtonGameObject.SetActive(false);

        slider = sliderGameObject.GetComponent<Slider>();

        slider.onValueChanged.AddListener (delegate {OnSliderValueChanged(); });
        sliderGameObject.SetActive(false);
        
        SwitchCanvasModeToQuestionnaire(false);
        screenText.text = "";
    }

    public void StartCreationIntroduction()
    {
        continueButtonGameObject.SetActive(true);
        returnButtonGameObject.SetActive(true);

        if (GameObject.Find("GameManager").GetComponent<GameManager>().Language == GameManager.Languages.Deutsch)
        {
            instructionsDictionary = instructionsDictionaryGer;
            isLanguageGerman = true;
            continueButtonLabel.SetText("Weiter");
            returnButtonLabel.SetText("Zurück");
            optionalButtonLabel.SetText("Start");
        }
        else
        {
            instructionsDictionary = instructionDictionaryEng;
            continueButtonLabel.SetText("Continue");
            returnButtonLabel.SetText("Return");
            optionalButtonLabel.SetText("Start");
            isLanguageGerman = false;
        }
        
        SetInstruction();
    }

    void SetInstruction()
    {
        instructionsDictionary.TryGetValue(instructionCounter, out dictionaryValue);
        screenText.text = dictionaryValue;

        returnButton.interactable = instructionCounter != 0;
        //continueButtonGameObject.SetActive(_instructionCounter != _instructionsLength-1);
        continueButton.interactable = instructionCounter < instructionsLength - 1;
        
        optionalButtonGameObject.SetActive(!(instructionCounter < instructionsLength - 1));
        optionalButton.interactable = !(instructionCounter < instructionsLength - 1);

        //PlayerManager.LocalPlayerInstance.GetComponent<SynchronizeInformation>().Call_UpdateInformation(_dictionaryValue);
    }

    private void OnSliderValueChanged()
    {
        sliderAnswered = true;
        sliderValue = slider.value;
        continueButton.interactable = true;
    }

    // switch between full screen text only and questionnaire mode with enabled canvas elements like statement/instruction/note/likert scale
    // mode = true if questionnaire active
    private void SwitchCanvasModeToQuestionnaire(bool qrActive)
    {
        screenText.enabled = !qrActive;
        instruction.enabled = qrActive;
        statement.enabled = qrActive;
        note.enabled = qrActive;
    }

    private void AskForStressLevel()
    {
        sliderGameObject.SetActive(true);
        slider.interactable = true;

        screenText.enabled = true;
        
        screenText.text = isLanguageGerman ? "\nWie stressig fandest du die Aufgabe, den Stadtteil zu kreieren?\n \n \n" : "\nHow stressful did you find the exercise of creating the quarter?";
        //PlayerManager.LocalPlayerInstance.GetComponent<SynchronizeInformation>().Call_UpdateInformation(screenText.text);

        continueButton.interactable = false;
        returnButtonGameObject.SetActive(false);
    }
    
    private void StartQuestionnaire()
    {
        sliderGameObject.SetActive(false);

        CSVWriter.Instance.PhaseCounter = 3;
        CSVWriter.Instance.AddEntryToGeneral("Q0Start", "none");

        returnButtonGameObject.SetActive(true);
        returnButton.interactable = false;
        
        questionnaireActive = true;
        SwitchCanvasModeToQuestionnaire(true);
        
        // remove nasa tlx file according to language
        files[2] = isLanguageGerman ? nasaTlxGerman : nasaTlxEnglish;
        
        
        questionnaire = new Questionnaire
        {
            condition = GameManager.Instance.Condition,
            participantId = GameManager.Instance.ParticipantID,
            questions = Utilities.Parser.ParseQuestionsFromFiles(files),
            startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        questions = questionnaire.questions;
        questionCounter = 0;
        roomProperties.Add(RoomProperty.QuestionnaireIsOn, true);
        roomProperties.Add(RoomProperty.QuestionCounter, questionCounter);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        currentQuestion = questions[questionCounter]; // set first question
        if (currentQuestion.questionTypeValue == "7")
        {
            sevenPointLikertScaleGameObject.SetActive(true);
            currentLikertScale = _7PointLikertScale;
        }
        else
        {
            twentyOnePointLikertScaleGameObject.SetActive(true);
            currentLikertScale = _21PointLikertScale;
        }
        currentLikertScale.SetAllLabels(currentQuestion.scaleLabels);
        SetQuestion();
    }
    
    // Sets new question
    private void SetQuestion()
    {
        roomProperties = new Hashtable();

        if (questions[questionCounter].questionTypeValue == "7")
        {
            sevenPointLikertScaleGameObject.SetActive(true);
            twentyOnePointLikertScaleGameObject.SetActive(false);
            currentLikertScale = _7PointLikertScale;
        }
        else
        {
            sevenPointLikertScaleGameObject.SetActive(false);
            twentyOnePointLikertScaleGameObject.SetActive(true);
            currentLikertScale = _21PointLikertScale;
        }
        
        //if(_currentQuestion.scaleLabels != _questions[_questionCounter].scaleLabels)
        currentLikertScale.SetAllLabels(questions[questionCounter].scaleLabels);

        currentQuestion = questions[questionCounter];
        if(currentQuestion.showTime == 0)
            currentQuestion.showTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        instruction.text = currentQuestion.instruction;
        instruction.text = currentQuestion.instruction;
        statement.text = currentQuestion.question;
        note.text = currentQuestion.note;
        
        if (currentQuestion.IsAnswered)
        {
            currentLikertScale.SetAnswer(currentQuestion.answer);
        }
        else
        {
            currentLikertScale.Reset();
        }
        
        continueButton.interactable = currentQuestion.IsAnswered;
        returnButton.interactable = questionCounter != 0;
        roomProperties.Add(RoomProperty.QuestionCounter, questionCounter);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    public void OptionalButtonPressed()
    {
        if (!creationFinished) // after introduction
        {
            returnButtonGameObject.SetActive(false);
            continueButtonGameObject.SetActive(false);
            optionalButtonGameObject.SetActive(false);
            
            questMarker.GetComponent<QuestMarkerController>().AnchorPosition = new Vector3(-167.5f, 55f, 43f);

            screenText.text = isLanguageGerman ? "Hier geht es erst weiter wenn du auf jedem Feld der Fäche ein Stück platziert hast.\n \nDetails zur Steuerung und Interaktion findest links von dir an der mittleren Wand."
                : "To continue place a piece on every square of the board.\n \nYou can find details about your controls on the wall behind you next to the fridges.";

            CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().CurrentInteractionMode =
                OVRPlayer.InteractionMode.Physic;
            CSVWriter.Instance.PhaseCounter = 2;
            CSVWriter.Instance.AddEntryToGeneral("CreationStart", "none");
        }
    }

    public void ConfirmButtonPressed()
    {
        roomProperties = new Hashtable();
        if (!disableIntroductionCanvas)
        {
            CSVWriter.Instance.AddEntryToStressLevel("Baseline", introductionCanvas.gameObject.GetComponent<IntroductionCanvas>().GetSliderValue().ToString("#.00"));
            introductionCanvas.gameObject.SetActive(false);
            //introductionCanvas.enabled = _instructionCounter == 0;
            CSVWriter.Instance.PhaseCounter = 1;
            CSVWriter.Instance.AddEntryToGeneral("CreationIntroStart", "none");
            disableIntroductionCanvas = true;
            roomProperties.Add(RoomProperty.CurrentCanvas, "c");
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        
        //instructions not finished
        if (!questionnaireActive && !creationFinished)
        {
            instructionCounter++;
            roomProperties.Add(RoomProperty.CanvasInstructionCounter, instructionCounter);
            SetInstruction();
            //continueButtonGameObject.SetActive(_instructionCounter != _instructionsLength-1);
        } else if (!sliderAnswered)
        {
            AskForStressLevel();
        }
        else if (!questionnaireActive) // && _creationFinished
        {
            StartQuestionnaire();
        }
        else
        {
            questionCounter++;

            if (questionCounter == questions.Length)
            {
                ConfirmFinishingQuestionnaire();
            } 
            else if (questionCounter > questions.Length)
            {
                FinishQuestionnaire();
            }
            else
            {
                SetQuestion();
            }

        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    public void ReturnButtonPressed()
    {
        roomProperties = new Hashtable();
        if (!questionnaireActive)
        {
            instructionCounter--;
            roomProperties.Add(RoomProperty.CanvasInstructionCounter, instructionCounter);
            SetInstruction();
        }
        else
        {
            questionCounter--;
            roomProperties.Add(RoomProperty.QuestionCounter, questionCounter);
            SetQuestion();
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }
    
    public void IsTowNFinished(bool finished)
    {
        keyboard.TownFinished = finished;

        if (finished)
        {
            screenText.SetText(isLanguageGerman
                ? "Sehr gut, du hast auf jedem Feld ein Stück platziert! Bist du zufrieden? Wäre das eine Stadt die du gerne besuchen würdest?\n \nDann drücke auf die Tastatur, um den Stadtteil für das Navi zu speichern. Beachte: Danach kannst du ihn nicht mehr bearbeiten." 
                : "Very good!\n \nPress on the keyboard to save the city quarter. Take into consideration that you can not edit it afterwards.");
        }
        else
        {
            screenText.SetText(isLanguageGerman
                ? "Hier geht es erst weiter wenn du auf jedem Feld ein Stadtteil platziert hast.\n \nDetails zur Steuerung und Interaktion findest du links an der Wand auf den Bildern." 
                : "To continue place a piece on every square.\n \nYou can find details about your controls on the pictures on the wall to the left.");
            
        }
    }

    public void FinishCreation()
    {
        CSVWriter.Instance.AddEntryToGeneral("CreationFinish", "none");
        //PlayerManager.LocalPlayerInstance.GetComponent<DataPersistence>().Call_SaveScreenShots("Pictures\\");
        
        var props = new Hashtable
        {
            {RoomProperty.ScreenShot, true}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        
        
        creationFinished = true;
        screenText.text = isLanguageGerman
            ? "Sehr gut, das Stadtviertel ist gespeichert!\n \nDrücke auf \"Weiter\", um ein paar Fragen zur Kreation des Stadtteils zu beantworten. Diese müssen wir zum Großteil auf Englisch stellen."
            : "Very good, the quarter is saved!\n \nPress \"Continue\" to answer a few question about the creation of the quarter.";
        
        continueButtonGameObject.SetActive(true);
        continueButton.interactable = true;

        CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().CurrentInteractionMode =
            OVRPlayer.InteractionMode.Canvas;
        
        optionalButtonGameObject.SetActive(false);
    }
    
    public void SetAnswerToCurrentQuestion(bool value)
    {
        roomProperties = new Hashtable();
        currentQuestion.IsAnswered = value;
        if (currentQuestion.IsAnswered)
        {
            currentQuestion.answer = currentLikertScale.GetAnswer();
            int activeToggleIndex = currentLikertScale.GetAnswerAsInt();
            if (activeToggleIndex != -1)
            {
                roomProperties.Add(RoomProperty.LikertValue, currentLikertScale.GetAnswerAsInt());
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
            }
        }
        continueButton.interactable = value;
    }

    private void ConfirmFinishingQuestionnaire()
    {
        instruction.text = "";
        note.text = "";
        statement.text = isLanguageGerman 
            ? "Das waren alle Fragen.\nDu kannst sie dir noch einmal angucken oder auf \"Weiter\" klicken.\nWenn du auf \"Weiter\" klickst werden deine Antworten gespeichert und du kannst sie nicht mehr bearbeiten." 
            :"That's about it.\nYou can check your answers or press \"Continue\".\nIf you press \"Continue\" your answers are saved and you cannot edit them.";
        currentLikertScale.gameObject.SetActive(false);

        returnButton.interactable = true;
        continueButton.interactable = true;
    }

    private void FinishQuestionnaire()
    {
        questionnaire.endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        statement.text = "";
        instruction.text = "";
        screenText.enabled = true;
        screenText.text = isLanguageGerman
            ? "Vielen Dank für deine Antworten!\n \nDu kannst die Pizza nun ausliefern. \nGehe dazu durch die Küche in den Eingangsbereich zur Tür, und klicke auf das Schild über dem Skateboard."
            : "Thank you for your answers!\n \nYou can deliever the pizza now. \nGo through the kitchen to the door in the foyer and click on the sign above the skateboard.";
        
        questMarker.GetComponent<QuestMarkerController>().AnchorPosition = new Vector3(22f, 49f, -100f);
        introductionCanvas.gameObject.SetActive(false);
        //GameObject.Find("IntroductionCanvas").SetActive(false);
        
        CSVWriter.Instance.AddEntryToStressLevel("AfterCreation", sliderValue.ToString("#.00"));
        CSVWriter.Instance.AddEntryToGeneral("Q0Finished", "none");

        CSVWriter.Instance.PhaseCounter = 4;
        
        skateboard.EnableCanvas();
        
        sevenPointLikertScaleGameObject.SetActive(false);
        continueButtonGameObject.SetActive(false);
        returnButtonGameObject.SetActive(false);

        questionnaire.condition = GameManager.Instance.Condition;
        questionnaire.participantId = GameManager.Instance.ParticipantID;

        var filenamePrefix = GameManager.Instance.ParticipantID + "_" + GameManager.Instance.Condition + "_";

        CreationPlayerManager.LocalPlayerInstance.GetComponent<DataPersistence>().Call_SaveFile("Questionnaires\\Questionnaire_0\\", filenamePrefix + "q0_" + questionnaire.startTime + "_" + questionnaire.endTime + ".csv", questionnaire.ToString());
        
        // Add final entry to creation .csv
        CSVWriter.Instance.AddEntryToCreation(GameObject.Find("GridManager").GetComponent<GridManager>().GetFullGridLogString(), "ALL");
        CreationPlayerManager.LocalPlayerInstance.GetComponent<DataPersistence>().Call_SaveFile("Creation\\", filenamePrefix + "creation.csv", CSVWriter.Instance.TownCreationCsvString);
    }
}
