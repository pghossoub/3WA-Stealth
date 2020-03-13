using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [HideInInspector]
    public bool m_playerIsDead = false;

    //[SerializeField]
    //private float _behaviorTime;
    //[SerializeField]
    //private float _speed;
    //[SerializeField]
    //private float _speedRotation;

    [SerializeField]
    private float _speedPatrolling = 1;
    [SerializeField]
    private float _speedSearching = 1.5f;
    [SerializeField]
    private float _speedChasing = 3;
    [SerializeField]
    private Transform _trSight;
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private Material[] jointMaterials;
    [SerializeField]
    private SkinnedMeshRenderer jointsRenderer;

    protected NavMeshAgent _agent;
    protected Animator _anim;

    private Light _spotlight;
    private Transform _tr;
    //private string _detected = "Invisible";
    private bool _playerInSight = false;
    private Transform _trPlayer;
    protected State _state = State.PATROLLING;
    private bool _blindChasing = false;
    private Vector3 _target;

    //private Vector3 _directionXZ;
    //private Rigidbody _rb;
    //private bool _isRotating = false;
    //private Vector3 _lookAt;

    public enum State
    {
        PATROLLING,
        CHASING,
        SEARCHING
    }

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        //_rb = GetComponent<Rigidbody>();
        _tr = GetComponent<Transform>();
        _spotlight = GetComponentInChildren<Light>();
        _agent = GetComponent<NavMeshAgent>();
    }

    protected abstract void Start();

    protected void GoToWaypoint(Transform waypoint)
    {
        _agent.SetDestination(waypoint.position);
    }

    private void Update()
    {
        Vision();

        switch (_state)
        {
            case (State.PATROLLING):
                jointsRenderer.material = jointMaterials[0];
                SetAgentParameters(_speedPatrolling, 120, 0.01f);

                PatrolBehavior();
                break;

            case (State.CHASING):
                _agent.isStopped = false;
                jointsRenderer.material = jointMaterials[2];
                SetAgentParameters(_speedChasing, 180, _attackRange);

                FollowPlayer();
                CheckIfPlayerIsLost();
                Attack();
                CheckIfPlayerIsDead();
                break;

            case (State.SEARCHING):
                _agent.isStopped = false;
                jointsRenderer.material = jointMaterials[1];
                SetAgentParameters(_speedSearching, 120, 0.01f);

                SearchPlayer();
                break;
        }
    }

    private void SearchPlayer()
    {
        _anim.SetFloat(Animator.StringToHash("VelocityX"), 0.5f);
        _agent.SetDestination(_target);
        if (_agent.remainingDistance < 0.1f)
            _state = State.PATROLLING;
    }

    private void SetAgentParameters(float speed, float rotateSpeed, float stopDistance)
    {
        _agent.speed = speed;
        _agent.angularSpeed = rotateSpeed;
        _agent.stoppingDistance = stopDistance;
    }

    public void HeardSomething(Transform target)
    {
        if (_state != State.CHASING)
        {
            _state = State.SEARCHING;
            _target = target.position;
        }
    }

    private void CheckIfPlayerIsDead()
    {
        if (m_playerIsDead)
            _state = State.PATROLLING;
    }
    private void Attack()
    {
        if (Vector3.Distance(_trPlayer.position, _tr.position) < _attackRange && !m_playerIsDead)
        {
            _anim.SetTrigger(Animator.StringToHash("Strike"));
        }
    }

    private void CheckIfPlayerIsLost()
    {
        if (!_playerInSight && !_blindChasing)
        {
            _blindChasing = true;
            StartCoroutine(PlayerLostCoroutine());
        }
    }

    IEnumerator PlayerLostCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        if (!_playerInSight)
            _state = State.PATROLLING;
        _blindChasing = false;
    }

    private void FollowPlayer()
    {
        GoToWaypoint(_trPlayer);
        _agent.speed = _speedChasing;
        _anim.SetFloat(Animator.StringToHash("VelocityX"), 1f);
    }

    protected abstract void PatrolBehavior();
    //{
        //if (_agent.remainingDistance < 0.1f)

        //{
        //    if (_currentWaypoint < _waypoints.Length - 1)
        //        _currentWaypoint++;
        //    else
        //        _currentWaypoint = 0;

        //    GoToWaypoint(_waypoints[_currentWaypoint]);
        //}
        //_anim.SetFloat(Animator.StringToHash("VelocityX"), 0.4f);
    //}

    private void Vision()
    {
        RaycastHit hit;
        float spotAngle = _spotlight.spotAngle;
        for (float angle = -spotAngle; angle < spotAngle; angle += 10)
        {
            Vector3 rayDirection = Quaternion.AngleAxis(angle, Vector3.up) * _trSight.forward;
            Physics.Raycast(_trSight.position, rayDirection, out hit);
            Debug.DrawLine(_trSight.position, _trSight.position + rayDirection * 3);

            if (hit.collider.CompareTag("Player") && !m_playerIsDead)
            {
                _trPlayer = hit.collider.transform;
                _playerInSight = true;
                _state = State.CHASING;
                //_detected = "Detected";
                break;
            }
            else
            {
                //_detected = "Invisible";
                _playerInSight = false;
            }

        }
    }

    private void FixedUpdate()
    {
        /*
        _rb.velocity = _directionXZ * _speed * Time.fixedDeltaTime;

        if (_isRotating)
        {
            float step = _speedRotation * Time.fixedDeltaTime;

            Quaternion rotateTo = Quaternion.LookRotation(_lookAt);

            _tr.rotation = Quaternion.RotateTowards(_tr.rotation, rotateTo, step);
        }
        */
    }

    //IEnumerator MovePatternCoroutine()
    //{
    //    while (gameObject)
    //    {
    //        MoveForward();
    //        yield return new WaitForSeconds(_behaviorTime);

    //        Stop();
    //        yield return new WaitForSeconds(_behaviorTime);


    //        _isRotating = true;
    //        _lookAt = -_tr.forward;
    //        yield return new WaitForSeconds(_behaviorTime);
    //        _isRotating = false;

    //    }
    //}

    //private void Stop()
    //{
    //    _directionXZ = Vector3.zero;
    //    _anim.SetFloat(Animator.StringToHash("VelocityX"), 0f);
    //}

    //private void MoveForward()
    //{
    //    _anim.SetFloat(Animator.StringToHash("VelocityX"), 0.4f);
    //    _directionXZ = _tr.forward;
    //}

    private void OnDrawGizmos()
    {
        //Gizmos.DrawLine(transform.position, transform.position + transform.forward * _speed * _behaviorTime * Time.fixedDeltaTime);
        //Gizmos.DrawWireSphere(_trHead.position + _visionDirection, _visionSphereRadius);
        //Gizmos.DrawLine(_trHead.position, _trHead.position + _visionDirection);
    }

    void OnGUI()
    {
        //GUI.Button(new Rect(10, 90, 120, 30), $"{_detected}");
        //GUI.Button(new Rect(10, 130, 120, 30), $"State: {_state}");
    }
}
