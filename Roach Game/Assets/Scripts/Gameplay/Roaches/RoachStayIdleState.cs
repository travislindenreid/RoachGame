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
    protected class RoachStayIdleState : RoachState
    {
        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _roach._agent.enabled = false;

            SetupAntennaeAnimation();
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            RunAntennaeAnimation();
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._agent.enabled = true;
        }
    }
}