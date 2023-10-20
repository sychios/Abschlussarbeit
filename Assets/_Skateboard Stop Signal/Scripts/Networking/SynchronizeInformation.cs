using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SynchronizeInformation : MonoBehaviourPunCallbacks
{
    // files for instructions
    [SerializeField] private TextAsset introductionInstructionsFile;
    [SerializeField] private TextAsset creationInstructionsFile;
    private Dictionary<int, string> introInstructionsDictionaryGer;
    private Dictionary<int, string> introInstructionsDictionaryEng;
    private Dictionary<int, string> creationInstructionsDictionaryGer;
    private Dictionary<int, string> creationInstructionsDictionaryEng;
    private Dictionary<int, string> currentDictionary;
    
    public TMP_Text text;
    public TMP_Text questionText;

    public AssistantLikertScale likertScale;

    private PhotonView view;

    private bool questionnaireIsActive = false;

    private int currentQuestionIndex;

    private AssistantQuestionManager assistantQuestionManager;

    //private int _instructionCounter; TODO: not necessary? set directly from room-properties update and when switching canvas-mode use 0
    private int questionCounter;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
            return;
        }

        var dicts = Utilities.Parser.ParseBilingualDictionariesFromFile(introductionInstructionsFile);
        introInstructionsDictionaryGer = dicts[0];
        introInstructionsDictionaryEng = dicts[1];
        
        dicts = Utilities.Parser.ParseBilingualDictionariesFromFile(creationInstructionsFile);
        creationInstructionsDictionaryGer = dicts[0];
        creationInstructionsDictionaryEng = dicts[1];

        currentDictionary = introInstructionsDictionaryGer;
        
        assistantQuestionManager = GetComponent<AssistantQuestionManager>();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue(RoomProperty.CurrentCanvas, out var canvas ))
        {
            if (canvas as string == "c")
            {
                currentDictionary = creationInstructionsDictionaryGer;
                text.SetText(currentDictionary[0]);
            } 
            else if (canvas as string == "i")
            {
                currentDictionary = introInstructionsDictionaryGer;
                text.SetText(currentDictionary[0]);
            }
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.CanvasInstructionCounter, out var counter))
        {
            var i = (int) counter;
            string val;
            if (currentDictionary.TryGetValue(i, out val))
            {
                text.SetText(val);
            }
            else
            {
                text.SetText($"Key not found: {i}");
            }
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.QuestionnaireIsOn, out var isOn))
        {
            ChangeDisplayMode((bool) isOn);
            if((bool) isOn)
                assistantQuestionManager.ParseFinishQuestions();
            questionnaireIsActive = (bool) isOn;
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.QuestionCounter, out var questionIndex))
        {
            if (questionnaireIsActive)
            {
                Question q = assistantQuestionManager.GetQuestionForIndex((int) questionIndex);
                currentQuestionIndex = (int) questionIndex;
                questionText.SetText(q.question);
                if (q.IsAnswered)
                {
                    likertScale.SetAnswer(Question.AnswerToIntMapping7PointLikert[q.answer]);
                }
                else
                {
                    likertScale.Reset();
                }
                    
            }
        }

        if (propertiesThatChanged.TryGetValue(RoomProperty.LikertValue, out var toggleIndex))
        {
            if (questionnaireIsActive)
            {
                likertScale.SetAnswer((int) toggleIndex);

                try
                {
                    var currQuestion = assistantQuestionManager.GetQuestionForIndex(currentQuestionIndex);
                    currQuestion.answer = currQuestion.questionTypeValue == "21"
                        ? Question.IntToAnswerMapping21PointLikert[(int) toggleIndex]
                        : Question.IntToAnswerMapping7PointLikert[(int) toggleIndex];
                    if (!currQuestion.IsAnswered)
                        currQuestion.IsAnswered = true;
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    throw;
                }
                
                //_assistantQuestionManager.GetQuestionForIndex(_currentQuestionIndex).answer =
                //    Question.IntToAnswerMapping7PointLikert[(int) toggleIndex]; //TODO: KeyNotFoundException, because 21 likert?
                //_assistantQuestionManager .GetQuestionForIndex(_currentQuestionIndex).IsAnswered = true;
            }
        }
        
        
    }

    private void ChangeDisplayMode(bool isQuestionnaireActive)
    {
        text.enabled = !isQuestionnaireActive;
        questionText.gameObject.SetActive(isQuestionnaireActive);
        likertScale.gameObject.SetActive(isQuestionnaireActive);
        questionnaireIsActive = isQuestionnaireActive;
    }
}
