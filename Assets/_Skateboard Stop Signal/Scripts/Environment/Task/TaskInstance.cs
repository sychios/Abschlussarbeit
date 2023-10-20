using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class TaskInstance
{
    public TaskInstance(bool pIsTrial, bool pSignal, int pGlobalTaskIndex, int pLocalTaskIndex, int pStimulus, float pSignalDelay)
    {
        isTrial = pIsTrial;
        SignalAppeared = pSignal;
        globalTaskIndex = pGlobalTaskIndex;
        localTaskIndex = pLocalTaskIndex;
        stimulus = pStimulus;
        signalDelay = pSignalDelay;
        Reactions = new List<ReactionEntry>();
    }
    
    
    private readonly bool isTrial;
    
    private int globalTaskIndex;
    private int localTaskIndex;

    private float stimulus; // -1: left, 1: right
    public long StimulusDisplay;
    public bool SignalAppeared;
    private float signalDelay;

    public int FinalResultCode;

    public string Reaction;
    public float ReactionTime;

    public bool trialIsCorrect;
    
    public List<ReactionEntry> Reactions;

    public string ToCsvString()
    {
        var s = isTrial + ";"
                        + globalTaskIndex + ";"
                        + localTaskIndex + ";"
                        + stimulus + ";"
                        + StimulusDisplay + ";"
                        + SignalAppeared + ";"
                        + signalDelay + ";"
                        + FinalResultCode + ";" 
                        + Reaction + ";"
                        + (ReactionTime == -1f? "null":ReactionTime.ToString(CultureInfo.CurrentCulture)) + ";"
                        + trialIsCorrect + ";";// + reactionsArray
        var reactionsArray = "";
        if (Reactions != null && Reactions.Count > 0)
        {
            foreach (var reaction in Reactions)
            {
                reactionsArray += "~" + reaction.ToCsvString();
            }

            s += reactionsArray;
        }
        else
        {
            s += "none";
        }
        
        //s += "\n"; TODO: finishing line is done in CswWriter
        
        return s;
    }
}

public class ReactionEntry
{
    public ReactionEntry(string pTimestamp, float pReactionDirection)
    {
        _reactionDirection = pReactionDirection;
        _timestamp = pTimestamp;
    }
    private float _reactionDirection;
    private string _timestamp;

    public string ToCsvString()
    {
        return _reactionDirection + "#" + _timestamp;
    }
}
