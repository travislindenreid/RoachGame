/*
 * File: UiController.cs
 * Created: 28/05/2026, 2:02:19 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject _centerCursor;
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _healthDisplay;
    [SerializeField] private LayoutGroup _healthSegmentParent;
    [SerializeField] private HealthSegmentUI _healthSegmentPrefab;
    [SerializeField] private TMP_Text _roachesText;

    private HealthSegmentUI[] _healthSegments;
    private int _maxHealth;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Start ()
    {
        EventBus._Instance.PlayerHealthChanged += HandlePlayerHealthChanged;
        EventBus._Instance.SequenceStarted += HandleSequenceStarted;
        EventBus._Instance.EnemyHit += HandleEnemyHit;

        _maxHealth = Mathf.CeilToInt(Player._Instance._MaxHealth);
        _healthSegments = new HealthSegmentUI[_maxHealth];

        for(int i = 0; i < _maxHealth; i++)
        {
            _healthSegments[i] = Instantiate(_healthSegmentPrefab, _healthSegmentParent.transform);
            _healthSegments[i].Setup();
        }

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
                ResetHealthSegments();
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
    private void ResetHealthSegments ()
    {
        if(_healthSegments != null)
        {
            for(int i = 0; i < _healthSegments.Length; i++)
            {
                _healthSegments[i].Setup();
            }
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
        if(_healthSegments == null) return;

        float health = Player._Instance._Health;
        // segment 1 = far left (lowest health)
        // segment [maxHealth] = far right (highest health)
        int lastActiveSegmentIndex = Mathf.CeilToInt(health);

        int quant = Mathf.FloorToInt(health);
        float t = health - quant;
        Debug.LogFormat("health: {0}; quant: {1}; t: {2}", health, quant, t);

        if(lastActiveSegmentIndex > 0)
        {
            _healthSegments[lastActiveSegmentIndex - 1].SetColor(t);
        }

        for(int i = lastActiveSegmentIndex; i < _healthSegments.Length; i++)
        {
            _healthSegments[i].Hide();
        }   
    }
}
