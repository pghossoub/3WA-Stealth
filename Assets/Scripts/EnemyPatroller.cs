using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatroller : Enemy
{
    [SerializeField]
    protected Transform[] _waypoints;

    protected int _currentWaypoint = 0;

    protected override void Start()
    {
        GoToWaypoint(_waypoints[0]);
    }
    protected override void PatrolBehavior()
    {
        if (_agent.remainingDistance < 0.1f)

        {
            if (_currentWaypoint < _waypoints.Length - 1)
                _currentWaypoint++;
            else
                _currentWaypoint = 0;

            GoToWaypoint(_waypoints[_currentWaypoint]);
        }
        _anim.SetFloat(Animator.StringToHash("VelocityX"), 0.4f);
    }
}
