/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Cinematics/LightingController.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Cinematics
 * Created Date: Wednesday, July 1st 2026, 9:31:18 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

public class LightingController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Color _defaultLightColor = Color.white;
    [SerializeField] private Color _redLightColor = Color.red;
    [SerializeField] private float _ambientLightIntensityLightsOff = 0.2f;
    [SerializeField] private float _ambientLightIntensityLightsRed = 0.6f;
    [SerializeField] private float _ambientLightIntensityLightsOn = 1.0f;
    [SerializeField] private float _mainLightFadeInTime = 3.0f;
    [SerializeField] private float _mainLightMinIntensity = 0.0f;
    [SerializeField] private float _mainLightMaxIntensity = 1.0f;
    [SerializeField] private Vector3 _14bLightDir = Vector3.zero;

    private Light _mainDirLight;
    private bool _doLightFadeIn;
    private float _fadeInTime;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public static LightingController _Instance { get; private set; }

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
    private void Update()
    {
        if(_doLightFadeIn)
        {
            _fadeInTime += Time.deltaTime;
            if(_fadeInTime >= _mainLightFadeInTime)
            {
                _mainDirLight.intensity = _mainLightMaxIntensity;
                _doLightFadeIn = false;
            }
            else
            {
                _mainDirLight.intensity = Mathf.Lerp(
                    _mainLightMinIntensity,
                    _mainLightMaxIntensity,
                    _fadeInTime/_mainLightFadeInTime
                );
            }
        }
    } 

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void TurnLightsOff ()
    {
        _mainDirLight = FindAnyObjectByType<Light>();

        RenderSettings.ambientIntensity = _ambientLightIntensityLightsOff;
        _mainDirLight.enabled = false;
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void TurnLightsOn ()
    {
        _mainDirLight = FindAnyObjectByType<Light>();

        RenderSettings.ambientIntensity = _ambientLightIntensityLightsOn;
        _mainDirLight.enabled = true;
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void ActivateRedLights (bool activate)
    {
        _mainDirLight = FindAnyObjectByType<Light>();

        _mainDirLight.color = activate ? _redLightColor : _defaultLightColor;
        RenderSettings.ambientIntensity = activate? _ambientLightIntensityLightsRed : _ambientLightIntensityLightsOn;
    }

    // ------------------------------------------------------------------------
    // timeline callback
    public void FadeInMainLight ()
    {
        _mainDirLight = FindAnyObjectByType<Light>();
        
        _doLightFadeIn = true;
        _mainDirLight.enabled = true;
        _mainDirLight.intensity = _mainLightMinIntensity;
    }

    // ------------------------------------------------------------------------
    public void LoadEndSequence ()
    {
        _mainDirLight = FindAnyObjectByType<Light>();

        _mainDirLight.transform.eulerAngles = _14bLightDir;
    }
}