/*
 * File: RoachState.cs
 * Created: 28/05/2026, 11:59:26 AM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;

public partial class Roach
{
    // ------------------------------------------------------------------------
    // Types
    // ------------------------------------------------------------------------
    protected class RoachCollectedState : RoachState
    {
        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _roach._agent.enabled = false;

            _roach._movementSplineAnimator.enabled = false;
            _roach._deathSplineAnimator.enabled = false;

            foreach(MeshRenderer renderer in _roach._renderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            _roach._collider.enabled = false;
            EventBus._Instance.InvokeRoachCollected(_roach);
        }
    }
}