/*
 * File: DialogueNode.cs
 * Created: 24/05/2026, 12:27:50 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class DialogueOption
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------

    [SerializeField] private string _optionText; // what the speaker says in this option
    [SerializeField] private DialogueNode _nextNode; // the next node this option leads to

    public string _OptionText => _optionText;
    public DialogueNode _NextNode => _nextNode;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------

    public DialogueOption(string text, DialogueNode node)
    {
        _optionText = text;
        _nextNode = node;
    }

    // ------------------------------------------------------------------------
    public override string ToString()
    {
        return _optionText;
    }

    // ------------------------------------------------------------------------
    public void LoadNodeFromAsset (DialogueNode node)
    {
        _nextNode = node;
    }
}

[Serializable]
public class WordTimingData
{
    [Tooltip("The word or phrase to display.")]
    public string word;

    [Tooltip("Total time in seconds to type out this word/phrase.")]
    public float typewriterDuration = 0.2f;

    [Tooltip("Delay in seconds after this word finishes before the next one begins.")]
    public float delayAfterWord = 0.05f;
}

[CreateAssetMenu(fileName = "MessageData", menuName = "Dialogue/Message", order = 1)]
public class DialogueNode : DiscoverableData
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private FriendData _speaker;
    [SerializeField] private string[] _lines; // the lines of dialogue the speaker says
    [SerializeField] private DialogueOption[] _options; // the possible next nodes

    // Leave null/empty for every other speaker
    [SerializeField] private AudioClip _roachLordVoiceClip;
    [SerializeField] private WordTimingData[] _roachLordWordTimings;

    public ChatData _Chat;

    // editor stuff
    private DialogueNodeParseData _parseData;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public FriendData _Speaker => _speaker;
    public DialogueOption[] _Options => _options;
    public string[] _Lines => _lines;
    public AudioClip _RoachLordVoiceClip => _roachLordVoiceClip;
    public WordTimingData[] _RoachLordWordTimings => _roachLordWordTimings;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public bool LoadParseDataAndCreateId (
        DialogueNodeParseData parseData,
        string ifid
    ) {
        _parseData = parseData;
        _id = CreateID(ifid, _parseData._nodeId);
        return _id != -1;
    }

    // ------------------------------------------------------------------------
    public void CopyFrom(DialogueNode other)
    {
        _id = other._id;
        _speaker = other._speaker;
        _lines = other._lines;
        _options = other._options;
        _requiredClues = other._requiredClues;
    }

    // ------------------------------------------------------------------------
    private int CreateID (string ifid, string nodeId)
    {
        StringBuilder idSb = new StringBuilder(nodeId);

        string[] numbers = Regex.Split(ifid, @"\D+");
        foreach(string numText in numbers)
        {
            //Debug.Log("appended num: " + numText);
            idSb.Append(numText);
        }

        int finalId = -1;
        string finalIdText = idSb.ToString().Substring(0, 6);
        Int32.TryParse(finalIdText, out finalId);

        /*
        Debug.LogFormat(
            "created final id: {0} from ifid: {1}; node ID {2}; final text {3}",
            finalId,
            ifid,
            nodeId,
            finalIdText
        );
        */

        return finalId;
    }

    // ------------------------------------------------------------------------
    public void InitWithGameData(
        string ifid,
        Dictionary<int, DialogueNode> nodesById,
        Dictionary<string, FriendData> friendsByName,
        Dictionary<string, ClueData> cluesByName
    )
    {
        _lines = _parseData._lines;

        // find actual FriendData by speaker ID
        try
        {
            _speaker = friendsByName[_parseData._friend];
        } catch (KeyNotFoundException e)
        {
            Debug.LogError("Could not find Friend with name tag " + _parseData._friend);
        }

        // look for actual ClueData by clue name... if none is found, create one!
        List<ClueData> clues = new List<ClueData>();
        foreach(string clueName in _parseData._clues)
        {
            if(cluesByName.Keys.Contains(clueName))
            {
                clues.Add(cluesByName[clueName]);
            }
            else
            {
                ClueData clue = ScriptableObject.CreateInstance<ClueData>(); 
                clue.InitFromParseData(clueName);

                cluesByName.Add(clueName, clue);

                clues.Add(clue);
            }
        }
        _requiredClues = clues.ToArray();

        List<ClueData> cluesGiven = new List<ClueData>();
        foreach(string clueName in _parseData._cluesGiven)
        {
            if(cluesByName.Keys.Contains(clueName))
            {
                cluesGiven.Add(cluesByName[clueName]);
            }
            else
            {
                ClueData clue = ScriptableObject.CreateInstance<ClueData>(); 
                clue.InitFromParseData(clueName);

                cluesByName.Add(clueName, clue);

                cluesGiven.Add(clue);
            }
        }
        _cluesGiven = cluesGiven.ToArray();

        // init options list by finding actual DialogueNode by parse Data ID
        _options = new DialogueOption[_parseData._options.Length];
        int i = 0;
        foreach(DialogueOptionParseData optionParseData in _parseData._options)
        {
            int optionNodeId = CreateID(ifid, optionParseData._nextNodeId);
            _options[i] = new DialogueOption(
                optionParseData._line,
                nodesById[optionNodeId]
            );
            i++;
        }

        //Debug.LogFormat("created dialogue node with {0} total lines; start line: {1}", _lines.Length, _lines[0]);
    }

    // ------------------------------------------------------------------------
    public void LoadClueDataAssets(ClueData[] requiredClueAssets, ClueData[] givenClueAssets)
    {
        _requiredClues = requiredClueAssets;
        _cluesGiven = givenClueAssets;
    }
}