/*
 * File: SequenceController.cs
 * Created: 06/06/2026, 2:48:04 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameStateType
{
    Invalid, Action, Cinematic, Dialogue, Menu
}

public enum SequenceAudioType
{
    ContinuePreviousClip, StopClipOnly, PlayNewClip
}

public partial class SequenceController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Sequence _openGameSequence;
    [SerializeField] private Sequence _playerDiedSequence;
    [SerializeField] private Player _player;

    private GameState _activeState;

    private Dictionary<ClueData, Sequence> _sequencesByClueUnlock;
    private Sequence _activeSequence;
    private Sequence _lastSequence;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public static SequenceController _Instance { get; private set; }
    public Sequence _ActiveSequence => _activeSequence;

    public GameStateType _ActiveStateType
    {
        get
        {
            if(_activeState == null) return GameStateType.Invalid;
            else if(_activeState is GameActionState) return GameStateType.Action;
            else if(_activeState is GameCinematicState) return GameStateType.Cinematic;
            else if(_activeState is GameDialogueState) return GameStateType.Dialogue;
            else if(_activeState is GameMenuState) return GameStateType.Menu;
            return GameStateType.Invalid;
        }
    }

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
    private void Start ()
    {
        RefreshSequenceMap();

        EventBus._Instance.ClueUnlocked += HandleClueUnlocked;
        EventBus._Instance.PlayerDied += HandlePlayerDied;

        ActivateSequence(_openGameSequence);
    }

    // ------------------------------------------------------------------------
    private void OnDisable()
    {
        EventBus._Instance.ClueUnlocked -= HandleClueUnlocked;
        EventBus._Instance.PlayerDied -= HandlePlayerDied;
    }

    // ------------------------------------------------------------------------
    public void RefreshSequenceMap ()
    {
        _sequencesByClueUnlock = new Dictionary<ClueData, Sequence>();
        foreach(Sequence sequence in FindObjectsByType<Sequence>())
        {
            if(_sequencesByClueUnlock.Keys.Contains(sequence._TriggerClue))
            {
                Debug.LogError("sequences have idential start keys: " + sequence + " and " + _sequencesByClueUnlock[sequence._TriggerClue]);
                continue;
            }

            _sequencesByClueUnlock.Add(sequence._TriggerClue, sequence);
        }
    }

    // ------------------------------------------------------------------------
    public void HandleClueUnlocked (ClueData clue)
    {
        if(_sequencesByClueUnlock.Keys.Contains(clue))
        {
            Sequence unlockedSeq = _sequencesByClueUnlock[clue];
            if(unlockedSeq != null)
            {
                ActivateSequence(unlockedSeq);
            }   
        }
    }

    // ------------------------------------------------------------------------
    private void HandlePlayerDied ()
    {
        ActivateSequence(_playerDiedSequence);
    }

    // ------------------------------------------------------------------------
    private void ActivateSequence (Sequence sequence)
    {
        Sequence temp = _activeSequence;
        _lastSequence = temp;
        _activeSequence = sequence;
        EnterState(sequence._GameStateType);
        //Debug.LogFormat("old seq: {0}; NEW sequence: {1}", _lastSequence, _activeSequence);
        _activeSequence.StartSequence();
    }

    // ------------------------------------------------------------------------
    public void PauseSequence ()
    {
        Player._Instance.SetInputEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    // ------------------------------------------------------------------------
    public void ResumeSequence ()
    {
        Time.timeScale = 1;
        // resume whatever input type we had before pausing
        EnterState(_activeState.StateType);
    }

    // ------------------------------------------------------------------------
    private void EnterState(GameStateType newState)
    {
        if(_activeState != null)
        {
            _activeState?.ExitState();
        }

        switch(newState)
        {
            case GameStateType.Action: _activeState = new GameActionState(); break;
            case GameStateType.Cinematic: _activeState = new GameCinematicState(); break;
            case GameStateType.Dialogue: _activeState = new GameDialogueState(); break;
            case GameStateType.Menu: _activeState = new GameMenuState(); break;
            default: Debug.LogError("unhandled game state: " + newState); break;
        }
        //Debug.LogFormat("{0} new state: {1}", gameObject.name, _activeState);
        _activeState.EnterState(this);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void EndCurrentSequence ()
    {
        //Debug.Log("ended sequence");
        _activeSequence.EndSequence();
    }

    // ------------------------------------------------------------------------
    public void RestartActionSequence ()
    {
        if(_lastSequence == null || _lastSequence._GameStateType != GameStateType.Action)
        {
            Debug.LogError("No active sequence, or active sequence is not action.");
            return;
        }

        ActivateSequence(_lastSequence);

        _activeSequence.RestartSequence();
        EventBus._Instance.InvokeSequenceStarted(_activeSequence);
    }
}
