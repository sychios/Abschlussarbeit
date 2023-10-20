using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Utilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class IntroductionCanvas : MonoBehaviour
{
    // file for instructions
    [SerializeField] private TextAsset instructionsFile;
    private Dictionary<int, string> instructionsDictionaryGer;
    private Dictionary<int, string> instructionsDictionaryEng;
    private Dictionary<int, string> currentInstructionsDictionary;

    [SerializeField] private TMP_Text instructionTextField;

    public GameObject GermanLanguageButtonGameObject;
    private Button germanLanguageButton;
    public GameObject EnglishLanguageButtonGameObject;
    private Button englishLanguageButton;
    
    private int instructionCounter;

    private int instructionsLength;
    private string dictionaryInstructionValue = "";

    [SerializeField] private TMP_Text lowSliderValue;
    [SerializeField] private TMP_Text highSliderValue;
    public enum Languages
    {
        Deutsch,
        English
    }

    private Languages _language;
    public Languages Language
    {
        get => _language;
        set
        {
            _language = value;
            
            Debug.Log($"PlayerManagerInstance: {CreationPlayerManager.LocalPlayerInstance.name}.");
            CreationPlayerManager.LocalPlayerInstance.GetComponent<EnvironmentLanguageManager>().SetLabelsToLanguageWithoutNewLines(_language == Languages.Deutsch);
            GameObject.Find("EnvironmentLanguageManager").GetComponent<EnvironmentLanguageManager>().SetLabelsToLanguage(value == Languages.Deutsch);
            if (_language == Languages.Deutsch)
            {
                currentInstructionsDictionary = instructionsDictionaryGer;
                instructionTextField.SetText("Derzeitige Sprache: Deutsch. Drücke \"Englisch\", um die Sprache zu Englisch zu ändern. Ansonsten drücke \"Weiter\" um fortzufahren.\n \nCurrent language: German. Press \"English\" to change the language to English. Otherwise press \"Continue\" to move on.\n \n \n");
                _continueButtonLabel.SetText("Weiter");
                _returnButtonLabel.SetText("Zurück");
                _optionalButtonLabel.SetText("Wechseln");
                lowSliderValue.SetText(_sliderValueLabelsDeutsch[0]);
                highSliderValue.SetText(_sliderValueLabelsDeutsch[1]);
                GameObject.Find("GameManager").GetComponent<GameManager>().Language =
                    GameManager.Languages.Deutsch;
            }
            else
            {
                currentInstructionsDictionary = instructionsDictionaryEng;
                instructionTextField.SetText("Derzeitige Sprache: Englisch. Drücke \"Deutsch\", um die Sprache zu Deutsch zu ändern. Ansonsten drücke \"Weiter\" um fortzufahren.\n \nCurrent language: English. Press \"German\" to change the language to German. Otherwise press \"Continue\" to move on.\n \n \n");
                _continueButtonLabel.SetText("Continue");
                _returnButtonLabel.SetText("Return");
                _optionalButtonLabel.SetText("Switch");
                lowSliderValue.SetText(_sliderValueLabelsEnglish[0]);
                highSliderValue.SetText(_sliderValueLabelsEnglish[1]);
                GameObject.Find("GameManager").GetComponent<GameManager>().Language =
                    GameManager.Languages.English;
            }

            _continueButton.interactable = true;
        }
    }
    
    private readonly string[] _sliderValueLabelsEnglish = 
    {
        "Not stressful at all.",
        "Extremely stressful."
    };
    
    private readonly string[] _sliderValueLabelsDeutsch = 
    {
        "Gar nicht stressig.",
        "Extrem stressig."
    };
    
    [SerializeField] private GameObject continueButtonGameObject;
    private Button _continueButton;
    private TMP_Text _continueButtonLabel;
    
    [SerializeField] private GameObject returnButtonGameObject;
    private Button _returnButton;
    private TMP_Text _returnButtonLabel;

    [SerializeField] private GameObject optionalButtonGameObject;
    private TMP_Text _optionalButtonLabel;
    private bool _controlsSwitched;
    private bool _controlsChosen;
    
    private bool _creationIntroductionStarted;

    [SerializeField] private GameObject sliderGameObject;
    private Slider _slider;
    private const float DefaultSliderValue = 0.5f;
    private bool _sliderMoved ;
    private float _sliderValue;

    public GameObject questMarker;

    private bool _introEntryMade;
    
    private Hashtable _properties;

    public Sprite leftTouchIndexSprite;
    public Sprite leftTouchThumbstickSprite;
    [SerializeField] private GameObject imageContainer;
    
    private void Start()
    {
        var dicts = Parser.ParseBilingualDictionariesFromFile(instructionsFile);
        instructionsDictionaryGer = dicts[0];
        instructionsDictionaryEng = dicts[1];
        
        
        _continueButton = continueButtonGameObject.GetComponent<Button>();
        _continueButtonLabel = continueButtonGameObject.GetComponentInChildren<TMP_Text>();
        _continueButton.interactable = false;
        _returnButton = returnButtonGameObject.GetComponent<Button>();
        _returnButtonLabel = returnButtonGameObject.GetComponentInChildren<TMP_Text>();
        _returnButton.interactable = false;

        _optionalButtonLabel = optionalButtonGameObject.GetComponentInChildren<TMP_Text>();
        optionalButtonGameObject.SetActive(false);

        currentInstructionsDictionary = instructionsDictionaryGer;
        instructionsLength = currentInstructionsDictionary.Count;

        _slider = sliderGameObject.GetComponent<Slider>();
        
        _slider.onValueChanged.AddListener (delegate {OnSliderValueChanged(); });
        
        sliderGameObject.SetActive(false);

        //Language = Languages.Deutsch;
        germanLanguageButton = GermanLanguageButtonGameObject.GetComponent<Button>();
        englishLanguageButton = EnglishLanguageButtonGameObject.GetComponent<Button>();
        
        questMarker.GetComponent<QuestMarkerController>().AnchorPosition = new Vector3(71f, 50f, 42.2f);
        
        instructionCounter = -1;
        _continueButton.interactable = false;
        SetInstruction();

        _properties = new Hashtable
        {
            {RoomProperty.CurrentCanvas, "i"}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(_properties);
    }

    public void SwitchLanguageToGerman()
    {
        Language = Languages.Deutsch;
    }

    public void SwitchLanguageToEnglish()
    {
        Language = Languages.English;
    }

    private void SetInstruction()
    {
        if (instructionCounter < 0) // language field
        {
            GermanLanguageButtonGameObject.SetActive(true);
            EnglishLanguageButtonGameObject.SetActive(true);
            
            if (Language == Languages.Deutsch)
            {
                instructionTextField.SetText("Bitte wähle eine Sprache aus.\n \nPlease choose a language.");
                //instructionTextField.SetText("Derzeitige Sprache: Deutsch. Drücke \"Englisch\", um die Sprache zu Englisch zu ändern. Ansonsten drücke \"Weiter\" um fortzufahren.\n \nCurrent language: German. Press \"English\" to change the language to English. Otherwise press \"Continue\" to move on.\n \n \n");
                _continueButtonLabel.SetText("Weiter");
                _returnButtonLabel.SetText("Zurück");
                germanLanguageButton.Select();
            }
            else
            {
                instructionTextField.SetText("Please choose a language.\n \nBitte wähle eine Sprache aus.");
                //instructionTextField.SetText("Derzeitige Sprache: Englisch. Drücke \"Deutsch\", um die Sprache zu Deutsch zu ändern. Ansonsten drücke \"Weiter\" um fortzufahren.\n \nCurrent language: English. Press \"German\" to change the language to German. Otherwise press \"Continue\" to move on.\n \n \n");
                _continueButtonLabel.SetText("Continue");
                _returnButtonLabel.SetText("Return");
                englishLanguageButton.Select();
            }

            _returnButton.interactable = false;
            sliderGameObject.SetActive(false);
            _continueButton.interactable = false;
            return;
        }
        
        GermanLanguageButtonGameObject.SetActive(instructionCounter < 0);
        EnglishLanguageButtonGameObject.SetActive(instructionCounter < 0);
        
        currentInstructionsDictionary.TryGetValue(instructionCounter, out dictionaryInstructionValue);
        instructionTextField.text = dictionaryInstructionValue;

        _returnButton.interactable = instructionCounter > 0;
        
        optionalButtonGameObject.SetActive(instructionCounter == 3);
        
        imageContainer.SetActive(instructionCounter == 1 || instructionCounter == 2);
        if (imageContainer.activeSelf)
            imageContainer.GetComponent<Image>().sprite =
                instructionCounter == 1 ? leftTouchThumbstickSprite : leftTouchIndexSprite;
        
        
        if (instructionCounter == instructionsLength - 1 && !_creationIntroductionStarted)
        {
            GameObject.Find("CreationCanvas").GetComponent<CreationCanvas>().StartCreationIntroduction();
            _creationIntroductionStarted = true;
            questMarker.GetComponent<QuestMarkerController>().AnchorPosition = new Vector3(-88f, 56f, 137f);
        }
        _continueButton.interactable = instructionCounter != instructionsLength - 1;

        if (instructionCounter == 3)
            _continueButton.interactable = _controlsChosen;

        if (instructionCounter == 4)
        {
            ShowSlider();
        }
        else
        {
            sliderGameObject.SetActive(false);
        }

        if (instructionCounter >= 0)
        {
            _properties = new Hashtable
            {
                {RoomProperty.CanvasInstructionCounter, instructionCounter}
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(_properties);
        }
    }

    private void ShowSlider()
    {
        sliderGameObject.SetActive(true);
        _continueButton.interactable = _sliderMoved;

        if (_sliderMoved)
        {
            _slider.SetValueWithoutNotify(_sliderValue);
        }
        else
        {
            _slider.SetValueWithoutNotify(DefaultSliderValue);
        }
    }

    private void OnSliderValueChanged()
    {
        _sliderValue = _slider.value;
        _sliderMoved = true;
        _continueButton.interactable = true;
    }

    public float GetSliderValue()
    {
        return _sliderValue;
    }

    public void ContinueButtonClicked()
    {
        if (!_introEntryMade)
        {
            CSVWriter.Instance.AddEntryToGeneral("IntroStart", "none");
            _introEntryMade = true;
        }
        instructionCounter++;
        SetInstruction();
    }

    public void ReturnButtonClicked()
    {
        instructionCounter--;
        SetInstruction();
    }

    public void OptionalButtonClicked()
    {
        _controlsSwitched = !_controlsSwitched;
        CreationPlayerManager.LocalPlayerInstance.GetComponent<OVRPlayer>().SetPointers(_controlsSwitched);
        GameManager.Instance.ControllersSwitched = _controlsSwitched;
        _controlsChosen = true;
        _continueButton.interactable = true;
    }
}
