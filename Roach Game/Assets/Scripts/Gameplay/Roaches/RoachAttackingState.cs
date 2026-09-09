/*
 * File: RoachState.cs
 * Created: 28/05/2026, 11:59:26 AM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

public partial class Roach
{
    // ------------------------------------------------------------------------
    // Types
    // ------------------------------------------------------------------------
    protected class RoachAttackingState : RoachState
    {
        // --------------------------------------------------------------------
        // Variable
        // --------------------------------------------------------------------
        private float _timeBetweenSubstates;
        private bool _usedWeapon;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);
            _roach.ShowGun();
            _timeBetweenSubstates = 0.0f;
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach.HideGun();
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            _roach._gun.PointAtPlayer();
            _timeBetweenSubstates += deltaTime;

            if(_usedWeapon && _timeBetweenSubstates >= _roach._timeAfterWeaponUse)
            {
                _roach.EnterState(RoachStateType.PatternRunning);
            }
            else if(_timeBetweenSubstates >= _roach._timeBeforeWeaponUse)
            {
                _roach._gun.Use();
                _usedWeapon = true;
                _timeBetweenSubstates = 0.0f;
            }
        }
    }
}