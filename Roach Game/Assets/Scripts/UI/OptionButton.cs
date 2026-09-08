/*
 * File: OptionButton.cs
 * Created: 26/05/2026, 12:29:04 AM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private Button _button;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    public void SetupButton (DialogueOption option)
    {
        _buttonText.text = option._OptionText;

        _button.onClick.AddListener(delegate
        {
            GetComponent<ButtonClickAudio>().PlayClickAudio();
           DialogueRunner._Instance.SelectOption(option._NextNode); 
        });
    }
}
