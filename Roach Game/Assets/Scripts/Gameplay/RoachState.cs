/*
 * File: RoachState.cs
 * Created: 28/05/2026, 11:59:26 AM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
    }

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

    // ------------------------------------------------------------------------
    protected class RoachRunningState : RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        private List<Vector3> _randomPositionGizmos;
        private List<Vector3> _foundPositionGizmos;
        private float _legAnimTime;
        private Vector3[] _legRots;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _roach.ResetAntennae();

            _roach.transform.Rotate(0, Random.Range(0, 350), 0);
            _roach._roachSplines.Rotate(0, Random.Range(0, 350), 0);
            _roach._roachSplines.position = _roach.transform.position;

            _randomPositionGizmos = new List<Vector3>();
            _foundPositionGizmos = new List<Vector3>();

            var knots = _roach._movementSplineContainer.Spline.Knots.ToArray();
            for(int i = 1; i < knots.Length; i++)
            {
                SetKnotPosition(knots, i);
            }

            _roach._movementSplineAnimator.Restart(true);

            _legRots = new Vector3[_roach._legs.Length];
            for(int i = 0; i < _legRots.Length; i++)
            {
                _legRots[i] = _roach._legAnim;
                if(Random.Range(0,2) == 0) _legRots[i] = -_legRots[i];
            }
        }

        // --------------------------------------------------------------------
        private void SetKnotPosition(UnityEngine.Splines.BezierKnot[] knots, int splineIndex)
        {
            Vector3 randomDisplacement = Random.onUnitSphere;

            switch(_roach._movementPlane)
            {
                case MovementPlane.XZ:
                    randomDisplacement.y = 0;
                    randomDisplacement *= _roach._pathKnotDistance;
                    break;
                case MovementPlane.XY:
                    randomDisplacement.z = 0;
                    randomDisplacement *= _roach._pathKnotDistance;
                    break;
                case MovementPlane.YZ:
                    randomDisplacement.x = 0;
                    randomDisplacement *= _roach._pathKnotDistance;
                    break;
            }

            Vector3 prevKnotPos = _roach._movementSplineContainer.transform.TransformPoint((Vector3)knots[splineIndex - 1].Position);
            Vector3 randomPos = prevKnotPos + randomDisplacement;

            _randomPositionGizmos.Add(randomPos);
            
            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(randomPos, out navMeshHit, 2.0f, NavMesh.AllAreas);
            if(navMeshHit.hit)
            {
                var targetKnot = knots[splineIndex];
                targetKnot.Position = _roach._movementSplineContainer.transform.InverseTransformPoint(navMeshHit.position);
                _roach._movementSplineContainer.Spline.SetKnot(splineIndex, targetKnot);  

                _foundPositionGizmos.Add(navMeshHit.position); 
            }
        }

        // --------------------------------------------------------------------
        public override void OnDrawGizmos ()
        {
            if(_randomPositionGizmos == null || _foundPositionGizmos == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            foreach(Vector3 pos in _randomPositionGizmos)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }

            Gizmos.color = Color.green;
            foreach(Vector3 pos in _foundPositionGizmos)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            _legAnimTime += deltaTime;
            if(_legAnimTime >= _roach._legFlipTime)
            {
                _legAnimTime = 0;
                
                for(int i = 0; i < _legRots.Length; i++)
                {
                    _legRots[i] = new Vector3(-_legRots[i].x, _legRots[i].y, _legRots[i].z);
                }
            }
            for(int i = 0; i < _roach._legs.Length; i++)
            {
                _roach._legs[i].Rotate(_legRots[i] * Time.deltaTime);
            }


            if(!_roach._movementSplineAnimator.IsPlaying)
            {
                _roach.EnterState(RoachStateType.Idle);
            }
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._movementSplineAnimator.Pause();
        }
    }

    // ------------------------------------------------------------------------
    protected class RoachStayIdleState : RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        private float _antennaeAnimTime;
        private Vector3 _leftRot;
        private Vector3 _rightRot;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

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
        }
    }

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

    // ------------------------------------------------------------------------
    protected class RoachCinematicState : RoachState
    {
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);
            _roach._gun.SkipFirstReloadAudio();
            roach._activeCinematic.Play();
        }
    }

    // ------------------------------------------------------------------------
    protected class RoachRunOnceState : RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        private List<Vector3> _randomPositionGizmos;
        private List<Vector3> _foundPositionGizmos;
        private float _legAnimTime;
        private Vector3[] _legRots;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            _roach.ResetAntennae();

            _roach.transform.Rotate(0, Random.Range(0, 350), 0);
            _roach._roachSplines.Rotate(0, Random.Range(0, 350), 0);
            _roach._roachSplines.position = _roach.transform.position;

            _randomPositionGizmos = new List<Vector3>();
            _foundPositionGizmos = new List<Vector3>();

            var knots = _roach._movementSplineContainer.Spline.Knots.ToArray();
            for(int i = 1; i < knots.Length; i++)
            {
                SetKnotPosition(knots, i);
            }

            _roach._movementSplineAnimator.Restart(true);

            _legRots = new Vector3[_roach._legs.Length];
            for(int i = 0; i < _legRots.Length; i++)
            {
                _legRots[i] = _roach._legAnim;
                if(Random.Range(0,2) == 0) _legRots[i] = -_legRots[i];
            }
        }

        // --------------------------------------------------------------------
        private void SetKnotPosition(UnityEngine.Splines.BezierKnot[] knots, int splineIndex)
        {
            Vector3 randomDisplacement = Random.onUnitSphere;

            switch(_roach._movementPlane)
            {
                case MovementPlane.XZ:
                    randomDisplacement.y = 0;
                    randomDisplacement *= _roach._pathKnotDistance;
                    break;
                case MovementPlane.XY:
                    randomDisplacement.z = 0;
                    randomDisplacement *= _roach._pathKnotDistance;
                    break;
                case MovementPlane.YZ:
                    randomDisplacement.x = 0;
                    randomDisplacement *= _roach._pathKnotDistance;
                    break;
            }

            Vector3 prevKnotPos = _roach._movementSplineContainer.transform.TransformPoint((Vector3)knots[splineIndex - 1].Position);
            Vector3 randomPos = prevKnotPos + randomDisplacement;

            _randomPositionGizmos.Add(randomPos);
            
            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(randomPos, out navMeshHit, 2.0f, NavMesh.AllAreas);
            if(navMeshHit.hit)
            {
                var targetKnot = knots[splineIndex];
                targetKnot.Position = _roach._movementSplineContainer.transform.InverseTransformPoint(navMeshHit.position);
                _roach._movementSplineContainer.Spline.SetKnot(splineIndex, targetKnot);  

                _foundPositionGizmos.Add(navMeshHit.position); 
            }
        }

        // --------------------------------------------------------------------
        public override void OnDrawGizmos ()
        {
            if(_randomPositionGizmos == null || _foundPositionGizmos == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            foreach(Vector3 pos in _randomPositionGizmos)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }

            Gizmos.color = Color.green;
            foreach(Vector3 pos in _foundPositionGizmos)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }

        // --------------------------------------------------------------------
        public override void RunState(float deltaTime)
        {
            _legAnimTime += deltaTime;
            if(_legAnimTime >= _roach._legFlipTime)
            {
                _legAnimTime = 0;
                
                for(int i = 0; i < _legRots.Length; i++)
                {
                    _legRots[i] = new Vector3(-_legRots[i].x, _legRots[i].y, _legRots[i].z);
                }
            }
            for(int i = 0; i < _roach._legs.Length; i++)
            {
                _roach._legs[i].Rotate(_legRots[i] * Time.deltaTime);
            }


            if(!_roach._movementSplineAnimator.IsPlaying)
            {
                _roach.EnterState(RoachStateType.StayIdle);
            }
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._movementSplineAnimator.Pause();
        }
    }
}