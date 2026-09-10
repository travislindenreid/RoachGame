/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Gameplay/Attackable.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Gameplay
 * Created Date: Wednesday, September 9th 2026, 3:38:37 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using System.Text;
using TMPro;
using UnityEngine;

public class Attackable : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [Header("Attackable")]
    [SerializeField] protected float _maxHealth = 1;
    [SerializeField] protected GameObject _healthCanvas;
    [SerializeField] protected TMP_Text _healthText;
    [SerializeField] protected bool _healthVisibleOnActionBegin = false;

    protected float _health;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public bool _IsDead => _health <= 0;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    protected virtual void Start()
    {
        _health = _maxHealth;
        _healthCanvas.SetActive(false);

        EventBus._Instance.SequenceStarted += HandleSequenceStarted;
    } 

    // ------------------------------------------------------------------------
    protected virtual void OnDisable ()
    {
        EventBus._Instance.SequenceStarted -= HandleSequenceStarted;
    }

    // ------------------------------------------------------------------------
    public virtual void Hit ()
    {
        _health--;
        EventBus._Instance.InvokeAttackableHit(this);
        UpdateHealthText();
    }

    // ------------------------------------------------------------------------
    protected virtual void HandleSequenceStarted(Sequence sequence)
    {
        if(sequence._GameStateType == GameStateType.Action)
        {
            UpdateHealthText();
        }
    }

    // ------------------------------------------------------------------------
    public virtual void ResetAttackable ()
    {
        _health = _maxHealth;
        UpdateHealthText();
    }

    // ------------------------------------------------------------------------
    protected void UpdateHealthText ()
    {
        bool show = _healthVisibleOnActionBegin || (_health > 0 && _health != _maxHealth);
        _healthCanvas.SetActive(show);

        if(show)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < _health; i++)
            {
                sb.Append(".");
            }
            _healthText.text = sb.ToString();
        }
    }
}