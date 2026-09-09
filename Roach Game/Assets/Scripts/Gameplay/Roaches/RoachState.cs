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
    protected class RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        protected Roach _roach;
        protected float _timeInState;

        // antennae animation
        protected float _antennaeAnimTime;
        protected Vector3 _leftRot;
        protected Vector3 _rightRot;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public virtual void EnterState(Roach roach)
        {
            _roach = roach;
        }

        // --------------------------------------------------------------------
        public virtual void ExitState() {}
        // --------------------------------------------------------------------
        public virtual void RunState(float deltaTime) {}
        // --------------------------------------------------------------------
        public virtual void OnMouseOver () {}
        // --------------------------------------------------------------------
        public virtual void OnMouseExit () {}
        // --------------------------------------------------------------------
        public virtual void OnDrawGizmos () {}

        // --------------------------------------------------------------------
        protected void SetupAntennaeAnimation ()
        {
            _roach.ResetAntennae();
            _leftRot = Vector3.Lerp(_roach._antennaeAnimMin, _roach._antennaeAnimMax, Random.Range(0.0f, 1.0f));
            _rightRot = Vector3.Lerp(_roach._antennaeAnimMin, _roach._antennaeAnimMax, Random.Range(0.0f, 1.0f));
        }

        // --------------------------------------------------------------------
        protected void RunAntennaeAnimation ()
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
        }
    }
}