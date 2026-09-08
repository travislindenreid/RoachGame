/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI/PauseMenu.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI
 * Created Date: Monday, September 7th 2026, 4:39:17 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;
using UnityEngine.UI;

public class PauseWindow : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private GameObject _confirmClosePopup;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    // ui callback
    public void HandleVolumeSliderChange ()
    {
        AudioController._Instance.ChangeVolume(_volumeSlider.value);
    }

    // ------------------------------------------------------------------------
    // ui callback
    public void HandleSensitivitySliderChange ()
    {
        Player._Instance.SetPlayerMouseSensitivity(_sensitivitySlider.value);
    }

    // ------------------------------------------------------------------------
    // button callback
    public void TryExitButton ()
    {
        _confirmClosePopup.SetActive(true);
    }

    // ------------------------------------------------------------------------
    // button callback
    public void ConfirmExitButton ()
    {
        Application.Quit();
    }

    // ------------------------------------------------------------------------
    // button callback
    public void ReturnToGameButton ()
    {
        _confirmClosePopup.SetActive(false);
    }
}