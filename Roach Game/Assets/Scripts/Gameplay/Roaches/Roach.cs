/*
 * File: Roach.cs
 * Created: 26/05/2026, 5:19:10 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Splines;

public partial class Roach : Attackable
{
    // ------------------------------------------------------------------------
    // Types
    // ------------------------------------------------------------------------
    private enum RoachStateType
    {
        Idle,
        RandomRunning,
        Dead,
        Collected,
        Attacking,
        Cinematic,
        StayIdle,
        RunOnce,
        PatternRunning,
        Divebomb
    }

    private enum MovementPlane
    {
        XZ, XY, YZ
    }

    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [Header("Unique Values")]
    [SerializeField] private bool _divebomb;
    [SerializeField] private bool _isDocile;
    [SerializeField] private bool _isImmobile;
    [SerializeField] private bool _doCinematicRunInCircle;
    [SerializeField] private MovementPlane _movementPlane = MovementPlane.XZ;
    [Header("Movement")]
    [SerializeField] private Vector2 _idleTimeMinMax;
    [SerializeField] private float _pathKnotDistance = 0.5f;
    [SerializeField] private float _hostileMovementPathDistanceLvl1 = 2.0f;
    [SerializeField] private NavMeshAgent _agent;
    [Header("Splines")]
    [SerializeField] private SplineContainer _movementSplineContainer;
    [SerializeField] private SplineAnimate _movementSplineAnimator;
    [SerializeField] private SplineAnimate _deathSplineAnimator;
    [SerializeField] private SplineContainer _hostileMovementSplineContainer;
    [SerializeField] private SplineAnimate _hostileMovementSplineAnimate;
    [SerializeField] private SplineAnimate _divebombSplineAnimator;
    [SerializeField] private Transform _roachSplines;
    [Header("Antennae")]
    [SerializeField] private Transform _leftAntennae;
    [SerializeField] private Transform _rightAntennae;
    [SerializeField] private Vector3 _antennaeAnimMin;
    [SerializeField] private Vector3 _antennaeAnimMax;
    [SerializeField] private float _antennaeFlipTime;
    [Header("Legs")]
    [SerializeField] private Transform[] _legs;
    [SerializeField] private Vector3 _legAnim;
    [SerializeField] private float _legFlipTime;
    [Header("Health")]
    [SerializeField] private ParticleSystem _bloodParticles;
    [SerializeField] private GameObject _healthCanvas;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private Collider _collider;
    [Header("Weapons")]
    [SerializeField] private float _timeToDivebomb = 10.0f;
    [SerializeField] private int _gunLevel = 1;
    [SerializeField] private RoachWeapon _level1Gun;
    [SerializeField] private RoachWeapon _level2Gun;
    [SerializeField] private RoachWeapon _level3Gun;
    [SerializeField] private float _timeBeforeWeaponUse = 1.5f;
    [SerializeField] private float _timeAfterWeaponUse = 0.5f;
    [Header("Collection")]
    [SerializeField] private GameObject _collectUI;
    [Header("Cinematics")]
    [SerializeField] private PlayableDirector _firstGunTimeline;
    [SerializeField] private PlayableDirector _secondGunTimeline;
    [SerializeField] private PlayableDirector _divebombSound;

    // weapon
    private RoachWeapon _gun;
    [SerializeField] private PlayableDirector _activeCinematic;

    // shared state variables
    private bool _hostile;
    private RoachState _currentState;
    private bool _moveAlt; // for movement states that go back and forth
    private int _moveState; // for movement states with >2 substates
    private Vector3[] _lvl1Positions;
    private Vector3[] _lvl2Positions;

    private MeshRenderer[] _renderers;

    // antennae rotation
    private Vector3 _leftAntennaeNeutralPos;
    private Quaternion _leftAntennaeNeutralRot;
    private Vector3 _rightAntennaeNeutralPos;
    private Quaternion _rightAntennaeNeutralRot;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    protected override void Start ()
    {
        base.Start();

        AssignGun();

        _healthCanvas.SetActive(false);

        _roachSplines.SetParent(null);

        _leftAntennaeNeutralPos = _leftAntennae.localPosition;
        _leftAntennaeNeutralRot = _leftAntennae.localRotation;

        _rightAntennaeNeutralPos = _rightAntennae.localPosition;
        _rightAntennaeNeutralRot = _rightAntennae.localRotation;

        _collectUI.SetActive(false);

        _renderers = GetComponentsInChildren<MeshRenderer>().ToArray();

        EventBus._Instance.SequenceStarted += HandleSequenceStarted;

        EnterState(RoachStateType.StayIdle);
    }

    // ------------------------------------------------------------------------
    private void OnDisable()
    {
        EventBus._Instance.SequenceStarted -= HandleSequenceStarted;
    }

    // ------------------------------------------------------------------------
    private void Update ()
    {
        if(SequenceController._Instance == null)
        {
            return;
        }

        _currentState.RunState(Time.deltaTime);
    }

    // ------------------------------------------------------------------------
    // timeline callback
    public void Divebomb ()
    {
        EnterState(RoachStateType.Divebomb);
    }

    // ------------------------------------------------------------------------
    private void HandleSequenceStarted(Sequence sequence)
    {
        switch(sequence._GameStateType)
        {
            case GameStateType.Action:
                // avoid having roach already holding gun start running when new seq loads
                if(!(_currentState is RoachAttackingState))
                {
                    if(_divebomb)
                    {
                        EnterState(RoachStateType.Divebomb);
                    }
                    else
                    {
                        EnterState(RoachStateType.RandomRunning);
                    }
                }
                break;
            case GameStateType.Invalid:
            case GameStateType.Cinematic:
            case GameStateType.Menu:
            case GameStateType.Dialogue:
                EnterState(RoachStateType.StayIdle);
                break;
        }
        
    }

    // ------------------------------------------------------------------------
    public void Scatter ()
    {
        EnterState(RoachStateType.RunOnce);
    }

    // ------------------------------------------------------------------------
    private void OnMouseOver ()
    {
        if(SequenceController._Instance._ActiveStateType != GameStateType.Action)
        {
            return;
        }

        _currentState.OnMouseOver();
    }

    // ------------------------------------------------------------------------
    private void OnMouseExit ()
    {
        _currentState.OnMouseExit();
    }

    // ------------------------------------------------------------------------
    public void SetRoachGunLevel (int level)
    {
        _gunLevel = level;
        AssignGun();
    }

    // ------------------------------------------------------------------------
    private void AssignGun ()
    {
        switch(_gunLevel)
        {
            case 1: _gun = _level1Gun; break;
            case 2: _gun = _level2Gun; break;
            case 3: _gun = _level3Gun; break;
            default:
                Debug.LogError("Invalid roach gun level: " + _gunLevel);
                _gun = _level1Gun;
                break;
        }
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void ZoomInToRoach ()
    {
        GameController._Instance.SetTargetRoach(this);
        CameraCinematics._Instance.AnimateRoachZoomIn();
    }

    // ------------------------------------------------------------------------
    public override void Hit ()
    {
        if(SequenceController._Instance._ActiveStateType != GameStateType.Action)
        {
            return;
        }
        if(_IsDead) return;

        _health--;
        _hostile = true;

        UpdateHealthText();

        if(GameController._Instance._WaitingForFirstRoachGunCinematic)
        {
            _activeCinematic = _firstGunTimeline;
            EnterState(RoachStateType.Cinematic);
        }
        else if(GameController._Instance._WaitingForSecondRoachGunCinematic)
        {
            _activeCinematic = _secondGunTimeline;
            _gunLevel = 2;
            EnterState(RoachStateType.Cinematic);
        }
        else
        {
            if(_health <= 0)
            {
                EnterState(RoachStateType.Dead);
            }
            else if(_isDocile)
            {
                EnterState(RoachStateType.PatternRunning);
            }
            else
            {
                if(_currentState is RoachAttackingState)
                {
                    EnterState(RoachStateType.PatternRunning);
                }
                else
                {
                    EnterState(RoachStateType.Attacking);   
                }
            }
        }

        _bloodParticles.transform.SetParent(null);
        _bloodParticles.transform.position = transform.position;
        _bloodParticles.Play();

        // fire event AFTER everything else, so roach has most accurate
        //      health and state information for rest of game
        EventBus._Instance.InvokeRoachHit(this);
    }

    // ------------------------------------------------------------------------
    private void UpdateHealthText ()
    {
        bool show = _health > 0 && _health != _maxHealth;
        _healthCanvas.SetActive(show);

        if(show)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < _health; i++)
            {
                sb.Append(".");
            }
            _healthText.text = sb.ToString();
        }
    }

    // ------------------------------------------------------------------------
    // also a Timeline signal callback- do not rename
    public void ShowGun ()
    {
        _gun.gameObject.SetActive(true);
        _gun.PointAtPlayer();
    }

    // ------------------------------------------------------------------------
    protected void HideGun ()
    {
        _gun.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------------
    // Timeline signal callback
    public void ShowLevel2Gun ()
    {
        _gun = _level2Gun;
        _gun.gameObject.SetActive(true);
        _gun.PointAtPlayer();
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void FinishFirstGunCinematic ()
    {
        EnterState(RoachStateType.Attacking);
    }

    // ------------------------------------------------------------------------
    public void KilledPlayer ()
    {
        EnterState(RoachStateType.RandomRunning);
    }

    // ------------------------------------------------------------------------
    public void ResetRoach(Vector3 originalPos)
    {
        ResetAntennae();

        _agent.enabled = true;
        _collider.enabled = true;
        _movementSplineAnimator.enabled = true;
        _deathSplineAnimator.enabled = true;

        _health = _maxHealth;
        UpdateHealthText();

        _hostile = false;

        transform.SetParent(null);
        transform.position = originalPos;

        EnterState(RoachStateType.Idle);
    } 

    // ------------------------------------------------------------------------
    private void EnterState(RoachStateType newState)
    {
        _currentState?.ExitState();

        switch(newState)
        {
            case RoachStateType.Idle: _currentState = new RoachIdleState(); break;
            case RoachStateType.RandomRunning: _currentState = new RoachRandomRunningState(); break;
            case RoachStateType.Attacking: _currentState = new RoachAttackingState(); break;
            case RoachStateType.Dead: _currentState = new RoachDeadState(); break;
            case RoachStateType.Collected: _currentState = new RoachCollectedState(); break;
            case RoachStateType.Cinematic: _currentState = new RoachCinematicState(); break;
            case RoachStateType.StayIdle: _currentState = new RoachStayIdleState(); break;
            case RoachStateType.RunOnce: _currentState = new RoachRunOnceState(); break;
            case RoachStateType.PatternRunning: _currentState = new RoachPatternRunningState(); break;
            case RoachStateType.Divebomb: _currentState = new RoachDivebombState(); break;
            default: Debug.LogError("unhandled roach state: " + newState); break;
        }
        //Debug.LogFormat("{0} new state: {1}", gameObject.name, _currentState);
        _currentState.EnterState(this);
    }

    // ------------------------------------------------------------------------
    private void ResetAntennae ()
    {
        _leftAntennae.localRotation = _leftAntennaeNeutralRot;
        _leftAntennae.localPosition = _leftAntennaeNeutralPos;

        _rightAntennae.localRotation = _rightAntennaeNeutralRot;
        _rightAntennae.localPosition = _rightAntennaeNeutralPos;
    }

    // ------------------------------------------------------------------------
    private void OnDrawGizmos ()
    {
        if(_currentState == null) return;

        _currentState.OnDrawGizmos();
    }

#if UNITY_EDITOR
    // ------------------------------------------------------------------------
    public void DebugKill ()
    {
        for(int i = 0; i < _health; i++)
        {
            Hit();
        }
    }
#endif
}