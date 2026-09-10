/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Gameplay/Attackable.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Gameplay
 * Created Date: Wednesday, September 9th 2026, 3:38:37 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

public class Attackable : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [Header("Attackable")]
    [SerializeField] protected float _maxHealth = 1;

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
    } 

    // ------------------------------------------------------------------------
    public virtual void Hit ()
    {
        _health--;
        EventBus._Instance.InvokeAttackableHit(this);
    }
}