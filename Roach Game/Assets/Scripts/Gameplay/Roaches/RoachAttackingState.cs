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
        private float _timeBetweenUse;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);
            _roach.ShowGun();
            _timeBetweenUse = 0.0f;
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._gun.gameObject.SetActive(false);
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            _roach._gun.PointAtPlayer();

            _timeBetweenUse += deltaTime;
            if(_timeBetweenUse >= _roach._weaponUseInterval)
            {
                _roach._gun.Use();
                _timeBetweenUse = 0.0f;
            }
        }
    }
}