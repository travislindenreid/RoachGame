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
    protected class RoachIdleState : RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        private float _maxStateTime;
        private float _antennaeAnimTime;
        private Vector3 _leftRot;
        private Vector3 _rightRot;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _timeInState = 0;
            _maxStateTime = Random.Range(_roach._idleTimeMinMax.x, _roach._idleTimeMinMax.y);
            _leftRot = Vector3.Lerp(_roach._antennaeAnimMin, _roach._antennaeAnimMax, Random.Range(0.0f, 1.0f));
            _rightRot = Vector3.Lerp(_roach._antennaeAnimMin, _roach._antennaeAnimMax, Random.Range(0.0f, 1.0f));
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            _antennaeAnimTime += Time.deltaTime;
            if(_antennaeAnimTime >= _roach._antennaeFlipTime)
            {
                _antennaeAnimTime = 0;
                _leftRot = new Vector3(-_leftRot.x, _leftRot.y, _leftRot.z);
                _rightRot = new Vector3(-_rightRot.x, _rightRot.y, _rightRot.z);
            }
            _roach._leftAntennae.Rotate(_leftRot * Time.deltaTime);
            _roach._rightAntennae.Rotate(_rightRot * Time.deltaTime);

            if(!_roach._isImmobile)
            {
                _timeInState += Time.deltaTime;
                if(_timeInState >= _maxStateTime)
                {
                    _roach.EnterState(RoachStateType.Running);
                }
            }
        }
    }
}