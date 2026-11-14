/*
 * File: FadeUI.cs
 * Created: 13/11/2026, 11:13:19 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Image _image;
    [SerializeField] private float _totalFadeTime;

    private float _startAlpha;
    private float _time;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Start()
    {
        _startAlpha = _image.color.a;
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        _time -= Time.deltaTime;
        _image.color = new Color(
            _image.color.r, _image.color.g, _image.color.b,
            _time/_totalFadeTime * _startAlpha
        );
        if(_time <= 0.0f)
        {
            StopFade();
        }
    } 

    // ------------------------------------------------------------------------
    public void StartFade ()
    {
        enabled = true;
        _time = _totalFadeTime;
        _image.color = new Color(
            _image.color.r, _image.color.g, _image.color.b,
            _startAlpha
        );
    }

    // ------------------------------------------------------------------------
    public void StopFade ()
    {
        enabled = false;
        _image.color = new Color(
            _image.color.r, _image.color.g, _image.color.b,
            0.0f
        );
    }
}