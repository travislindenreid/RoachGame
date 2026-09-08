/*
 * File: Sequence.cs
 * Created: 06/06/2026, 2:47:17 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Collections.Generic;
using UnityEngine;

public class Sequence : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [Header("Clues")]
    [SerializeField] private ClueData _triggerClue;
    [SerializeField] private ClueData _finishClue;
    [Header("Gameplay")]
    [SerializeField] private GameStateType _gameStateType;
    [SerializeField] private GameObject[] _objects;
    [SerializeField] private bool _disableObjectsAtEnd;
    [SerializeField] private bool _removeRoachesInHandAtEnd = true;
    [SerializeField] private DialogueNode _dialogueStartNode;
    [Header("Player & Camera Setup")]
    [SerializeField] private Transform _playerStartPos;
    [SerializeField] private Transform _cameraStartRot;
    [SerializeField] private bool _onlySetPlayerLocOnRestart;
    [Header("Audio")]
    [SerializeField] private AudioClip _music;
    [SerializeField] private SequenceAudioType _audioType;

    // stuff for action sequences to keep track of for restarting
    private Roach[] _roaches;
    private Vector3[] _roachOriginalPositions;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public ClueData _TriggerClue => _triggerClue;
    public GameStateType _GameStateType => _gameStateType;
    public AudioClip _Music => _music;
    public SequenceAudioType _AudioType => _audioType;
    public Roach[] _Roaches => _roaches == null ? FindRoaches() : _roaches;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void StartSequence ()
    {
        SetupPlayer(!_onlySetPlayerLocOnRestart);

        if(_objects != null)
        {
            foreach(GameObject obj in _objects)
            {
                obj.SetActive(true);
            }

            if(_roaches == null)
            {
                FindRoaches();
            }

            _roachOriginalPositions = new Vector3[_roaches.Length];
            for(int i = 0; i < _roaches.Length; i++)
            {
                _roachOriginalPositions[i] = _roaches[i].transform.position;
            }
        }
        else
        {
            _roaches = new Roach[0];
        }

        // MUST set up roach list before firing sequence start event
        EventBus._Instance.InvokeSequenceStarted(this);

        if(_gameStateType == GameStateType.Dialogue && _dialogueStartNode != null)
        {
            DialogueRunner._Instance.StartDialogue(_dialogueStartNode);
        }
    }

    // ------------------------------------------------------------------------
    private Roach[] FindRoaches ()
    {
        List<Roach> _roachList = new List<Roach>();
        foreach(GameObject obj in _objects)
        {
            _roachList.AddRange(obj.GetComponentsInChildren<Roach>());
        }
        _roaches = _roachList.ToArray();
        return _roaches;
    }

    // ------------------------------------------------------------------------
    public void EndSequence ()
    {
        if(_disableObjectsAtEnd && _objects != null)
        {
            foreach(GameObject obj in _objects)
            {
                obj.SetActive(false);
            }
        }

        if(_removeRoachesInHandAtEnd)
        {
            Player._Instance.RemoveRoachesFromHand();
        }

        if(_finishClue != null)
        {
            EventBus._Instance.InvokeClueUnlocked(_finishClue);
        }
    }

    // ------------------------------------------------------------------------
    public void RestartSequence()
    {
        if(_gameStateType != GameStateType.Action)
        {
            Debug.LogError("Trying to restart non-action sequence.");
            return;
        }

        for(int i = 0; i < _roaches.Length; i++)
        {
            _roaches[i].ResetRoach(_roachOriginalPositions[i]);
        }

        SetupPlayer(true);
    }

    // ------------------------------------------------------------------------
    private void SetupPlayer (bool setPlayerPosition)
    {
        if(setPlayerPosition)
        {
            if(_playerStartPos != null)
            {
                Player._Instance.TeleportTo(_playerStartPos);   
            }

            if(_cameraStartRot != null)
            {
                Player._Instance._CameraTransform.localEulerAngles = _cameraStartRot.localEulerAngles;
            }
        }

        if(_gameStateType == GameStateType.Action)
        {
            Player._Instance.SetupForActionSequence();
        }
    }
}
