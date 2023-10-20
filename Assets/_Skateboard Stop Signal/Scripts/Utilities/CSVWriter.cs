using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CSVWriter : MonoBehaviour
{
    // unique participant ID,
    private string participantID;
    
    // Study conditions, can be one of [A, B],
    // A := assistant not present in exposure phase
    // B := assistant present in exposure phase
    private string condition;
    
    // Current phase. Possible phases are defined in "_phases"
    private int _phaseCounter = 0;
    public int PhaseCounter
    {
        set
        {
            if (value >= 0 && value <= phases.Length - 1)
            {
                _phaseCounter = value;
            }
        }

    }

    private string[] phases =
    {
        "intro", // introduction, player is introduced to the VR-world and entered the magic circle, lasts until player starts the creation introduction
        "creationIntro", // player reads introductions regarding the creation of the city
        "creation", // actual creation of the city, starts when player definitely ends creation introduction
        "q1", // first questionnaire, phase starts with the presentation of the first question
        "firstSceneTransition", // phase describes the time between finishing the first questionnaire and the arrival in the exposure scene which starts by clicking on button at the door
        "sstIntro", // introduction to and explanation of the Stop-Signal-Task
        "sst", // the actual Stop-Signal-Task
        "secondsSceneTransition", // phase describes the time between spawning in the final scene and starting the seconds questionnaire
        "q2", // seconds questionnaire, starts with presentation of the first question
    };

    // String for general CSV
    // this csv logs movement, phase-specific start and endtimes, canvas- or other interaction with the VR-environment
    // header: participantId, condition, phase, event, eventData, time
    /**
     * 1, A, intro , Spawn, none, time
     *          .. , Movement, Vector3#Vector3, time
     *          .. , Q0Start, none, time
     *          .. , Q0Finish, none, time
     *          .. , CreationStart, none, time
     *          .. , CreationFinish, none, time
     *          .. , Q1Start, none, time
     *          .. , Q1Finish, none, time
     *          .. , SSTIntroductionStart, none, time
     *          .. , SSTIntroductionFinish, none, time
     *          .. , Finish, none, time
     *          .. , Q2Start, none, time
     *          .. , Q2Finish, none, time
     *
     *  --> Event ChunkAction only between CreationStart and CreationFinish
     *  --> eventData in PlayerMovement can be read as "initial position"#"destination position"
     */
    private string _generalCsvString = "ParticipantID;Condition;Phase;Event;EventData;Time\n";
    public string GeneralCsvString => _generalCsvString;

    // String for the town creation CSV
    // logs interaction with the creation tool
    // header: participantId, condition, action, chunkId, time
    // action can be one of [ADD, DEL, ROT] while chunkId refers to the field on which the action takes place
    private string _townCreationCsvString = "ParticipantID;Condition;Action;ChunkID;Time\n";
    public string TownCreationCsvString => _townCreationCsvString;
    
    // Strings for questionnaire CSV's
    // these csv's log the questionnaires
    // questionnaire columns: participantID, condition, questiontype, question, answer, reverse; time
    private string _demographicsCsvString = "ParticipantID;Condition;Questiontype;Question;Answer;Reverse;Time\n";
    private string _creationCsvString = "ParticipantID;Condition;Questiontype;Question;Answer;Time\n";
    private string _exposureCsvString = "ParticipantID;Condition;Questiontype;Question;Answer;Time\n";

    public string DemographicsCsvString => _demographicsCsvString;
    public string CreationCsvString => _creationCsvString;
    public string ExposureCsvString => _exposureCsvString;

    // String for SST phase
    // ID, Condition, trial?, Beginn des SST, GlobalTaskCount, localTaskCount, Erwartetet Antwort, Signal on?, SSD in ms,    [Reaktionen(Art der Reaktion, Zeitpunkt der Reaktion (vor Ablauf, nach Ablauf))], Task gut?
    // int, [A,B],     [y,n],     Timestamp,   int,             int,             [-1,1, none],        [y,n],  [none, xxx],      {[none, ~RawButton#[-xxx, +yyy]]},                                                [y,n]
    //private string _sstCsvString = "ParticipantID;Condition;TrialInstance;FirstDisplayOfArrow;GlobalIndex;LocalIndex;Stimulus;Signal;SignalDelay;ResultCode;Reactions;EndTime\n";

    // ID, Condition, Trial, GlobalTaskCount, LocalTaskCount, Stimulus, StimulusDisplay, Signal, SSD, ResultCode, Reaction, ReactionTime, Correct, Reactions
    private string _sstCsvString =
        "ParticipantID;Condition;Trial;GlobalIndex;LocalIndex;Stimulus;StimulusDisplay;Signal;SSD;ResultCode;Reaction;ReactionTime;Correct;Reactions\n";
    public string SstCsvString => _sstCsvString;


    private string _stressLevels = "ParticipantID;Condition;Phase;StressLevel\n";
    public string StressLevels => _stressLevels;


    public void AddEntryToGeneral(string pEvent, string pEventData)
    {
        _generalCsvString += participantID + ";" + condition + ";" + phases[_phaseCounter] + ";" + pEvent + ";" + pEventData + ";" + GetTimestamp() + "\n";
    }

    public void AddEntryToStressLevel(string level, string phase)
    {
        _stressLevels += participantID + ";" + condition + ";" + level + ";" + phase + "\n";
    }

    public void AddEntryToCreation(string action, string chunkID)
    {
        _townCreationCsvString +=  participantID + ";" + condition + ";" + action + ";" + chunkID + ";" + GetTimestamp() + "\n";
    }

    public void AddEntryToSst(string sstData)
    {
        _sstCsvString += participantID + ";" + condition + ";" + sstData + ";" + "\n";
    }
    
    public static CSVWriter Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
                Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void SetCondition(string condition)
    {
        this.condition = condition;
    }
    public void SetParticipantId(string id)
    {
        participantID = id;
    }

    // timestamp in utc
    public static string GetTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }
}
