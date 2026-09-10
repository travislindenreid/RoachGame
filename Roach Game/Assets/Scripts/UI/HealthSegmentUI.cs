/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI/HealthSegmentUI.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI
 * Created Date: Wednesday, September 9th 2026, 10:38:28 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;
using UnityEngine.UI;

public class HealthSegmentUI : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------

    [SerializeField] private Image _image;
    [SerializeField] private Shake _shake;
    [SerializeField] private LayoutElement _layoutElement;
    [SerializeField] private Color _maxHealthColor;
    [SerializeField] private Color _noHealthColor;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------

    public void Setup ()
    {
        _image.enabled = true;
        _image.color = _maxHealthColor;
        _shake.enabled = false;
        _layoutElement.ignoreLayout = false;
    }

    // ------------------------------------------------------------------------
    public void SetColor(float t)
    {
        _image.color = Color.Lerp(_noHealthColor, _maxHealthColor, t);
        _image.enabled = true;

        if(!_shake.enabled)
        {
            _shake.enabled = true;
            //_layoutElement.ignoreLayout = true;
        }
    }

    // ------------------------------------------------------------------------
    public void Hide()
    {
        _image.enabled = false;
        _shake.enabled = false;
        _layoutElement.ignoreLayout = false;
    }
}
