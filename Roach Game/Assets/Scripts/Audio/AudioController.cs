/*
 * File: AudioController.cs
 * Created: 26/05/2026, 9:20:16 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField] private AudioSource _uiAudioSource;
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioSource _footstepsAudioSource;
    [SerializeField] private AudioClip _roachHitClip;
    [SerializeField] private AudioClip _step1Clip;
    [SerializeField] private AudioClip _step2Clip;
    [SerializeField] private AudioClip _roachlordRevealSong;

    private bool _playSteps;
    private float _clipTime;
    private bool _flipClip;
    private float _globalVolume = 1.0f;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public static AudioController _Instance { get; private set; }
    public float _GlobalVolume => _globalVolume;

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
        EventBus._Instance.RoachHit += HandleRoachHit;
        EventBus._Instance.SequenceStarted += HandleSequenceStarted;
        EventBus._Instance.PlayerMovementChanged += HandlePlayerMovementChanged;
    }

    // ------------------------------------------------------------------------
    private void Update ()
    {
        if(_playSteps)
        {
            _clipTime += Time.deltaTime;
            if(_clipTime >= _footstepsAudioSource.clip.length)
            {
                _clipTime = 0.0f;
                _flipClip = !_flipClip;
                _footstepsAudioSource.clip = _flipClip ? _step2Clip : _step1Clip;
                _footstepsAudioSource.Play();
            }
        }
    }

    // ------------------------------------------------------------------------
    public void ChangeVolume (float volume)
    {
        _globalVolume = volume;
        foreach(AudioSourceHelper audioSource in FindObjectsByType<AudioSourceHelper>(FindObjectsInactive.Include))
        {
            audioSource.SetVolume(_globalVolume);
        }
    }

    // ------------------------------------------------------------------------
    public void PlayMusicBackwards ()
    {
        _musicAudioSource.clip = _roachlordRevealSong;
        _musicAudioSource.pitch = -1;
        _musicAudioSource.Play();
    }

    // ------------------------------------------------------------------------
    public void PlayButtonClick ()
    {
        Debug.Log("play click audio");
        _uiAudioSource.time = 0;
        _uiAudioSource.Play();
    }

    // ------------------------------------------------------------------------
    private void HandleRoachHit (Roach roach)
    {
        _sfxAudioSource.PlayOneShot(_roachHitClip);
    }

    // ------------------------------------------------------------------------
    private void HandleSequenceStarted(Sequence sequence)
    {
        switch(sequence._AudioType)
        {
            case SequenceAudioType.ContinuePreviousClip:
                // do nothing (intentionally)
                break;
            case SequenceAudioType.StopClipOnly:
                _musicAudioSource.Stop();
                break;
            case SequenceAudioType.PlayNewClip:
                Assert.IsNotNull(sequence._Music);
                _musicAudioSource.clip = sequence._Music;
                _musicAudioSource.Play();   
                break;
        }
    }

    // ------------------------------------------------------------------------
    private void HandlePlayerMovementChanged (PlayerMovementType movementType)
    {
        switch(movementType)
        {
            case PlayerMovementType.Still:
                _footstepsAudioSource.Stop();
                _playSteps = false;
                break;
            case PlayerMovementType.Walking:
            case PlayerMovementType.Running:    
                StartPlayingSteps();
                break;
            default:
                Debug.LogError("Unhandled player movement type.");
                break;
        }
    }

    // ------------------------------------------------------------------------
    private void StartPlayingSteps ()
    {
        // TODO... play faster for running
        //Debug.Log("start playing steps");
        _footstepsAudioSource.clip = _step1Clip;
        _flipClip = false;
        _footstepsAudioSource.Play();
        _playSteps = true;
        _clipTime = 0;
    }
}
