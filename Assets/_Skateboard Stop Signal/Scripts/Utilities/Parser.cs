using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    public class Parser : MonoBehaviour
    {
        /// <summary>
        /// Parses json-file including two string-arrays for keys "Deutsch" and "English" into two Dictionaries
        /// </summary>
        /// <param name="file"></param>
        /// <returns>A two-sized array with first the german and secondly the english dictionary.</returns>
        public static Dictionary<int, string>[] ParseBilingualDictionariesFromFile(TextAsset file)
        {
            var dictGerman = new Dictionary<int, string>();
            var dictEnglish = new Dictionary<int, string>();
            var dictArray = new Dictionary<int, string>[2];

            if (file == null) return dictArray;

            var jsonFile = JsonUtility.FromJson<BilingualStringArraysJsonBlueprint>(file.text);
            
            if (jsonFile == null) return dictArray;
            if (jsonFile.Deutsch.Length != jsonFile.English.Length) return dictArray;

            for (var index = 0; index <= jsonFile.Deutsch.Length - 1; index++)
            {
                dictGerman.Add(index, jsonFile.Deutsch[index]);
                dictEnglish.Add(index, jsonFile.English[index]);
            }

            dictArray[0] = dictGerman;
            dictArray[1] = dictEnglish;

            return dictArray;
        }

        /// <summary>
        /// Parses settings regarding the exposure phase (task-amount, duration, signal ladder, ..) from file.
        /// </summary>
        /// <param name="file">File with settings</param>
        /// <returns>Settings as strings.</returns>
        public static Dictionary<string, string> ParseSettingsFromFile(TextAsset file)
        {
            var dict = new Dictionary<string, string>();

            if (file == null) return dict;

            var jsonFile = JsonUtility.FromJson<SettingsDictionaryBlueprint>(file.text);

            dict.Add(SettingsDictionaryKeys.TaskAmountPerRound, jsonFile.taskAmountPerRound.ToString());
            dict.Add(SettingsDictionaryKeys.ArrowDuration, jsonFile.arrowDuration.ToString("N2")); // limit float to two decimal places
            dict.Add(SettingsDictionaryKeys.FixationDuration, jsonFile.fixationDuration.ToString("N2"));
            dict.Add(SettingsDictionaryKeys.ResultDuration, jsonFile.resultDuration.ToString("N2"));
            dict.Add(SettingsDictionaryKeys.SignalDelayLadder, jsonFile.signalDelayLadder);

            return dict;
        }

        public static string[] ParseResultMessagesFromFile(TextAsset file, bool inGerman)
        {
            var jsonFile = JsonUtility.FromJson<BilingualStringArraysJsonBlueprint>(file.text);

            return inGerman ? jsonFile.Deutsch : jsonFile.English;
        }
        
        /// <summary>
        /// Parses questions from multiple files. The questions are not randomized in their order, but get returned in the order of the file-array
        /// </summary>
        /// <param name="fileNames">Enumerable with question files</param>
        /// <returns>Array of questions ordered in the </returns>
        public static Question[] ParseQuestionsFromFiles(IEnumerable<TextAsset> fileNames)
        {
            var questions = new List<Question>();
            foreach (var file in fileNames)
            {
                var jsonFile = JsonUtility.FromJson<JsonElements.QuestionnaireFile>(file.text);
                questions.AddRange(ParseQuestionsFromJsonFile(jsonFile));
            }
        
            return questions.ToArray();
        }

        private static IEnumerable<Question> ParseQuestionsFromJsonFile(JsonElements.QuestionnaireFile file)
        {
            var questions = new List<Question>();

            foreach (var question in file.questions)
            {
                var q = GetQuestionFromJsonFile(question);
                q.scale = file.code;
                q.questionTypeValue = file.typeValue;
                q.scaleLabels = question.label_id == "0" ? file.likertLabels0 : file.likertLabels1;
                questions.Add(q);
            }
            return questions;
        }

        private static Question GetQuestionFromJsonFile(JsonElements.Question fileQuestion)
        {
            var q = new Question
            {
                id = fileQuestion.id,
                questionType = fileQuestion.questiontype,
                instruction = fileQuestion.instructions,
                question = fileQuestion.statement,
                note = fileQuestion.note,
                IsAnswered = false,
                answerTime = 0,
                showTime = 0,
                reversed = fileQuestion.reverse,
                AnswerCounter = 0
            };
            return q;
        }
        
    }

    // Starting from here the classes for importing from json-files are described. When parsing a json-file, a class with corresponding keys(attribute name) and values (attribute type) must exist 
    
    [System.Serializable]
    public class BilingualStringArraysJsonBlueprint
    {
        public string[] Deutsch;
        public string[] English;
    }

    [System.Serializable]
    public class SettingsDictionaryBlueprint
    {
        public int taskAmountPerRound;
        public float arrowDuration;
        public float fixationDuration;
        public float resultDuration;
        public string signalDelayLadder;
        public string[] taskErrorMessages;
    }

    public class SettingsDictionaryKeys
    {
        public const string TaskAmountPerRound = "taskAmountPerRound";
        public const string ArrowDuration = "arrowDuration";
        public const string FixationDuration = "fixationDuration";
        public const string ResultDuration = "resultDuration";
        public const string SignalDelayLadder = "signalDelayLadder"; // "0.1#0.15#0.2#0.25#0.3#0.35#0.4#0.4
        public const string TaskErrorMessages = "taskErrorMessages";
    }
}
