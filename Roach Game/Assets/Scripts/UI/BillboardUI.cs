/*
 * File: BillboardUI.cs
 * Created: 03/06/2026, 3:43:57 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    private Camera _mainCamera;
    private Vector3 _originalPos;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Start()
    {
        _mainCamera = Camera.main;
        _originalPos = transform.localPosition;
    }

    // ------------------------------------------------------------------------
    private void LateUpdate()
    {
        transform.LookAt(_mainCamera.transform);
        transform.Rotate(0, 180, 0);

        transform.localPosition = _originalPos;
    }
}