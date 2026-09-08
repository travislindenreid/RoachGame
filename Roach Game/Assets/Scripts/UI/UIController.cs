/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI/UIController.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI
 * Created Date: Wednesday, July 1st 2026, 5:03:44 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

public class UIController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private GameObject _gameOverScreen;
    [SerializeField] private GameObject _pauseMenu;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Start()
    {
        EventBus._Instance.PlayerDied += HandlePlayerDied;
    }

    // ------------------------------------------------------------------------
    private void OnDisable()
    {
        EventBus._Instance.PlayerDied -= HandlePlayerDied;
    }

    // ------------------------------------------------------------------------
    private void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && GameController._Instance._GameStarted)
        {
            _pauseMenu.SetActive(!_pauseMenu.activeSelf);
        }
    }

    // ------------------------------------------------------------------------
    private void HandlePlayerDied ()
    {
        _gameOverScreen.SetActive(true);
    }
}
