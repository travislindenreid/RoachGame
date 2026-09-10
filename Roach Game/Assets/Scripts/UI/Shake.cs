/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI/Shake.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/UI
 * Created Date: Wednesday, September 9th 2026, 10:13:27 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

public class Shake : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Vector3 _displacement = Vector3.up;
    [SerializeField] private float _flipTime = 1.0f;

    private float _time;
    private bool _flip;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Update()
    {
        _time += Time.deltaTime;
        if (_time >= _flipTime)
        {
            _flip = !_flip;
            _time = 0.0f;
        }

        float mult = _flip ? -1 : 1;
        transform.position += mult * _displacement * Time.deltaTime;
    }
}