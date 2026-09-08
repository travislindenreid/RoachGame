/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Audio/ButtonClickAudio.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Audio
 * Created Date: Monday, September 7th 2026, 6:41:27 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

public class ButtonClickAudio : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void PlayClickAudio ()
    {
        AudioController._Instance.PlayButtonClick();
    }
}