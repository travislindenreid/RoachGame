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
    protected class RoachHostileRunningState : RoachState
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
                SetKnotPositionLvl1(knots, 0, _roach._lvl1Positions[1]);
                SetKnotPositionLvl1(knots, 1, _roach._lvl1Positions[0]);
            }
            else
            {
                SetKnotPositionLvl1(knots, 0, _roach._lvl1Positions[0]);
                SetKnotPositionLvl1(knots, 1, _roach._lvl1Positions[1]);
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
        private void SetKnotPositionLvl1(
            UnityEngine.Splines.BezierKnot[] knots,
            int splineIndex,
            Vector3 worldPosition
        ) {
            _desiredPositionGizmos.Add(worldPosition);

            var targetKnot = knots[splineIndex];
            targetKnot.Position = _roach._hostileMovementSplineContainer.transform.InverseTransformPoint(worldPosition);
            _roach._hostileMovementSplineContainer.Spline.SetKnot(splineIndex, targetKnot);  
        }

        // --------------------------------------------------------------------
        private Vector3 GetLocationOnNavMesh (Vector3 tryPosition)
        {
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
            _roach._collider.enabled = true;
            _roach._hostileMovementSplineAnimate.Pause();
        }
    }
}