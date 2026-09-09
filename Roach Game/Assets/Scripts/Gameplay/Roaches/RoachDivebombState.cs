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
    protected class RoachDivebombState : RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        private bool _isDiving;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _roach.transform.rotation = Quaternion.identity;

            _timeInState = 0.0f;
            _isDiving = false;

            _roach._hostile = true;
            _roach._agent.enabled = false;

            SetupAntennaeAnimation();
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            RunAntennaeAnimation();

            if(_isDiving)
            {
                if(!_roach._divebombSplineAnimator.IsPlaying)
                {
                    _roach.EnterState(RoachStateType.Attacking);
                }
            }
            else
            {
                _timeInState += Time.deltaTime;
                if(_timeInState >= _roach._timeToDivebomb)
                {
                    _roach._divebombSplineAnimator.Restart(true);
                    _isDiving = true;

                    _roach._divebombSound.time = 0.0f;
                    _roach._divebombSound.Play();
                }
            }
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._agent.enabled = true;
            _roach._divebombSplineAnimator.Pause();
        }
    }
}