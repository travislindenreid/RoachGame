/*
 * File: UiController.cs
 * Created: 28/05/2026, 2:02:19 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Linq;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject _centerCursor;
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _healthDisplay;
    [SerializeField] private RectTransform _healthBar;
    [SerializeField] private TMP_Text _roachesText;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Start ()
    {
        EventBus._Instance.PlayerHealthChanged += HandlePlayerHealthChanged;
        EventBus._Instance.SequenceStarted += HandleSequenceStarted;
        EventBus._Instance.EnemyHit += HandleEnemyHit;

        HandlePlayerHealthChanged();
    }

    // ------------------------------------------------------------------------
    private void OnDisable()
    {
        EventBus._Instance.PlayerHealthChanged -= HandlePlayerHealthChanged;
        EventBus._Instance.SequenceStarted -= HandleSequenceStarted;
        EventBus._Instance.EnemyHit -= HandleEnemyHit;
    } 

    // ------------------------------------------------------------------------
    private void HandleSequenceStarted(Sequence sequence)
    {
        switch(sequence._GameStateType)
        {
            case GameStateType.Action:
                OpenHud(sequence);
                _centerCursor.SetActive(true);
                break;
            case GameStateType.Cinematic:
            case GameStateType.Dialogue:
            case GameStateType.Menu:
                if(_hud != null) _hud.SetActive(false);
                if(_centerCursor != null) _centerCursor.SetActive(false);
                break;
        }
    }

    // ------------------------------------------------------------------------
    private void OpenHud (Sequence sequence)
    {
        _hud.SetActive(true);
        _roachesText.text = sequence._Roaches.Count(r => !r._IsDead).ToString();

        _healthDisplay.SetActive(GameController._Instance._ReadyForHealthDisplay);
    }

    // ------------------------------------------------------------------------
    private void HandleEnemyHit (Attackable roach)
    {
        _roachesText.text = GameController._Instance._LivingRoachCount.ToString();
    }

    // ------------------------------------------------------------------------
    private void HandlePlayerHealthChanged ()
    {
        float health = Player._Instance._HealthPercent;
        _healthBar.anchorMax = new Vector2(
            health,
            _healthBar.anchorMax.y
        );
        _healthBar.offsetMin = Vector2.zero;
        _healthBar.offsetMax = Vector2.zero;
    }
}
