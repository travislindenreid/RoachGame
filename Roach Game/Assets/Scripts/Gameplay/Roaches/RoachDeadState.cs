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
    protected class RoachDeadState : RoachState
    {
        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _roach.ResetAntennae();

            _roach._agent.enabled = false;

            _roach._collectUI.SetActive(true);

            _roach._roachSplines.position = _roach.transform.position;
            _roach._deathSplineAnimator.Play();
        }

        // --------------------------------------------------------------------
        public override void OnMouseOver ()
        {
            _roach._collectUI.SetActive(true);

            if(Input.GetKeyDown(KeyCode.E))
            {
                _roach.EnterState(RoachStateType.Collected);
            }
        }

        // --------------------------------------------------------------------
        public override void OnMouseExit ()
        {
            _roach._collectUI.SetActive(false);
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._collectUI.SetActive(false);
        }
    }
}