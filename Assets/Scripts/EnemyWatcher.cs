using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWatcher : Enemy
{
    [SerializeField]
    private Transform _spot;
    [SerializeField]
    private float _speedRotation;

    private Quaternion _rotateAngle;

    private string _patrolState = "Idle";
    private Transform _tr;

    protected override void Start()
    {
        _tr = GetComponent<Transform>();
        _rotateAngle = transform.rotation;
        GoToWaypoint(_spot);
    }

    protected void FixedUpdate()
    {
        if (_patrolState == "Idle")
        {
            float step = _speedRotation * Time.fixedDeltaTime;

            _tr.rotation = Quaternion.RotateTowards(_tr.rotation, _rotateAngle, step);
        }
    }
    protected override void PatrolBehavior()
    {
        if(_agent.remainingDistance > 0.1f)
        {
            _agent.isStopped = false;
            GoToWaypoint(_spot);
            _anim.SetFloat(Animator.StringToHash("VelocityX"), 0.4f);
            _patrolState = "Repositionning";
        }
        else
        {
            _agent.isStopped = true;
            GoToWaypoint(_spot);
            _anim.SetFloat(Animator.StringToHash("VelocityX"), 0);
            _patrolState = "Idle";
        }
    }

    private void OnGUI()
    {
        GUI.Button(new Rect(10, 130, 120, 30), $"State: {_patrolState}");
    }
}
