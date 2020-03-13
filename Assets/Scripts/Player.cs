using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum State
    {
        STANDING,
        JUMPING,
        RUNNING,
        CROUCHING,
        AIMING,
        DEAD
    }

    private Transform _tr;
    private Rigidbody _rb;

    [SerializeField]
    float _speed;
    [SerializeField]
    float _runSpeedModif;
    [SerializeField]
    float _crouchSpeedModif;
    [SerializeField]
    float _speedRotation;
    [SerializeField]
    float _jumpForce;
    [SerializeField]
    float _distanceTocheckGround;

    [SerializeField]
    Camera _cameraMain;
    [SerializeField]
    LayerMask _solidLayer;
    [SerializeField]
    LayerMask _stairLayer;


    private State _state;
    private float _currentSpeed;
    private Vector3 _moveX;
    private Vector3 _moveZ;
    private Animator _anim;
    private Vector3 _startPos;
    [SerializeField]
    private GameObject _colliderNoise;

    [SerializeField]
    private float _raycastOffsetY;
    [SerializeField]
    private float _raycastOffset;
    [SerializeField]
    private float _distanceToCheckStairs;

    private Vector3 _CombinedRaycast;
    [SerializeField]
    private float _floorOffsetY;

    private void Awake()
    {
        _tr = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        _startPos = _tr.position;
    }

    private void Start()
    {
        _state = State.STANDING;
    }

    void Update()
    {

        switch (_state)
        {
            case (State.STANDING):

                _anim.SetBool(Animator.StringToHash("IsAiming"), false);
                _anim.SetBool(Animator.StringToHash("IsLanding"), false);
                _anim.SetBool(Animator.StringToHash("IsFalling"), false);

                if (Aim())
                {
                    _state = State.AIMING;
                    break;
                }
                CheckFalling();
                RunOrCrouch();
                Move();
                Jump();
                //_anim.ResetTrigger(Animator.StringToHash("Land"));

                break;

            case (State.JUMPING):
                Move();
                CheckGround();
                break;

            case (State.RUNNING):
                CheckFalling();
                Move();
                if (!Run())
                    _state = State.STANDING;
                Jump();

                break;

            case (State.CROUCHING):
                CheckFalling();
                Move();
                if (!Crouch())
                    _state = State.STANDING;

                break;

            case (State.AIMING):
                CheckFalling();
                Move();
                if (!Aim())
                    _state = State.STANDING;

                break;

            case (State.DEAD):
                break;
        }
        //Debug.DrawRay(_tr.position, Vector2.down * _distanceTocheckGround, Color.red);

        //Debug
        /*
        if (Input.GetButtonDown("Fire2"))
        {
            _state = State.STANDING;
            Debug.Log(_state);
        }
        */
    }

    public void IsHit()
    {
        _anim.SetBool(Animator.StringToHash("IsDead"), true);
        _state = State.DEAD;
    }
    private bool Aim()
    {
        if (Input.GetAxis("Aim") >= 0.1f)
        {
            //_state = State.AIMING;
            _anim.SetBool(Animator.StringToHash("IsAiming"), true);
            return true;
        }
        return false;
    }

    private void RunOrCrouch()
    {
        if (Run())
            _state = State.RUNNING;
        else if (Crouch())
            _state = State.CROUCHING;
        /*else
            _state = State.STANDING;*/

    }

    private bool Run()
    {
        if (Input.GetAxis("Run") >= 0.1f && Input.GetAxis("Vertical") >= 0.8)
        {
            //_state = State.RUNNING;
            _anim.SetBool(Animator.StringToHash("IsRunning"), true);
            _currentSpeed = _speed + _runSpeedModif;
            return true;
        }
        else
        {
            //_state = State.STANDING;
            _anim.SetBool(Animator.StringToHash("IsRunning"), false);
            _currentSpeed = _speed;
            return false;
        }
    }

    private bool Crouch()
    {
        if (Input.GetButton("Crouch"))
        {
            //_state = State.CROUCHING;
            _anim.SetBool(Animator.StringToHash("IsCrouching"), true);
            _currentSpeed = _speed + _crouchSpeedModif;
            return true;
        }
        else
        {
            //_state = State.STANDING;
            _anim.SetBool(Animator.StringToHash("IsCrouching"), false);
            _currentSpeed = _speed;
            return false;
        }
    }

    private void CheckFalling()
    {
        /*Vector3 originRaycastMiddle = new Vector3(
            _tr.position.x, _tr.position.y + _raycastOffsetY + _floorOffsetY, _tr.position.z);*/

        if (!Physics.Raycast(_tr.position, Vector2.down, _distanceTocheckGround, _solidLayer))
        {
            _anim.SetBool(Animator.StringToHash("IsFalling"), true);
            _state = State.JUMPING;
            //_anim.SetTrigger(Animator.StringToHash("Fall"));
        }
        //_anim.SetBool(Animator.StringToHash("IsFalling"), false);
    }

    private void CheckGround()
    {


        if (Physics.Raycast(_tr.position, Vector2.down, _distanceTocheckGround, _solidLayer)
            && _rb.velocity.y <= 0)
        {
            _anim.SetBool(Animator.StringToHash("IsLanding"), true);
            //_anim.SetTrigger(Animator.StringToHash("Land"));
            _state = State.STANDING;
        }
        //_anim.SetBool(Animator.StringToHash("IsLanding"), false);
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Fire1"))
        {

            //_rb.velocity = new Vector3(_rb.velocity.x, _jumpForce * Time.deltaTime, _rb.velocity.z);
            //_moveY = new Vector3(_rb.velocity.x, _jumpForce, _rb.velocity.z);
            _rb.AddForce(Vector3.up * _jumpForce);
            _anim.SetTrigger(Animator.StringToHash("Jump"));
            _state = State.JUMPING;
        }
    }

    private void Move()
    {
        float amplitudeX = Input.GetAxis("Vertical");
        float amplitudeZ = Input.GetAxis("Horizontal");

        _anim.SetFloat(Animator.StringToHash("VelocityX"), amplitudeX);
        _anim.SetFloat(Animator.StringToHash("VelocityZ"), amplitudeZ);

        _moveX = new Vector3(
            amplitudeX * _currentSpeed * _cameraMain.transform.forward.x,
            _rb.velocity.y,
            amplitudeX * _currentSpeed * _cameraMain.transform.forward.z
            );

        _moveZ = new Vector3(
            amplitudeZ * _currentSpeed * _cameraMain.transform.right.x,
            0f,
            amplitudeZ * _currentSpeed * _cameraMain.transform.right.z
            );

        //look at where it moves
        if (Mathf.Abs(amplitudeX) >= 0.1f || Mathf.Abs(amplitudeZ) >= 0.1f)
        {
            Vector3 lookAt = _cameraMain.transform.forward;
            lookAt.y = 0;

            Quaternion rotateTo = Quaternion.LookRotation(lookAt);

            float step = _speedRotation * Time.deltaTime;
            _tr.rotation = Quaternion.RotateTowards(_tr.rotation, rotateTo, step);
        }
    }


    private void FixedUpdate()
    {
        //float currentSpeed = _currentSpeed * Time.fixedDeltaTime;
        if (_state == State.STANDING || _state == State.RUNNING || _state == State.CROUCHING
            || _state == State.AIMING)
        {
            //Axe Vertical du joystick
            _rb.velocity = _moveX * Time.fixedDeltaTime;
            //Axe Horizontal du joystick
            _rb.velocity += _moveZ * Time.fixedDeltaTime;

        }

        if (_state != State.CROUCHING && (Mathf.Abs(_rb.velocity.x) > 0.01 || Mathf.Abs(_rb.velocity.y) > 0.01))
            _colliderNoise.SetActive(true);
        else
            _colliderNoise.SetActive(false);

        //find the Y position via raycasts
        /*Vector3 floorMovement = new Vector3(_rb.position.x, FindFloor().y, _rb.position.z);

        Vector3 originRaycastMiddle = new Vector3(
            _tr.position.x, _tr.position.y + _raycastOffsetY + _floorOffsetY, _tr.position.z);

        if (StairsRayCasts(originRaycastMiddle) != Vector3.zero && floorMovement != _rb.position)
        {
            // move the rigidbody to the floor
            _rb.MovePosition(floorMovement);
        }
        */



    }

    private Vector3 FindFloor()
    {
        Vector3 originRaycastMiddle = new Vector3(
            _tr.position.x, _tr.position.y + _raycastOffsetY, _tr.position.z);
        Vector3 originRaycastForward = new Vector3(
            _tr.position.x + _raycastOffset, _tr.position.y + _raycastOffsetY, _tr.position.z);
        Vector3 originRaycastBack = new Vector3(
            _tr.position.x - _raycastOffset, _tr.position.y + _raycastOffsetY, _tr.position.z);
        Vector3 originRaycastRight = new Vector3(
            _tr.position.x, _tr.position.y + _raycastOffsetY, _tr.position.z + _raycastOffset);
        Vector3 originRaycastLeft = new Vector3(
            _tr.position.x, _tr.position.y + _raycastOffsetY, _tr.position.z - _raycastOffset);

        Debug.DrawRay(originRaycastForward, Vector2.down * _distanceToCheckStairs, Color.red);
        Debug.DrawRay(originRaycastBack, Vector2.down * _distanceToCheckStairs, Color.red);
        Debug.DrawRay(originRaycastRight, Vector2.down * _distanceToCheckStairs, Color.red);
        Debug.DrawRay(originRaycastLeft, Vector2.down * _distanceToCheckStairs, Color.red);

        int floorAverage = 1;

        _CombinedRaycast = StairsRayCasts(originRaycastMiddle);

        floorAverage += GetFloorAverage(originRaycastForward) + GetFloorAverage(originRaycastBack)
            + GetFloorAverage(originRaycastRight) + GetFloorAverage(originRaycastLeft);

        return _CombinedRaycast / floorAverage;
    }


    private int GetFloorAverage(Vector3 position)
    {

        if (StairsRayCasts(position) != Vector3.zero)
        {
            _CombinedRaycast += StairsRayCasts(position);
            return 1;
        }
        else return 0;
    }

    private Vector3 StairsRayCasts(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, Vector2.down, out hit, _distanceToCheckStairs, _stairLayer))
            return hit.point;
        else
            return Vector3.zero;
    }





#if UNITY_EDITOR
    void OnGUI()
    {
        GUI.Button(new Rect(10, 10, 120, 30), $"State: {_state}");
        if (GUI.Button(new Rect(10, 50, 120, 30), "Reset"))
        {
            /*_tr.position = _startPos;
            _anim.SetBool(Animator.StringToHash("IsDead"), false);
            _anim.SetTrigger(Animator.StringToHash("Reset"));
            _anim.ResetTrigger(Animator.StringToHash("Reset"));
            _state = State.STANDING;
            */
            SceneManager.LoadScene("SampleScene");

        }


    }
#endif
}
