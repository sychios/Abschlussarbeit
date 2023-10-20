using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class SstIntroduction : MonoBehaviour
    {
        [SerializeField] private GameObject introductionCanvas;
        
        [SerializeField] private Button optionalButton;
        private TMP_Text _optionalButtonText;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button continueButton;
        
        public SkaterController skaterController;

        public GameObject uiHelpers;
        
        private int segmentCounter;

        private PhotonView view;

        private TaskLogic taskLogic;

        private TaskUI taskUI;

        public GameObject warmth;
        public GameObject timer;


        public AudioSource audioSource;
        public GameObject volumeSlider;
        private Slider _volumeSlider;
        public GameObject sliderLabelLeft;
        public GameObject sliderLabelRight;
        
        // used to re-activate buttons after task trials
        private bool _tasksFinished;
        public bool TasksFinished
        {
            set => _tasksFinished = value;
        }
        
        private bool[] segmentWasVisited = new bool[9];

        private void Awake()
        {
            view = GetComponent<PhotonView>();
            taskLogic = GetComponent<TaskLogic>();
            taskUI = GetComponent<TaskUI>();

            for (int i = 0; i < segmentWasVisited.Length; i++)
            {
                segmentWasVisited[i] = false;
            }
        }

        private void Start()
        {
            if (!view.IsMine)
            {
                continueButton.gameObject.SetActive(false);
                returnButton.gameObject.SetActive(false);
                optionalButton.gameObject.SetActive(false);
                GetComponent<BoxCollider>().enabled = false;
                
                Destroy(introductionCanvas);
                Destroy(this);
                return;
            }

            _volumeSlider = volumeSlider.GetComponent<Slider>();
            _volumeSlider.onValueChanged.AddListener (delegate {OnSliderValueChanged(); });
            audioSource.volume = _volumeSlider.value;
            
            introductionCanvas.SetActive(true);

            warmth.SetActive(false);
            
            _optionalButtonText = optionalButton.GetComponentInChildren<TMP_Text>();

            segmentCounter = 0;
            segmentWasVisited[segmentCounter] = true;

            CSVWriter.Instance.PhaseCounter = 5;
            CSVWriter.Instance.AddEntryToGeneral("SSTIntroStarted", "none");
            
            SetSegment(segmentCounter);
        }
    
    
        private void SetSegment(int pSegmentCounter)
        {
            switch (pSegmentCounter)
            {
                case 0: // first segment with general text introduction, no interaction otherwise
                    taskUI.HideAll();
                    optionalButton.gameObject.SetActive(false);
                    returnButton.gameObject.SetActive(false);

                    continueButton.interactable = true;
                    
                    taskUI.SetInstruction(pSegmentCounter);
                    break;
                case 1: // only text information
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    returnButton.gameObject.SetActive(true);

                    continueButton.interactable = true;
                    returnButton.interactable = true;
                    
                    optionalButton.gameObject.SetActive(false);
                    break;
            
                case 2: // introduction to arrows, show example of left arrow
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    continueButton.interactable = segmentWasVisited[segmentCounter];
                    returnButton.interactable = segmentWasVisited[segmentCounter];
                    
                    optionalButton.gameObject.SetActive(true);
                    optionalButton.interactable = true;
                    _optionalButtonText.text = "Test";
                    break;
                
                case 3: // intro to how to react to arrows, short repeatable segment of left/right arrow showing and waiting for input. For doing that, only show the optional button
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    continueButton.interactable = segmentWasVisited[segmentCounter];
                    returnButton.interactable = segmentWasVisited[segmentCounter];
                    
                    _optionalButtonText.text = "Start";
                    break;
            
                case 4: // Sound segment
                    taskUI.SetInstruction(pSegmentCounter);

                    continueButton.interactable = segmentWasVisited[segmentCounter];
                    returnButton.interactable = segmentWasVisited[segmentCounter];
                    
                    _optionalButtonText.text = "Ton abspielen";
                    break;
                
                case 5: // Trials with stop signal
                    volumeSlider.SetActive(false);
                    sliderLabelLeft.SetActive(false);
                    sliderLabelRight.SetActive(false);
                    taskUI.SetInstruction(pSegmentCounter);

                    continueButton.interactable = segmentWasVisited[segmentCounter];
                    returnButton.interactable = segmentWasVisited[segmentCounter];
                    
                    optionalButton.gameObject.SetActive(true);
                    _optionalButtonText.text = "Start";
                    break;
                
                case 6: // Info about warmth slider and timer
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    timer.SetActive(true);
                    warmth.SetActive(true);
                    
                    optionalButton.gameObject.SetActive(false);
                    continueButton.gameObject.SetActive(true);

                    break;
                case 7: // info about enemy pizzerias
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    GameObject.Find("Border").GetComponent<Border>().ShowBorder();
                    
                    optionalButton.gameObject.SetActive(false);
                    continueButton.gameObject.SetActive(true);
                    
                    break;
                case 8:
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    optionalButton.gameObject.SetActive(false);
                    continueButton.gameObject.SetActive(true);

                    break;
                case 9:
                    taskUI.SetInstruction(pSegmentCounter);
                    
                    continueButton.gameObject.SetActive(false);
                    optionalButton.gameObject.SetActive(true);
                    _optionalButtonText.text = "Start";
                    
                    break;
                case 10:
                    taskUI.SetInstruction(pSegmentCounter);
                    break;
            }
        }
        

        public void OptionalButtonPressed()
        {
            switch (segmentCounter)
            {
                case 2:
                    taskUI.Call_ShowFixationAndArrow();

                    optionalButton.interactable = false;
                    continueButton.interactable = false;
                    returnButton.interactable = false;
                    taskUI.ExampleRunning = true;

                    StartCoroutine(WaitUntilExampleFinished(2));
                    break;
                case 3: // trial with 8 trials without stop signal
                    StartCoroutine(SstTrials(10, false));

                    optionalButton.interactable = false;
                    continueButton.interactable = false;
                    returnButton.interactable = false;
                    //_taskUI.ExampleRunning = true;

                    //StartCoroutine(WaitUntilExampleFinished(3));
                    break;
                case 4: // play sound of signal
                    volumeSlider.SetActive(true);
                    sliderLabelLeft.SetActive(true);
                    sliderLabelRight.SetActive(true);
                    taskUI.Call_PlaySignalWithDelay(0.5f, true);

                    optionalButton.interactable = false;
                    continueButton.interactable = false;
                    returnButton.interactable = false;
                    taskUI.ExampleRunning = true;

                    StartCoroutine(WaitUntilExampleFinished(4));
                    
                    break;
            
                case 5: // trials with signal
                    StartCoroutine(SstTrialsWithModifiedSignalProbability(10, 0.5f));
                    
                    optionalButton.interactable = false;
                    continueButton.interactable = false;
                    returnButton.interactable = false;
                    break;
                case 9: // take measures to start delivering the pizza                  
                    
                    taskUI.SetInstruction(segmentCounter); // set instruction text to "" (for assistant)
                    taskUI.DisableScreenText(); // hide instruction text and background
                    
                    HideButtons(); // hide buttons
                    
                    
                    uiHelpers.SetActive(false); // disable canvas interaction
                    
                    skaterController.FinishIntroduction(); // start exposure
                    taskUI.HideAll();
                    break;
            }
        }

        // wait until examples (e.g. showing arrows, task trials etc.) finished and enable buttons afterwards
        public IEnumerator WaitUntilExampleFinished(int segmentCounter)
        {
            yield return new WaitUntil(() => !taskUI.ExampleRunning);

            segmentWasVisited[segmentCounter] = true;
            switch (segmentCounter)
            {
                case 2: case 3: case 4: case 5:
                    optionalButton.interactable = true;
                    continueButton.interactable = true;
                    returnButton.interactable = true; 
                    break;
            }
        }

        public void ContinueButtonPressed()
        {
            segmentCounter++;
            SetSegment(segmentCounter);
        }

        public void ReturnButtonPressed()
        {
            segmentCounter--;
            SetSegment(segmentCounter);
        }

        public void SkipButtonPressed()
        {
            segmentCounter = 9;
            OptionalButtonPressed();
        }

        /*
         * Start tasks manually
         * */
        IEnumerator SstTrials(int amount, bool showSignal)
        {
            returnButton.interactable = false;
            continueButton.interactable = false;
            optionalButton.interactable = false;

            _tasksFinished = false;
            StartCoroutine(taskLogic.StartTasks(amount, showSignal));
            yield return new WaitUntil(() => _tasksFinished);

            segmentWasVisited[segmentCounter] = true;
            
            returnButton.interactable = true;
            continueButton.interactable = true;
            optionalButton.interactable = true;
        }

        private int _stopTasksExerciseCounter = 0;
        
        IEnumerator SstTrialsWithModifiedSignalProbability(int amount, float signalProbability)
        {
            returnButton.interactable = false;
            continueButton.interactable = false;
            optionalButton.interactable = false;

            StartCoroutine(taskLogic.StartTasksWithModifiedSignalProbability(amount, signalProbability));
            _tasksFinished = false;
            
            yield return new WaitUntil(() => _tasksFinished);
            _stopTasksExerciseCounter++;
            //TODO: check if we have to start again, set text accordingly and only enable optional button
            if (!segmentWasVisited[segmentCounter] && taskLogic.GetStopTaskPercentage() < 0.4f && _stopTasksExerciseCounter <= 2) // restart if segment was not approved yet and stop task  percentage lower than 0.4
            {
                Debug.Log($"Current stop task percentage: {taskLogic.GetStopTaskPercentage()}");
                optionalButton.interactable = true;
                taskUI.SetInstructionTextManually("Lass uns das noch einmal probieren!\nMerke: Wenn du den Ton hörst, nicht drücken!\n \nDrücke auf \"Start\" um es erneut zu probieren.");
            }
            else
            {
                taskUI.SetInstructionTextManually("Sehr gut!\nDu kannst jetzt weitermachen.");
                returnButton.interactable = true;
                continueButton.interactable = true;
                optionalButton.interactable = true;

                segmentWasVisited[segmentCounter] = true;
            }
        }

        public void HideButtons()
        {
            continueButton.gameObject.SetActive(false);
            returnButton.gameObject.SetActive(false);
            optionalButton.gameObject.SetActive(false);
        }

        private void OnSliderValueChanged()
        {
            audioSource.volume = _volumeSlider.value;
        }
    }
}
