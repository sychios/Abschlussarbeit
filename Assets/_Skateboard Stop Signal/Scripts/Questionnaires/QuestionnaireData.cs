using System;
using System.Collections.Generic;

public class Questionnaire
{
    public string condition; // A,B
    public string participantId; // unique
    //public string codes; // sub-questionnaires, imi, nasa tlx, pens
    public Question[] questions; // all questions
    public long startTime;
    public long endTime;

    public override string ToString()
    {
        string csvString = "";
        
        // condition, participantId, Scale, id, questionType, question, answer, answer counter, reversed, showtime, answer time, nr
        string header =
            "ParticipantID;Condition;Scale;ID;QuestionType;Question;Answer;AnswerCounter;Reversed;Showtime;Answertime;Nr\n";

        csvString += header;
        string startString = participantId + ";" + condition + ";";

        /*string firstEntry = participantId + ";" +
                            condition + ";" +
                            "qrInfo" + ";" +
                            "none" + ";" +
                            "infoEntry" + ";" +
                            "none" + ";" +
                            "none" + ";" +
                            "none" + ";" +
                            "none" + ";" +
                            startTime + ";" +
                            endTime + "\n";

        csvString += firstEntry;*/

        int counter = 0;
        
        foreach (var question in questions)
        {
            var mergedString = startString + question.ToCsvEntry() + counter + "\n";
            csvString += mergedString;
            counter++;
        }
        
        
        return csvString;
    }
}

public class Question
{
    public static readonly Dictionary<string, int> AnswerToIntMapping7PointLikert = new Dictionary<string, int>
    {
        {"Min",1},
        {"Min+1",2},
        {"Min+2",3},
        {"Neutral",4},
        {"Max-2",5},
        {"Max-1",6},
        {"Max",7}
    };
    
    public static readonly Dictionary<int, string> IntToAnswerMapping7PointLikert = new Dictionary<int, string>
    {
        {1, "Min"},
        {2, "Min+1"},
        {3, "Min+2"},
        {4, "Neutral"},
        {5, "Max-2"},
        {6, "Max-1"},
        {7, "Max"}
    };
    
    public static readonly Dictionary<string, int> AnswerToIntMapping21PointLikert = new Dictionary<string, int>
    {
        {"Min",1},
        {"Min+1",2},
        {"Min+2",3},
        {"Min+3",4},
        {"Min+4",5},
        {"Min+5",6},
        {"Min+6",7},
        {"Min+7",8},
        {"Min+8",9},
        {"Min+9",10},
        {"Neutral",11},
        {"Max-9",12},
        {"Max-8",13},
        {"Max-7",14},
        {"Max-6",15},
        {"Max-5",16},
        {"Max-4",17},
        {"Max-3",18},
        {"Max-2",19},
        {"Max-1",20},
        {"Max",21}
    };
    
    public static readonly Dictionary<int, string> IntToAnswerMapping21PointLikert = new Dictionary<int, string>
    {
        {1,"Min"},
        {2,"Min+1"},
        {3,"Min+2"},
        {4,"Min+3"},
        {5,"Min+4"},
        {6,"Min+5"},
        {7,"Min+6"},
        {8,"Min+7"},
        {9,"Min+8"},
        {10,"Min+9"},
        {11,"Neutral"},
        {12,"Max-9"},
        {13,"Max-8"},
        {14,"Max-7"},
        {15,"Max-6"},
        {16,"Max-5"},
        {17,"Max-4"},
        {18,"Max-3"},
        {19,"Max-2"},
        {20,"Max-1"},
        {21,"Max"}
    };
    
    
    public string ToCsvEntry()
    {
        string entry = "";

        int answerToInt = -1;
        
        if (questionTypeValue == "7")
        {
            AnswerToIntMapping7PointLikert.TryGetValue(answer, out answerToInt);
        }
        else
        {
            AnswerToIntMapping21PointLikert.TryGetValue(answer, out answerToInt);
        }

        entry += scale + ";" +
                 id + ";" +
                 questionType + ";" +
                 question + ";" +
                 answerToInt + ";" +
                 AnswerCounter + ";" +
                 reversed + ";" +
                 showTime + ";" +
                 answerTime + ";";
        return entry;
    }
    public string id; // unique id, int
    public string scale; // nasaTLX, IMI, PENS, Demo
    public string questionType; // likert
    public string questionTypeValue; // for likert: 7 or 21 item
    public string[] scaleLabels; //TODO: check length in questions file for length of label entry, if > 1 use "labelId" of question to determine the labels
    public string instruction; // instruction on how to answer the question
    public string question; // question to be answered
    public string note; // additional, explanatory note
    
    public long showTime; // time the question was first displayed
    public long answerTime; // time the question was first answered

    public string reversed; // reverse question?

    public string answer;
    
    private bool _isAnswered;
    private int _answerCounter;

    public bool IsAnswered
    {
        get => _isAnswered;
        set
        {
            if (value)
            {
                answerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _answerCounter++;
            }

            _isAnswered = value;
        }
    }

    public int AnswerCounter
    {
        get => _answerCounter;
        set => _answerCounter = value;
    }

    public override string ToString()
    {
        return "Question with id: " + id
            + "was first shown at \n" + showTime
            + "\nhas answer time \n" + answerTime
            + "\n,was answered for " + AnswerCounter + " times and currently has answer: " + answer;
    }
}
