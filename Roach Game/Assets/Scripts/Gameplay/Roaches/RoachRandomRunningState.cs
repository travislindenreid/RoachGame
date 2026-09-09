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
    protected class RoachRandomRunningState : RoachState
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

            // makes roaches invulnerable while running
            _roach._collider.enabled = false;

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
            _roach._collider.enabled = true;
            _roach._movementSplineAnimator.Pause();
        }
    }
}