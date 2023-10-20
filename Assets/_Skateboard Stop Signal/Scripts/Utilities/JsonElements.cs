using UnityEngine;

public class JsonElements : MonoBehaviour
{
    [System.Serializable]
    public class QuestionnaireFile
    {
        public string title; // Questionnaire title
        public string instructions; // Questionnaire instructions
        public string code; // Questionnaire code, e.g. nasa_tlx_german
        public string type; // Type of the questions, e.g. likert
        public string typeValue; // 21 or 7 depending on likert-scale
        //public string[][] likertLabels; // Different Labels-Stringarrays
        public string[] likertLabels0;
        public string[] likertLabels1;
        public Question[] questions; // Questions of the questionnaire

    }

    [System.Serializable]
    public class Question
    {
        public string id;
        public string questiontype;
        public string label_id;
        public string instructions;
        public string note;
        public string statement;
        public string reverse;
    }
}
