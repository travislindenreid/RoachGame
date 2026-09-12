/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Cinematics/PostEffectController.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Cinematics
 * Created Date: Wednesday, July 1st 2026, 5:54:45 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostEffectController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Volume _ppVolume;
    [SerializeField] private float _fadeInDurationSeconds = 2.0f;
    [SerializeField] private float _fadeOutDurationSeconds = 2.0f;
    [SerializeField] private float _maxBattleMoldValue = 0.4f;
    [SerializeField][Range(0,1)] private float _seq0MoldStartValue = 0.0f;
    [SerializeField][Range(0,1)] private float _seq0MoldEndValue = 0.4f;
    [SerializeField] private float _seq0MoldTime = 10.0f;
    [SerializeField][Range(0,1)] private float _seq3MoldStartValue = 0.0f;
    [SerializeField][Range(0,1)] private float _seq3MoldEndValue = 0.3f;
    [SerializeField] private float _seq3MoldTime = 5.0f;
    [SerializeField] private float _seq7MoldIncrease = 0.1f;
    [SerializeField] private float _seq7MoldTime = 10.0f;

    private ColorAdjustments _colorAdjustments;
    private MoldPsychosisEffectVolumeComponent _moldPostEffect;

    private bool _doFadeIn;
    private bool _doFadeOut;
    private float _fadeTimeSeconds;

    private bool _doMoldFadeIn;
    private float _moldMaxTimeSeconds;
    private float _moldTimeSeconds;
    private float _moldStartVal;
    private float _moldEndVal;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public static PostEffectController _Instance { get; private set; }

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
    private void Start()
    {
        _ppVolume.profile.TryGet<ColorAdjustments>(out _colorAdjustments);
        _ppVolume.profile.TryGet<MoldPsychosisEffectVolumeComponent>(out _moldPostEffect);
        Assert.IsNotNull(_colorAdjustments);
        Assert.IsNotNull(_moldPostEffect);

        DisableMoldPsychosis();

        SetFadeInNormalizedValue(1.0f);

        EventBus._Instance.PlayerDamaged += HandlePlayerHealthChanged;
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        if(_doFadeIn)
        {
            _fadeTimeSeconds += Time.deltaTime;

            SetFadeInNormalizedValue((float)_fadeTimeSeconds / (float)_fadeInDurationSeconds);

            if(_fadeTimeSeconds >= _fadeInDurationSeconds)
            {
                SetFadeInNormalizedValue(1.0f);
                _doFadeIn = false;
            }
        }
        else if(_doFadeOut)
        {
            _fadeTimeSeconds += Time.deltaTime;

            SetFadeOutNormalizedValue((float)_fadeTimeSeconds / (float)_fadeOutDurationSeconds);

            if(_fadeTimeSeconds >= _fadeOutDurationSeconds)
            {
                SetFadeOutNormalizedValue(1.0f);
                _doFadeOut = false;
            }
        }

        if(_doMoldFadeIn)
        {
            _moldTimeSeconds += Time.deltaTime;

            float tNorm = Mathf.InverseLerp(0, _moldMaxTimeSeconds, _moldTimeSeconds);
            SetMoldValue(Mathf.Lerp(_moldStartVal, _moldEndVal, tNorm));

            if(_moldTimeSeconds >= _moldMaxTimeSeconds)
            {
                SetMoldValue(_moldEndVal);
                _doMoldFadeIn = false;
            }
        }
    }

    // ------------------------------------------------------------------------
    private void HandlePlayerHealthChanged (bool hasAttacker, Vector3 attackerPosition)
    {
        if(Player._Instance._AtMaxHealth)
        {
            SetMoldValue(_seq3MoldStartValue);
        }
        else
        {
            _doMoldFadeIn = false;
            SetMoldValue((1.0f - Player._Instance._HealthPercent) * _maxBattleMoldValue);
        }
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void DisableMoldPsychosis ()
    {
        _moldPostEffect.useMold.SetValue(new BoolParameter(false, true));
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartSeq0MoldAnimation ()
    {
        StartMoldAnimation(_seq0MoldStartValue, _seq0MoldEndValue, _seq0MoldTime);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartSeq3MoldAnimation ()
    {
        StartMoldAnimation(_seq3MoldStartValue, _seq3MoldEndValue, _seq3MoldTime);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartSeq7MoldAnimation ()
    {
        StartMoldAnimation(
            _moldPostEffect.moldCoverage.value,
            _moldPostEffect.moldCoverage.value + _seq7MoldIncrease,
            _seq7MoldTime
        );
    }

    // ------------------------------------------------------------------------
    public void StartMoldAnimation (float startVal, float endVal, float time)
    {
        _moldTimeSeconds = 0.0f;
        _doMoldFadeIn = true;
        _moldMaxTimeSeconds = time;
        _moldStartVal = startVal;
        _moldEndVal = endVal;
        _moldPostEffect.useMold.SetValue(new BoolParameter(true, true));
        SetMoldValue(_moldStartVal);
    }

    // ------------------------------------------------------------------------
    private void SetMoldValue (float val)
    {
        _moldPostEffect.moldCoverage.SetValue(new FloatParameter(val, true));
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void SetWhiteBackgroundEnabled (bool enabled)
    {
        _moldPostEffect.useWhiteBackground.SetValue(new BoolParameter(enabled, true));
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void SetVideo1Enabled (bool enabled)
    {
        _moldPostEffect.useVideo1.SetValue(new BoolParameter(enabled, true));
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void SetVideo2Enabled (bool enabled)
    {
        _moldPostEffect.useVideo2.SetValue(new BoolParameter(enabled, true));
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void SetVideo3Enabled (bool enabled)
    {
        _moldPostEffect.useVideo3.SetValue(new BoolParameter(enabled, true));
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartFadeIn ()
    {
        _doFadeIn = true;
        _doFadeOut = false;
        _fadeTimeSeconds = 0;

        SetFadeInNormalizedValue(0);
    }

    // ------------------------------------------------------------------------
    // timeline signal callback
    public void StartFadeOut ()
    {
        _doFadeIn = false;
        _doFadeOut = true;
        _fadeTimeSeconds = 0;
    }

    // ------------------------------------------------------------------------
    private void SetFadeInNormalizedValue(float t)
    {
        _colorAdjustments.colorFilter.SetValue(new ColorParameter(
            Color.Lerp(Color.black, Color.white, t),
            true
        ));
    }

    // ------------------------------------------------------------------------
    private void SetFadeOutNormalizedValue(float t)
    {
        _colorAdjustments.colorFilter.SetValue(new ColorParameter(
            Color.Lerp(Color.white, Color.black, t),
            true
        ));
    }
}
