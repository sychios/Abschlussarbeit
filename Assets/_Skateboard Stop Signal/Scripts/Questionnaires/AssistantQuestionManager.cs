using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AssistantQuestionManager : MonoBehaviour
{
    [Tooltip("Make sure these are the same files in the same order as on participant prefab!")]
    public TextAsset[] breakQuestionnaireFiles;

    private Question[] questions;

    private PhotonView view;

    [Tooltip("Make sure these are the same files in the same order as on participant prefab!")]
    public TextAsset[] finishQuestionnaireFiles;
    
    
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(this);
            return;
        }
    }
    
    // Use with care, as Question are parsed with limited information (instruction, question, note, isAnswered)!
    public Question GetQuestionForIndex(int index)
    {
        if (index >= 0 && index < questions.Length)
        {
            return questions[index];
        }

        Debug.LogWarning($"Question for index {index} not found in Questions of length {questions.Length}");
        return new Question();
    }

    public void ParseBreakQuestions()
    {
        questions = ParseQuestionsFromFiles(breakQuestionnaireFiles);
    }

    public void ParseFinishQuestions()
    {
        questions = ParseQuestionsFromFiles(finishQuestionnaireFiles);
    }
    
    private Question[] ParseQuestionsFromFiles(TextAsset[] fileNames)
    {
        List<Question> questions = new List<Question>();
        foreach (var file in fileNames)
        {
            var jsonFile = JsonUtility.FromJson<JsonElements.QuestionnaireFile>(file.text);
            questions.AddRange(ParseQuestionsFromJsonFile(jsonFile));
        }
        
        return questions.ToArray();
    }

    private IEnumerable<Question> ParseQuestionsFromJsonFile(JsonElements.QuestionnaireFile file)
    {
        var questions = new List<Question>();

        foreach (var question in file.questions)
        {
            var q = GetQuestionFromJsonFile(question);
            q.scale = file.code;
            q.questionTypeValue = file.typeValue;
            q.scaleLabels = question.label_id == "0" ? file.likertLabels0 : file.likertLabels1;
            //q.scaleLabels = file.segments;
            questions.Add(q);
        }
        return questions;
    }

    private Question GetQuestionFromJsonFile(JsonElements.Question fileQuestion)
    {
        var q = new Question
        {
            id = fileQuestion.id,
            instruction = fileQuestion.instructions,
            question = fileQuestion.statement,
            note = fileQuestion.note,
            IsAnswered = false,
        };
        return q;
    }
}
