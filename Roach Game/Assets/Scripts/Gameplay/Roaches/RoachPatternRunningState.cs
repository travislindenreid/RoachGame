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
    protected class RoachPatternRunningState : RoachState
    {
        // --------------------------------------------------------------------
        // Variables
        // --------------------------------------------------------------------
        private List<Vector3> _desiredPositionGizmos;
        private List<Vector3> _foundPositionGizmos;
        private float _legAnimTime;
        private Vector3[] _legRots;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);

            // makes roaches invulnerable while running
            _roach._collider.enabled = false;

            _roach.ResetAntennae();

            _roach.transform.Rotate(0, Random.Range(0, 350), 0);
            _roach._roachSplines.Rotate(0, Random.Range(0, 350), 0);
            _roach._roachSplines.position = _roach.transform.position;

            _desiredPositionGizmos = new List<Vector3>();
            _foundPositionGizmos = new List<Vector3>();

            var knots = _roach._hostileMovementSplineContainer.Spline.Knots.ToArray();

            if(_roach._gunLevel == 1)
            {
                SetupLvl1Movement(knots);
            }
            else
            {
                SetupLevel2Movement(knots);
            }

            _roach._hostileMovementSplineAnimate.Restart(true);

            _legRots = new Vector3[_roach._legs.Length];
            for(int i = 0; i < _legRots.Length; i++)
            {
                _legRots[i] = _roach._legAnim;
                if(Random.Range(0,2) == 0) _legRots[i] = -_legRots[i];
            }
        }

        // --------------------------------------------------------------------
        private void SetupLvl1Movement (UnityEngine.Splines.BezierKnot[] knots)
        {
            if(_roach._lvl1Positions == null)
            {
                _roach._lvl1Positions = new Vector3[2];
                _roach._lvl1Positions[0] = _roach.transform.position;

                Vector3 movement = Vector3.zero;
                switch(_roach._movementPlane)
                {
                    case MovementPlane.XZ:
                    case MovementPlane.XY:
                        movement.x = _roach._hostileMovementPathDistanceLvl1;
                        break;
                    case MovementPlane.YZ:
                        movement.z = _roach._hostileMovementPathDistanceLvl1;
                        break;
                }
                
                Vector3 tryPosition = _roach.transform.position + _roach.transform.TransformDirection(movement);
                _roach._lvl1Positions[1] = GetLocationOnNavMesh(tryPosition);
            }

            if(_roach._moveAlt)
            {
                SetKnotPosition(knots, 0, _roach._lvl1Positions[1]);
                SetKnotPosition(knots, 1, _roach._lvl1Positions[0]);
            }
            else
            {
                SetKnotPosition(knots, 0, _roach._lvl1Positions[0]);
                SetKnotPosition(knots, 1, _roach._lvl1Positions[1]);
            }
        }

        // --------------------------------------------------------------------
        private void SetupLevel2Movement (UnityEngine.Splines.BezierKnot[] knots)
        {
            if(_roach._lvl2Positions == null)
            {
                _roach._lvl2Positions = new Vector3[3];
                _roach._lvl2Positions[0] = _roach.transform.position;

                Vector3 movement = Vector3.zero;
                switch(_roach._movementPlane)
                {
                    case MovementPlane.XZ:
                    case MovementPlane.XY:
                        movement.x = _roach._hostileMovementPathDistanceLvl1;
                        break;
                    case MovementPlane.YZ:
                        movement.z = _roach._hostileMovementPathDistanceLvl1;
                        break;
                }
                
                Vector3 tryPosition = _roach.transform.position + _roach.transform.TransformDirection(movement);
                _roach._lvl2Positions[1] = GetLocationOnNavMesh(tryPosition);

                tryPosition = _roach.transform.position + _roach.transform.TransformDirection(-movement);
                _roach._lvl2Positions[2] = GetLocationOnNavMesh(tryPosition);
            }

            // positions: 0 = center, 1 = left, 2 = right
            if(_roach._moveState == 0)
            {
                SetKnotPosition(knots, 0, _roach._lvl2Positions[0]);
                SetKnotPosition(knots, 1, _roach._lvl2Positions[1]);
            }
            else if(_roach._moveState == 1)
            {
                SetKnotPosition(knots, 0, _roach._lvl2Positions[1]);
                SetKnotPosition(knots, 1, _roach._lvl2Positions[0]);
            }
            else if(_roach._moveState == 2)
            {
                SetKnotPosition(knots, 0, _roach._lvl2Positions[0]);
                SetKnotPosition(knots, 1, _roach._lvl2Positions[2]);
            }
            else if(_roach._moveState == 3)
            {
                SetKnotPosition(knots, 0, _roach._lvl2Positions[2]);
                SetKnotPosition(knots, 1, _roach._lvl2Positions[0]);
            }
        }

        // --------------------------------------------------------------------
        private void SetKnotPosition(
            UnityEngine.Splines.BezierKnot[] knots,
            int splineIndex,
            Vector3 worldPosition
        ) {
            var targetKnot = knots[splineIndex];
            targetKnot.Position = _roach._hostileMovementSplineContainer.transform.InverseTransformPoint(worldPosition);
            _roach._hostileMovementSplineContainer.Spline.SetKnot(splineIndex, targetKnot);  
        }

        // --------------------------------------------------------------------
        private Vector3 GetLocationOnNavMesh (Vector3 tryPosition)
        {
            _desiredPositionGizmos.Add(tryPosition);

            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(tryPosition, out navMeshHit, 5.0f, NavMesh.AllAreas);
            if(navMeshHit.hit)
            {
                _foundPositionGizmos.Add(navMeshHit.position);
                return navMeshHit.position;
            }
            return tryPosition;
        }

        // --------------------------------------------------------------------
        public override void OnDrawGizmos ()
        {
            if(_desiredPositionGizmos == null || _foundPositionGizmos == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            foreach(Vector3 pos in _desiredPositionGizmos)
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


            if(!_roach._hostileMovementSplineAnimate.IsPlaying)
            {
                _roach.EnterState(RoachStateType.Attacking);
            }
        }

        // --------------------------------------------------------------------
        public override void ExitState()
        {
            _roach._moveAlt = !_roach._moveAlt;
            _roach._moveState ++;
            if(_roach._moveState > 3)
            {
                _roach._moveState = 0;
            }

            _roach._collider.enabled = true;
            _roach._hostileMovementSplineAnimate.Pause();
        }
    }
}