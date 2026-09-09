/*
 * File: DialogueRunner.cs
 * Created: 25/05/2026, 11:43:33 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    private DialogueNode _currentNode;

    public static DialogueRunner _Instance { get; private set; }
    public FriendData _CurrentSpeaker => _currentNode == null ? null : _currentNode._Speaker;
    public DialogueNode _CurrentNode => _currentNode;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this);
            return;
        }

        _Instance = this;
    }

    // ------------------------------------------------------------------------
    public void StartDialogue(DialogueNode node)
    {
        _currentNode = node;
        Debug.Log("runner visit node: " + node);
        EventBus._Instance.InvokeVisitDialogue(node);
    }

    // ------------------------------------------------------------------------
    // button callback
    public void SelectOption(DialogueNode nextNode)
    {
        _currentNode = nextNode;
        Debug.Log("selected node: " + nextNode);
        EventBus._Instance.InvokeVisitDialogue(nextNode);
    }
}