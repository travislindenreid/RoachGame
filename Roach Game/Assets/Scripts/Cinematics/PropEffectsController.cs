/*
 * Filename: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Cinematics/PropEffectsController.cs
 * Path: /Users/lindenreid/Documents/GitHub/RoachGame/Roach Game/Assets/Scripts/Cinematics
 * Created Date: Tuesday, August 4th 2026, 5:53:42 pm
 * Author: Travis Reid
 * 
 * Copyright (c) 2026 Studio Tilia
 */

using UnityEngine;

public class PropEffectsController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [SerializeField] private Renderer _safeRenderer;
    [SerializeField] private Renderer _aptNoMoldRenderer;
    [SerializeField] private Renderer _apartmentWithSafeMoldRenderer;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public static PropEffectsController _Instance { get; private set; }

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this);
            return;
        }

        _Instance = this;
    }

    // ------------------------------------------------------------------------
    // timeline callback
    public void StartSafeAnimation ()
    {
        // TODO
    }

    // ------------------------------------------------------------------------
    public void StartApartmentDoorDissolve (Renderer[] aptRenderers)
    {
        foreach(Renderer renderer in aptRenderers)
        {
            StartDissolve(renderer);
        }
    }

    // ------------------------------------------------------------------------
    public void StartDissolve (Renderer renderer)
    {
        Material[] mats = renderer.materials;
        foreach(Material mat in mats)
        {
            mat.SetFloat("_DissolveStartTime", Time.time);
            mat.SetInt("_DoDissolve", 1);
        }
    }
}