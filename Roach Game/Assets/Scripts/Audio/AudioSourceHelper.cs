/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Audio/AudioSourceHelper.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Audio
 * Created Date: Monday, September 7th 2026, 5:32:55 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceHelper : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private float _maxVolume;

    private AudioSource[] _audioSources;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void Start()
    {
        _audioSources = GetComponents<AudioSource>();
        SetVolume(AudioController._Instance._GlobalVolume);
    } 

    // ------------------------------------------------------------------------
    public void SetVolume (float volume)
    {
        if(_audioSources == null || _audioSources.Length == 0)
        {
            _audioSources = GetComponents<AudioSource>();
        }

        foreach(AudioSource source in _audioSources)
        {
            source.volume = _maxVolume * volume;
        }
    }
}