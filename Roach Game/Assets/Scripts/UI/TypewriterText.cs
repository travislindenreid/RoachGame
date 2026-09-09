/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI/TypewriterText.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI
 * Created Date: Thursday, July 2nd 2026, 11:07:32 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    private bool _isRoachLordPhraseReveal;
    private Coroutine _revealCoroutine;

    private List<string> committedChunks;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void SetText(string text)
    {
        if (_revealCoroutine != null)
        {
            StopCoroutine(_revealCoroutine);
        }

        _revealCoroutine = StartCoroutine(RevealText(text));
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        if (_animating && Input.GetMouseButtonDown(0))
        {
            SkipAnimation();
        }
    }

    // ------------------------------------------------------------------------
    private void SkipAnimation()
    {
        if (_isRoachLordPhraseReveal)
        {
            EventBus._Instance.InvokeStopRoachLordVoiceClip();
        }

        if (_revealCoroutine != null)
        {
            StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
        }

        _text.maxVisibleCharacters = _text.textInfo.characterCount;

        _animating = false;
        _isRoachLordPhraseReveal = false;
        EventBus._Instance.InvokeTyperwriterFinished();
    }

    // ------------------------------------------------------------------------
    private IEnumerator RevealText(string message)
    {
        DialogueNode node = DialogueRunner._Instance._CurrentNode;

        bool isRoachLord =
            node != null &&
            _roachLordFriendData != null &&
            node._Speaker == _roachLordFriendData &&
            node._RoachLordPhraseTimings != null &&
            node._RoachLordPhraseTimings.Length > 0;

        _isRoachLordPhraseReveal = isRoachLord;

        if (isRoachLord)
        {
            yield return RevealTextWithPhraseTimings(node);
        }
        else
        {
            yield return RevealTextLegacy(message);
        }

        FinishAnimation();
    }

    // ------------------------------------------------------------------------
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

        _text.maxVisibleCharacters = maxChars;
    }

    // ------------------------------------------------------------------------
    private IEnumerator RevealTextWithPhraseTimings(DialogueNode node)
    {
        _animating = true;

        StringBuilder textBuilder = new StringBuilder();
        List<PhraseRevealRange> phraseRanges =
            new List<PhraseRevealRange>();

        foreach (PhraseTimingData phraseData in node._RoachLordPhraseTimings)
        {
            int sourceStart = textBuilder.Length;

            textBuilder.Append(phraseData.phrase);

            if (phraseData.newLineAfterPhrase)
            {
                textBuilder.Append("\n\n");
            }

            phraseRanges.Add(new PhraseRevealRange
            {
                Data = phraseData,
                SourceStart = sourceStart,
                SourceEnd = textBuilder.Length
            });
        }

        _text.text = textBuilder.ToString();
        _text.maxVisibleCharacters = 0;
        _text.ForceMeshUpdate();

        if (node._RoachLordVoiceClip != null)
        {
            EventBus._Instance.InvokePlayRoachLordVoiceClip(
                node._RoachLordVoiceClip
            );
        }

        int revealedCharacters = 0;

        foreach (PhraseRevealRange phraseRange in phraseRanges)
        {
            int phraseEndCharacter = GetVisibleCharacterCountBeforeSourceIndex(
                phraseRange.SourceEnd
            );

            int phraseCharacterCount =
                phraseEndCharacter - revealedCharacters;

            if (phraseCharacterCount > 0)
            {
                float phraseDuration = Mathf.Max(
                    0f,
                    phraseRange.Data.typewriterDuration
                );

                if (phraseDuration <= 0f)
                {
                    revealedCharacters = phraseEndCharacter;
                    _text.maxVisibleCharacters = revealedCharacters;
                }
                else
                {
                    float elapsed = 0f;
                    float secondsPerCharacter =
                        phraseDuration / phraseCharacterCount;

                    while (revealedCharacters < phraseEndCharacter)
                    {
                        elapsed += Time.deltaTime;

                        int charactersToReveal = Mathf.Min(
                            Mathf.FloorToInt(
                                elapsed / secondsPerCharacter
                            ),
                            phraseCharacterCount
                        );

                        int targetCharacters =
                            phraseEndCharacter - phraseCharacterCount +
                            charactersToReveal;

                        if (targetCharacters > revealedCharacters)
                        {
                            revealedCharacters = targetCharacters;
                            _text.maxVisibleCharacters =
                                revealedCharacters;
                        }

                        yield return null;
                    }
                }
            }

            if (phraseRange.Data.delayAfterPhrase > 0f)
            {
                yield return new WaitForSeconds(
                    phraseRange.Data.delayAfterPhrase
                );
            }
        }
    }

    // ------------------------------------------------------------------------
    private void FinishAnimation()
    {
        _animating = false;
        _isRoachLordPhraseReveal = false;
        _revealCoroutine = null;

        EventBus._Instance.InvokeTyperwriterFinished();
    }

    private int GetVisibleCharacterCountBeforeSourceIndex(int sourceIndex)
    {
        int count = 0;

        for (int i = 0; i < _text.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo =
                _text.textInfo.characterInfo[i];

            if (characterInfo.index < sourceIndex)
            {
                count++;
            }
        }

        return count;
    }

    private sealed class PhraseRevealRange
    {
        public PhraseTimingData Data;
        public int SourceStart;
        public int SourceEnd;
    }
}
