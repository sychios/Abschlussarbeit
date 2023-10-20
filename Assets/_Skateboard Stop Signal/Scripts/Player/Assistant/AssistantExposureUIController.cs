using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Utilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class AssistantExposureUIController : MonoBehaviourPunCallbacks
{    private PhotonView view;
    
    // Files
    [SerializeField] private TextAsset taskIntroInstructionsFile;
    private Dictionary<int, string> taskIntroInstructions;

    [SerializeField] private TextAsset taskSettingsFile;
    private Dictionary<string, string> taskSettings;

    [SerializeField] private TextAsset taskResultMessagesFile;
    
    // Valid arrow room property values
    // 'l' -> left, 'r' -> right
    // 't' -> true, 'f' -> false
    private readonly string[] arrowPropertyValues = {"lt", "lf", "rt", "rf"};
        
    // Reference to task visualization
    [SerializeField] private AssistantExposureUI assistantExposureUI;
    
    // Settings for duration of task visualizations (arrow/result displaying, ..)
    private string arrowDisplayDuration;
    private string resultDisplayDuration;
    private string[] taskResultMessages;
    private float[] signalDelayLadder;
    private int signalDelayLadderIndex;
    
    // Questionnaire helper
    private AssistantQuestionManager assistantQuestionManager;
    private int currentQuestionIndex;

    
    // Result instances and score
    public TMP_Text resultListTMP;
    private List<Result> currentResults = new List<Result>();
    private string currentResultsString;
    public TMP_Text trialScores;
    
    private class Result
    {
        public Result(bool isGo, bool success, string resultMessage)
        {
            Success = success;
            IsGo = isGo;
            ResultMessage = resultMessage;
        }

        private bool Success { get;}
        public bool IsGo { get; }
        private string ResultMessage { get; }

        public bool IsGood()
        {
            return Success;
        }
        
        public override string ToString()
        {
            string s = "";
            if (Success)
            {
                s += "GOOD | ";
            }
            else
            {
                s += "BAD | ";
            }

            if (IsGo)
            {
                s += "YES | ";
            }
            else
            {
                s += "NO | ";
            }

            s += ResultMessage;

            return s;
        }
    }
    
    private List<Result> results = new List<Result>();
    
    
    // Feedback variables
    private List<bool> goTrials = new List<bool>(); // list of all go trials, where value is True if trials was good and False when trial was bad
    private List<bool> signalTrials = new List<bool>(); // same as with go-trials list
    
    
    private void Awake()
    {
        view = GetComponent<PhotonView>();

        if (!view.IsMine)
        {
            Destroy(this);
            return;
        }

        assistantQuestionManager = GetComponent<AssistantQuestionManager>();
    }

    private void UpdateResults(Result r)
    {
        results.Add(r);

        if (r.IsGo)
        {
            goTrials.Add(r.IsGood());
        }
        else
        {
            signalTrials.Add(r.IsGood());
        }

        UpdateTrialScore();
        
        var str = r.ToString();
        if (r.IsGood())
        {
            str = "<color=green>" + str + "</color>";
        }
        else
        {
            str = "<color=red>" + str + "</color>";
        }

        currentResultsString += "\n" + str;
        
        resultListTMP.SetText(currentResultsString);
        currentResults.Add(r);
    }

    private void ClearResults()
    {
        currentResultsString = "";
        currentResults.Clear();
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        ParseSettings();
        ParseFiles();
    }

    private void ParseFiles()
    {
        // Load dictionaries from files
        var dicts= Parser.ParseBilingualDictionariesFromFile(taskIntroInstructionsFile);
        if (dicts?[0] == null || dicts[1] == null || dicts[0].Count == 0 || dicts[1].Count == 0)
        {
            Debug.LogError("Parser: Parsing dictionaries from file failed or empty (taskIntroInstructionsFile)");
            return;
        }
        taskIntroInstructions = dicts[0]; // only german necessary
    }

    private void ParseSettings()
    {
        var dict = Parser.ParseSettingsFromFile(taskSettingsFile);
        if (dict == null || dict.Count == 0)
        {
            Debug.LogError("Parser: Parsing dictionary from file failed or empty (taskSettingsFile)");
            return;
        }

        if (dict.TryGetValue(SettingsDictionaryKeys.ArrowDuration, out arrowDisplayDuration))
        {
            assistantExposureUI.ArrowDisplayTime = float.Parse(arrowDisplayDuration);
        }
        else
        {
            Debug.LogError("Parser: Parsing value from dictionary failed. (ArrowDuration");
            
        }

        if (dict.TryGetValue(SettingsDictionaryKeys.ResultDuration, out resultDisplayDuration))
        {
            assistantExposureUI.ResultDisplayTime = float.Parse(resultDisplayDuration);
        }
        else
        {
            Debug.LogError("Parser: Parsing value from dictionary failed. (ArrowDuration");
        }

        if (dict.ContainsKey(SettingsDictionaryKeys.SignalDelayLadder))
        {
            signalDelayLadder =  Array.ConvertAll(dict[SettingsDictionaryKeys.SignalDelayLadder].Split('#'), float.Parse);
            signalDelayLadderIndex = 0;
        }
        else
        {
            Debug.LogError("Parser: Parsing value from dictionary failed. (SignalLadder");
        }
        
        if (taskResultMessagesFile != null)
        {
            taskResultMessages = Parser.ParseResultMessagesFromFile(taskResultMessagesFile, true);
        }
        else
        {
            Debug.LogError("Parser: Parsing value from dictionary failed. (TaskErrorMessages");
        }

    }

    public void FeedbackButtonPressed()
    {
        var properties = new Hashtable
        {
            {RoomProperty.FeedbackGiven, true}
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

    }
    

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        object valueAsObject;
        
        if(propertiesThatChanged.TryGetValue(RoomProperty.CurrentCanvas, out valueAsObject))
        {
            var cnvs = new []{"b", "c" };
            var canvas = valueAsObject as string;
            if (!cnvs.Contains(canvas))
            {
                Debug.LogError("Invalid canvas at this point: " + canvas + " .");
            }
            else
            {
                if (canvas == "b")
                {
                    assistantExposureUI.SetCanvasMode(AssistantExposureUI.CanvasMode.Basic);
                }
                else
                {
                    assistantExposureUI.SetCanvasMode(AssistantExposureUI.CanvasMode.Questionnaire);
                    //_assistantQuestionManager.ParseQuestions();
                    
                }
            }
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.QuestionnaireIsOn, out valueAsObject))
        {
            if ((bool) valueAsObject)
            {
                assistantExposureUI.SetCanvasMode(AssistantExposureUI.CanvasMode.Questionnaire);
            } 
            else if (!(bool) valueAsObject)
            {
                assistantExposureUI.SetCanvasMode(AssistantExposureUI.CanvasMode.None);
                
            }
        }
        
        if (propertiesThatChanged.TryGetValue(RoomProperty.Questionnaire, out valueAsObject))
        {
            if (valueAsObject as string == "b")
            {
                assistantQuestionManager.ParseBreakQuestions();
            } 
            else if (valueAsObject as string == "f")
            {
                assistantQuestionManager.ParseFinishQuestions();
            }
        }
        
        
        if(propertiesThatChanged.TryGetValue(RoomProperty.NewTaskArray, out valueAsObject))
        {
            var b = (bool) valueAsObject;
            if(b)
                ClearResults();
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.CanvasInstructionCounter, out valueAsObject))
        {
            var index = (int) valueAsObject;
            if (index < 0 || index > taskIntroInstructions.Count - 1)
            {
                Debug.LogError("Out of range index " + index + " .");
            }
            else
            {
                assistantExposureUI.SetTextBasicMode(taskIntroInstructions[index]);
            }
        }
        
        
        if(propertiesThatChanged.TryGetValue(RoomProperty.SSD, out valueAsObject))
        {
            var index = (int) valueAsObject;
            if (index < 0 || index >= signalDelayLadder.Length)
            {
                Debug.LogError("Could not set delay index from room property value" + index + " as it does not match the index range.");
            }
            else
            {
                signalDelayLadderIndex = index;
            }
        }
        
        
        if (propertiesThatChanged.TryGetValue(RoomProperty.Arrow, out valueAsObject))
        {
            var arrow = (string) valueAsObject;
            if (arrow.Length != 2 || !arrowPropertyValues.Contains(arrow))
            {
                Debug.LogError("Could not set arrow room-property from value "+ arrow +" as it does not match the arrow pattern {l,r}{t,f}");
            }
            else
            {
                assistantExposureUI.ShowArrowForTime(arrow[0], arrow[1] == 't', signalDelayLadder[signalDelayLadderIndex]);
            }
            
        }
        
        if (propertiesThatChanged.TryGetValue(RoomProperty.Result, out valueAsObject))
        {
            var result = (string) valueAsObject;
            string[] validResults = {"yt", "yf", "nt", "nf"};
            if (result.Length > 3 || result.Length == 0 || !validResults.Contains(result.Remove(result.Length-1)))
            {
                Debug.LogError("Could not set result from room-property value"+ result + " as it does not match the result patter {y,n}{t,f}[0, TaskErrorMessages.Length-1]");
            }
            else
            {
                result = (string) valueAsObject;

                var code = int.Parse(result[2].ToString());
                if (code >= 0 && code < taskResultMessages.Length)
                {
                    UpdateResults(new Result(result[0] == 'y' ,result[1] == 't', taskResultMessages[code]));
                    assistantExposureUI.ShowResult(result[1] == 't', taskResultMessages[code]);
                }
                else
                {
                    Debug.LogWarning($"ResultRoomProperty: Index {result[2]} was tried on {taskResultMessages.Length}-Length array, but failed.");
                }
            }
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.FeedbackGiven, out valueAsObject))
        {
            var given = (bool) valueAsObject;
            assistantExposureUI.SetFeedbackButton(!given);
        }
        
        if (propertiesThatChanged.TryGetValue(RoomProperty.QuestionCounter, out valueAsObject))
        {
            var index = (int) valueAsObject;
            if (assistantExposureUI.CurrentCanvasMode == AssistantExposureUI.CanvasMode.Questionnaire)
            {
                Question q = assistantQuestionManager.GetQuestionForIndex((int) index);
                currentQuestionIndex = (int) index;
                assistantExposureUI.SetTextQuestionnaireMode(q.question);
                if (q.IsAnswered)
                {
                    assistantExposureUI.SetLikertValueQuestionnaireMode(Question.AnswerToIntMapping7PointLikert[q.answer]);
                }
                else
                {
                    assistantExposureUI.ResetLikertScale();
                }
            }
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.LikertValue, out valueAsObject))
        {
            var toggleIndex = (int) valueAsObject;
            if (assistantExposureUI.CurrentCanvasMode == AssistantExposureUI.CanvasMode.Questionnaire)
            {
                assistantExposureUI.SetLikertValueQuestionnaireMode(toggleIndex);

                if (toggleIndex > 7)
                {
                    assistantQuestionManager.GetQuestionForIndex(currentQuestionIndex).answer = Question.IntToAnswerMapping7PointLikert[toggleIndex];
                }
                else
                {
                    assistantQuestionManager.GetQuestionForIndex(currentQuestionIndex).answer = Question.IntToAnswerMapping7PointLikert[toggleIndex];
                }
                assistantQuestionManager.GetQuestionForIndex(currentQuestionIndex).IsAnswered = true;
            }
        }
    }

    private void UpdateTrialScore()
    {
        trialScores.SetText($"Go-Score:\n{GetGoTaskPercentage()*100f}\n\nSignal-Score:{GetStopTaskPercentage()*100f}");
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
