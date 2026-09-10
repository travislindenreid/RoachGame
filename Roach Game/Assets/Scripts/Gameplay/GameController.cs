/*
 * Filename: GameController.cs
 * Created: 06/30/26, 1:20:23 pm
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

// Handles hard-coded clues for very specific sequence triggers
public class GameController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [Header("Game start")]
    [SerializeField] private ClueData _gameStartClue;
    [Header("First roach gun")]
    [SerializeField] private ClueData _firstRoachHitClue;
    [SerializeField] private ClueData _postHitFirstRoach;
    [SerializeField] private Sequence _firstRoachGunSequence;
    [Header("Second roach gun")]
    [SerializeField] private ClueData _secondRoachGunClue;
    [SerializeField] private ClueData _postSecondRoachGun;
    [SerializeField] private Sequence _secondRoachGunSequence;
    [Header("Player gets gun")]
    [SerializeField] private Sequence _playerGetsGunSequence;
    [SerializeField] private ClueData _playerGunClue;
    [Header("Roach world sequence")]
    [SerializeField] private ClueData _roachWorldClue;
    [SerializeField] private ClueData _goBackToAptClue;
    [Header("Other")]
    [SerializeField] private GameObject _bulletHoleObj;

    private bool _hitFirstRoach;
    private Roach _targetRoach;
    private List<Roach> _activeRoaches;
    private List<ClueData> _unlockedClues;
    private ClueData _loadWorldClue;
    private List<GameObject> _bulletHoles;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public bool _WaitingForFirstRoachGunCinematic => SequenceController._Instance._ActiveSequence == _firstRoachGunSequence && !_hitFirstRoach;
    public bool _WaitingForSecondRoachGunCinematic => SequenceController._Instance._ActiveSequence == _secondRoachGunSequence;
    public bool _WaitingForPlayerGunCinematic => SequenceController._Instance._ActiveSequence == _playerGetsGunSequence;

    public static GameController _Instance { get; private set; }
    
    public Roach _TargetRoach => _targetRoach;
    public List<Roach> _ActiveRoaches => _activeRoaches;
    public int _LivingRoachCount => _activeRoaches == null ? 0 : _activeRoaches.Count(r => !r._IsDead);
    public bool _ReadyForHealthDisplay => _hitFirstRoach;
    public bool _GameStarted => IsUnlocked(_gameStartClue);

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

        DontDestroyOnLoad(gameObject);

        _activeRoaches = new List<Roach>();
        _unlockedClues = new List<ClueData>();
    }

    // ------------------------------------------------------------------------
    private void Start ()
    {
        EventBus._Instance.EnemyHit += HandleAttackableHit;
        EventBus._Instance.ClueUnlocked += HandleClueUnlocked;
        EventBus._Instance.SequenceStarted += HandleSequenceStarted;

        _bulletHoles = new List<GameObject>();
    }

    // ------------------------------------------------------------------------
    public void StartGame ()
    {
        EventBus._Instance.InvokeClueUnlocked(_gameStartClue);
    }

    // ------------------------------------------------------------------------
    public void CreateBulletHole (Vector3 location, Vector3 normal)
    {
        _bulletHoles.Add(
            Instantiate(
                _bulletHoleObj,
                location,
                Quaternion.LookRotation(normal)
            )
        );
    }

    // ------------------------------------------------------------------------
    public void SetTargetRoach (Roach roach)
    {
        _targetRoach = roach;
    }

    // ------------------------------------------------------------------------
    private void HandleSequenceStarted(Sequence sequence)
    {
        foreach(GameObject obj in _bulletHoles)
        {
            Destroy(obj);
        }
        _bulletHoles.Clear();

        _activeRoaches.Clear();
        _activeRoaches.AddRange(sequence._Roaches);
    }

    // ------------------------------------------------------------------------
    private void HandleAttackableHit (Attackable enemy)
    {
        // don't bother counting dead roaches if we're checking for cinematic events
        if(_WaitingForFirstRoachGunCinematic && enemy is Roach)
        {
            //Debug.Log("set target roach: " + roach);
            _hitFirstRoach = true;
            SetTargetRoach((Roach)enemy);
            EventBus._Instance.InvokeClueUnlocked(_firstRoachHitClue);
        }
        else if(_WaitingForSecondRoachGunCinematic && enemy is Roach)
        {
            SetTargetRoach((Roach)enemy);
            EventBus._Instance.InvokeClueUnlocked(_secondRoachGunClue);
        }
        else
        {
            if(SequenceController._Instance._ActiveSequence._EndSequenceTarget != null)
            {
                if(SequenceController._Instance._ActiveSequence._EndSequenceTarget == enemy && enemy._IsDead)
                {
                    SequenceController._Instance.EndCurrentSequence();
                }
            }
            else
            {
                int deadRoaches = _activeRoaches.Count(r => r._IsDead);
                //Debug.LogFormat("dead roaches: {0}; total roaches: {1}", deadRoaches, _activeRoaches.Count());

                if(_WaitingForPlayerGunCinematic && deadRoaches == _activeRoaches.Count())
                {
                    EventBus._Instance.InvokeClueUnlocked(_playerGunClue);
                }
                else if(deadRoaches == _activeRoaches.Count())
                {
                    SequenceController._Instance.EndCurrentSequence();
                } 
            }
        }
    }

    // ------------------------------------------------------------------------
    public bool IsUnlocked(DiscoverableData discoverable)
    {
        if(discoverable._RequiredClues == null ||
            discoverable._RequiredClues.Length == 0)
        {
            return true;
        }

        foreach(ClueData clue in discoverable._RequiredClues)
        {
            if(!IsUnlocked(clue))
            {
                return false;
            }
        }
        return true;
    }

    // ------------------------------------------------------------------------
    public bool IsUnlocked(ClueData clue)
    {
        if(!_unlockedClues.Contains(clue))
        {
            return false;
        }
        return true;
    }

    // ------------------------------------------------------------------------
    private void HandleClueUnlocked (ClueData clue)
    {
        if(!_unlockedClues.Contains(clue))
        {
            _unlockedClues.Add(clue);
        }

        if(clue == _postHitFirstRoach || clue == _postSecondRoachGun)
        {
            _targetRoach = null;
        }
        else if(clue == _roachWorldClue)
        {
            LoadRoachWorld();
        }
        else if(clue == _goBackToAptClue)
        {
            LoadAptWorld();
        }
    }

    // ------------------------------------------------------------------------
    private void FinishSceneLoad (Scene scene, LoadSceneMode mode)
    {
        SequenceController._Instance.RefreshSequenceMap();
        SequenceController._Instance.HandleClueUnlocked(_loadWorldClue);

        if(_loadWorldClue == _roachWorldClue)
        {
            CameraCinematics._Instance.OpenRoachWorld();
        }
        else if(_loadWorldClue == _goBackToAptClue)
        {
            CameraCinematics._Instance.OpenAptWorld();
            LightingController._Instance.LoadEndSequence();
        }

        SceneManager.sceneLoaded -= FinishSceneLoad;
    }

    // ------------------------------------------------------------------------
    public void LoadRoachWorld ()
    {
        SceneManager.LoadScene(1);
        _loadWorldClue = _roachWorldClue;
        SceneManager.sceneLoaded += FinishSceneLoad;
    }

    // ------------------------------------------------------------------------
    private void LoadAptWorld ()
    {
        SceneManager.LoadScene(0);
        _loadWorldClue = _goBackToAptClue;
        SceneManager.sceneLoaded += FinishSceneLoad;
    }

#if UNITY_EDITOR
    // ------------------------------------------------------------------------
    public void DebugKillAllRoaches ()
    {
        if(_WaitingForFirstRoachGunCinematic || _WaitingForSecondRoachGunCinematic)
        {
            // only kill one roach if we're waiting on a roach gun cinematic
            // if we don't, the foreach loop below won't be able to finish executing
            _activeRoaches[0].DebugKill();
        }
        else
        {
            foreach(Roach roach in _activeRoaches)
            {
                roach.DebugKill();
            }
        }
    }
#endif
}