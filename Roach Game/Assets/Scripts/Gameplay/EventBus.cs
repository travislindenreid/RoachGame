/*
 * File: EventBus.cs
 * Created: 25/05/2026, 11:44:29 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;

public class EventBus : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Events
    // ------------------------------------------------------------------------
    public delegate void DialogueNodeDelegate(DialogueNode node);
    public event DialogueNodeDelegate VisitDialogueNode;

    public delegate void ClueDelegate(ClueData clue);
    public event ClueDelegate ClueUnlocked;

    public delegate void EmptyDelegate();
    public event EmptyDelegate PlayerHealthChanged;
    public event EmptyDelegate PlayerDied;
    public event EmptyDelegate PlayTypewriter;
    public event EmptyDelegate TyperwriterFinished;

    public delegate void AudioClipDelegate(AudioClip clip);
    public event AudioClipDelegate PlayRoachLordVoiceClip;
    public event EmptyDelegate StopRoachLordVoiceClip;

    public delegate void PlayerMovementTypeDelegate(PlayerMovementType movementType);
    public event PlayerMovementTypeDelegate PlayerMovementChanged;

    public delegate void RoachDelegate(Roach roach);
    public event RoachDelegate RoachHit;
    public event RoachDelegate RoachCollected;

    public delegate void SequenceDelegate(Sequence sequence);
    public event SequenceDelegate SequenceStarted;



    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    public static EventBus _Instance { get; private set; }

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
    public void InvokeSequenceStarted(Sequence sequence)
    {
        SequenceStarted?.Invoke(sequence);
    }

    // ------------------------------------------------------------------------
    public void InvokeClueUnlocked(ClueData clue)
    {
        Debug.LogFormat("unlocked clue: {0}", clue._Name);
        ClueUnlocked?.Invoke(clue);
    }

    // ------------------------------------------------------------------------
    public void InvokeVisitDialogue(DialogueNode node)
    {
        VisitDialogueNode?.Invoke(node);
    }

    // ------------------------------------------------------------------------
    public void InvokePlayerMovementChanged (PlayerMovementType movementType)
    {
        PlayerMovementChanged?.Invoke(movementType);
    }

    // ------------------------------------------------------------------------
    public void InvokeRoachHit (Roach roach)
    {
        RoachHit?.Invoke(roach);
    }

    // ------------------------------------------------------------------------
    public void InvokeRoachCollected (Roach roach)
    {
        RoachCollected?.Invoke(roach);
    }

    // ------------------------------------------------------------------------
    public void InvokePlayerHealthChanged ()
    {
        PlayerHealthChanged?.Invoke();
    }

    // ------------------------------------------------------------------------
    public void InvokePlayerDied ()
    {
        PlayerDied?.Invoke();
    }

    // ------------------------------------------------------------------------
    public void InvokeTyperwriterFinished()
    {
        TyperwriterFinished?.Invoke();
    }

    // ------------------------------------------------------------------------
    public void InvokePlayTypewriter()
    {
        PlayTypewriter?.Invoke();
    }

    // ------------------------------------------------------------------------
    public void InvokePlayRoachLordVoiceClip(AudioClip clip)
    {
        PlayRoachLordVoiceClip?.Invoke(clip);
    }

    // ------------------------------------------------------------------------
    public void InvokeStopRoachLordVoiceClip()
    {
        StopRoachLordVoiceClip?.Invoke();
    }
}