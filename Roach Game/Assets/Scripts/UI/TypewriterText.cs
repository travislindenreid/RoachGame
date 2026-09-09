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
    [SerializeField] private FriendData _roachLordFriendData;

    private bool _animating;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void SetText(string text)
    {
        StartCoroutine(RevealText(text));
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
    private IEnumerator RevealText(string message)
    {
        DialogueNode node = DialogueRunner._Instance._CurrentNode;
        bool isRoachLord = node != null
            && _roachLordFriendData != null
            && node._Speaker == _roachLordFriendData
            && node._RoachLordWordTimings != null
            && node._RoachLordWordTimings.Length > 0;

        if (isRoachLord)
        {
            yield return RevealTextWithWordTimings(node, message);
        }
        else
        {
            yield return RevealTextLegacy(message);
        }

        FinishAnimation();
    }

    // ------------------------------------------------------------------------
    // Legacy per-character reveal used by every non-Roach-Lord speaker,
    // unchanged from the original system.
    private IEnumerator RevealTextLegacy(string message)
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
    }

    // ------------------------------------------------------------------------
    // Roach Lord ONLY: plays the full voice clip once, then reveals text
    // according to each word's own duration/delay from the node data.
    private IEnumerator RevealTextWithWordTimings(DialogueNode node, string message)
    {
        _animating = true;

        _text.text = message;
        _text.maxVisibleCharacters = 0;
        _text.ForceMeshUpdate();

        int maxChars = _text.textInfo.characterCount;

        if (node._RoachLordVoiceClip != null)
        {
            EventBus._Instance.InvokePlayRoachLordVoiceClip(node._RoachLordVoiceClip);
        }

        int revealedChars = 0;
        foreach(WordTimingData wordData in node._RoachLordWordTimings)
        {
            int wordLength = wordData.word.Length;
            float delayPerChar = wordLength > 0 ? wordData.typewriterDuration / wordLength : 0f;

            int wordCharsRevealed = 0;
            float elapsed = 0f;
            while (wordCharsRevealed < wordLength && revealedChars < maxChars)
            {
                elapsed += Time.deltaTime;
                int targetChars = delayPerChar > 0
                    ? Mathf.Min(Mathf.FloorToInt(elapsed / delayPerChar), wordLength)
                    : wordLength;

                if (targetChars > wordCharsRevealed)
                {
                    int delta = targetChars - wordCharsRevealed;
                    wordCharsRevealed = targetChars;
                    revealedChars += delta;
                    _text.maxVisibleCharacters = revealedChars;
                }

                yield return null;
            }

            yield return new WaitForSeconds(wordData.delayAfterWord);
        }

        _text.maxVisibleCharacters = maxChars;
    }

    // ------------------------------------------------------------------------
    private void FinishAnimation ()
    {
        _text.maxVisibleCharacters = _text.textInfo.characterCount;
        _animating = false;
        EventBus._Instance.InvokeTyperwriterFinished();
    }
}
