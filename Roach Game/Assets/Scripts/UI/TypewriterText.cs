/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI/TypewriterText.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI
 * Created Date: Thursday, July 2nd 2026, 11:07:32 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _characterRevealSpeedSeconds = 0.01f;
    
    private bool _animating;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void SetText(string text)
    {
        StartCoroutine(IncreaseMaxVisibleChar(text));
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        if(_animating)
        {

            if (Input.GetMouseButtonDown(0))
            {
                StopAllCoroutines();
                FinishAnimation();
            }
        }
    }

    // ------------------------------------------------------------------------
    IEnumerator IncreaseMaxVisibleChar(string message)
    {
        _animating = true;

        _text.text = message;
        _text.maxVisibleCharacters = 0;
        _text.ForceMeshUpdate();

        int maxChars = _text.textInfo.characterCount;
        while (_text.maxVisibleCharacters < maxChars)
        {
            int nextIndex = _text.maxVisibleCharacters;
            char nextChar = _text.textInfo.characterInfo[nextIndex].character;

            _text.maxVisibleCharacters++;

            if (!char.IsWhiteSpace(nextChar))
            {
                EventBus._Instance.InvokePlayTypewriter();
            }

            yield return new WaitForSeconds(_characterRevealSpeedSeconds);
        }

        FinishAnimation();
    }

    // ------------------------------------------------------------------------
    private void FinishAnimation ()
    {
        _text.maxVisibleCharacters = _text.textInfo.characterCount;
        _animating = false;
        EventBus._Instance.InvokeTyperwriterFinished();
    }
}
