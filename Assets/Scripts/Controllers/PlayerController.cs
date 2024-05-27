using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent (typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField]
    private float _jumpForce = 5f;
    [SerializeField]    
    private float _rollForce = 5f, _dashForce = 5f;
    [SerializeField]
    private float _maxCrouchingSpeed = 1f, _maxMovementSpeed = 3f, _maxRunningSpeed = 5f;
    [SerializeField]
    Transform _cameraTarget;
    [SerializeField]
    private float _threshholdCameraMovement = 1f;
    [SerializeField]
    private GameObject _attackCollider;

    [SerializeField]
    [Tooltip("Default time to reach the max movement speed.")]
    [Range(0f, 2)]
    private float _startMovementSmooth = 0.01f, _stopMovementSmooth = 0.01f;
    [SerializeField]
    private float _rollDuration = 1f, _dashDuration = 1f, _attack1Duration = 0.5f, _attack2Duration = 0.7f;

    
    
    [Header("RayCast Values")]
    [SerializeField]
    private float _minDistanceToJump = 0.01f;

    [Header("References")]
    private AnimatorController _animatorController;

    
    
    #region UnityEvents
    [Header("Events")]
    [SerializeField]
    public UnityEvent OnJump;
    [SerializeField]
    public UnityEvent OnStartMovement, OnEndMovement, OnLand, OnTakeOff,
        OnStartRolling, OnEndRolling, OnStartCrouch, OnEndCrouch,
        OnStartAttack1, OnEndAttack1, OnStartAttack2, OnEndAttack2,
        OnStartKnock, OnEndKnock;

    [SerializeField]
    public UnityEvent<float> OnMovementChange;
    #endregion


    private Rigidbody2D _rb2d;
    private Animator _animator;


    #region Debug Attributes
    [Header("Debug")]
    [SerializeField]
    [CustomEditor.ReadOnly]
    private bool _canMove = true, _canJump = true, _canCrouch = true, _canRoll = true, _canAttack = true;
    
    [SerializeField]
    [CustomEditor.ReadOnly]
    private bool _moving = false, _jumping = false, _crouching = false, _rolling = false,
        _attacking1 = false, _attacking2 = false, _knocked = false, _grounded = false;

    [SerializeField]
    [CustomEditor.ReadOnly]
    private float _maxCurrentSpeed, _currentVelocityX;
    #endregion


    private float _currentMovementInput = 0f;



    #region Player States
    public bool IsMoving
    {
        get { return _moving; }
        set
        {
            if(_moving != value)
            {
                _moving = value;
                if(_moving)
                    OnStartMovement?.Invoke();
                else
                    OnEndMovement?.Invoke();
            }

            _moving = value;
        }
    }

    public bool IsJumping
    {
        get { return _jumping; }
        set
        {
            if (_jumping != value)
            {
                _jumping = value;
                if (_jumping)
                    OnJump?.Invoke();
            }
        }
    }

    public bool IsCrouching
    {
        get { return _crouching; }
        set
        {
            if (_crouching != value)
            {
                _crouching = value;
                if (_crouching)
                    OnStartCrouch?.Invoke();
                else
                    OnEndCrouch?.Invoke();
            }
        }
    }

    public bool IsRolling
    {
        get { return _rolling; }
        set
        {
            if (_rolling != value)
            {
                _rolling = value;
                if (_rolling)
                    OnStartRolling?.Invoke();
                else
                    OnEndRolling?.Invoke();
            }
        }
    }

    public bool IsAttacking1
    {
        get { return _attacking1; }
        set
        {
            if(_attacking1 != value)
            {
                _attacking1 = value;
                if(_attacking1)
                    OnStartAttack1?.Invoke();
                else
                    OnEndAttack1?.Invoke();
            }
        }
    }

    public bool IsAttacking2
    {
        get { return _attacking2; }
        set
        {
            if (_attacking2 != value)
            {
                _attacking2 = value;
                if (_attacking2)
                    OnStartAttack2?.Invoke();
                else
                    OnEndAttack2?.Invoke();
            }
        }
    }

    public bool IsKnocked
    {
        get { return _knocked; }
        set
        {
            if(_knocked != value)
            {
                _knocked = value;
                if(_knocked)
                    OnStartKnock?.Invoke();
                else
                    OnEndKnock?.Invoke();
            }
        }
    }

    public bool IsGrounded
    {
        get { return _grounded; }
        set
        {
            if(_grounded != value)
            {
                _grounded = value;
                if(_grounded)
                    OnLand?.Invoke();
                else
                    OnTakeOff?.Invoke();
            }
        }
    }
    #endregion


   
    
    public float CurrentMovementInput
    {
        get { return _currentMovementInput; }
        set
        {
            if (_currentMovementInput != value)
            {
                _currentMovementInput = value;
                OnMovementChange?.Invoke(value);
            }

            _currentMovementInput = value;
        }
    }



    #region InputActions
    private InputAction _jump;
    public InputAction JumpAction
    {
        get
        {
            _jump ??= GameManager.Instance.PlayerInput.actions.FindAction("Jump");
            return _jump;
        }
    }


    private InputAction _move;
    public InputAction MoveAction
    {
        get
        {
            _move ??= GameManager.Instance.PlayerInput.actions.FindAction("Move");
            return _move;
        }
    }

    private InputAction _cameraAction;
    public InputAction CameraAction
    {
        get
        {
            _cameraAction ??= GameManager.Instance.PlayerInput.actions.FindAction("Camera");
            return _cameraAction;
        }
    }

    private InputAction _roll;
    public InputAction RollAction
    {
        get
        {
            _roll ??= GameManager.Instance.PlayerInput.actions.FindAction("Roll");
            return _roll;
        }
    }

    private InputAction _attack1;
    public InputAction Attack1Action
    {
        get
        {
            _attack1 ??= GameManager.Instance.PlayerInput.actions.FindAction("Attack1");
            return _attack1;
        }
    }

    private InputAction _attack2;
    public InputAction Attack2Action
    {
        get
        {
            _attack2 ??= GameManager.Instance.PlayerInput.actions.FindAction("Attack2");
            return _attack2;
        }
    }

    #endregion



    #region MonobehaviourMethods
    private void Awake()
    {
        GetRequiredComponents();
        SetDefaultValues();
        SetOwnEvents();
    }

    private void OnEnable()
    {
        AddActionsEvents();
    }

    private void OnDisable()
    {
        RemoveActionsEvents();
    }

    private void Update()
    {
        IsGrounded = CheckGrounded();
        if(!IsRolling)
            _rb2d.velocity = new Vector2(_currentVelocityX, _rb2d.velocity.y);

        SetAnimations();
    }

    #endregion



    #region Methods "Initialize"
    private void GetRequiredComponents()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _animatorController = GetComponent<AnimatorController>();
    }

    private void SetDefaultValues()
    {
        _maxCurrentSpeed = _maxMovementSpeed;
    }

    private void SetOwnEvents()
    {
        OnMovementChange.AddListener(StartReachTargetSpeed);

        OnStartRolling.AddListener(() => { _canJump = _canCrouch = _canRoll = _canAttack = _canMove = false; });
        OnStartRolling.AddListener(() => _animatorController.PlayAnimation(AnimatorController.AnimationType.Roll));

        OnEndRolling.AddListener(() => { _canJump = _canCrouch = _canRoll = _canAttack = _canMove = true; });

        OnJump.AddListener(() => { _canJump = _canCrouch = _canRoll = false; });
        OnJump.AddListener(() => _animatorController.PlayAnimation(AnimatorController.AnimationType.Jump));

        OnLand.AddListener(() => { _canJump = _canCrouch = _canRoll = true; });
        OnLand.AddListener(() => { IsJumping = false; });

        OnStartAttack1.AddListener(() =>
        {
            _attackCollider.SetActive(true);
            _canCrouch = _canAttack = _canMove = false;
            _animatorController.PlayAnimation(AnimatorController.AnimationType.Attack1);
            CurrentMovementInput = 0;
        });

        OnEndAttack1.AddListener(() => _canCrouch = _canAttack = _canMove = true);
        OnEndAttack1.AddListener(() => _attackCollider.SetActive(false));


        OnStartAttack2.AddListener(() => { 
            _animatorController.PlayAnimation(AnimatorController.AnimationType.Attack2);
            _canCrouch = _canAttack = _canMove = false;
            CurrentMovementInput = 0; 
        });

        OnEndAttack2.AddListener(() => _canCrouch = _canAttack = _canMove = true);
    }

    private void AddActionsEvents()
    {
        JumpAction.performed += Jump;
        MoveAction.performed += Move;
        RollAction.performed += Roll;
        Attack1Action.performed += Attack1;
        Attack2Action.performed += Attack2;

        MoveAction.canceled += StopMove;
    }

    private void RemoveActionsEvents()
    {
        JumpAction.performed -= Jump;
        MoveAction.performed -= Move;

        MoveAction.canceled -= StopMove;
    }
    #endregion



    #region Methods "Action"
    private void Jump(InputAction.CallbackContext context)
    {
        if (_canJump)
        {
            IsJumping = true;
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, 0f);
            _rb2d.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }


    private Coroutine _moveRoutine = null;
    private void Move(InputAction.CallbackContext context)
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
        _moveRoutine = StartCoroutine(MoveRoutine());

    }

    private IEnumerator MoveRoutine()
    {
        IsMoving = true;
        while (IsMoving)
        {
            if (!_canMove)
                yield return new WaitForEndOfFrame();
            else
            {
                CurrentMovementInput = MoveAction.ReadValue<float>();

                if (CurrentMovementInput > 0)
                    transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                else if (CurrentMovementInput < 0)
                    transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);

                yield return new WaitForEndOfFrame();
            }
        }

        CurrentMovementInput = 0;
        IsMoving = false;
    }

    private void StopMove(InputAction.CallbackContext context)
    {
        CurrentMovementInput = 0f;
        IsMoving = false;
    }


    private void Roll(InputAction.CallbackContext context)
    {
        if (_canRoll)
        {
            IsRolling = true;
            Invoke(nameof(StopRolling), _rollDuration);

            _rb2d.velocity = new Vector2(0f, _rb2d.velocity.y);
            _rb2d.AddForce(_rollForce * transform.localScale.x * Vector2.right, ForceMode2D.Impulse);  
        }
    }
    
    private void StopRolling()
    {
        IsRolling = false;
        if (!IsMoving) {
            CurrentMovementInput = 0;
            StartReachTargetSpeed(0f);
        }
    }

    private void Attack1(InputAction.CallbackContext context)
    {
        if (_canAttack)
        {
            IsAttacking1 = true;
            Invoke(nameof(StopAttacking1), _attack1Duration);
        }
    }
    private void StopAttacking1()
    {
        IsAttacking1 = false;
        if (!IsMoving)
        {
            CurrentMovementInput = 0;
            StartReachTargetSpeed(0f);
        }
    }


    private void Attack2(InputAction.CallbackContext context)
    {
        if (_canAttack)
        {
            IsAttacking2 = true;
            Invoke(nameof(StopAttacking2), _attack2Duration);
        }
    }
    private void StopAttacking2()
    {
        IsAttacking2 = false;
        if (!IsMoving)
        {
            CurrentMovementInput = 0;
            StartReachTargetSpeed(0f);
        }
    }
    #endregion



    private float CalculateProportionalSmoothMovement(float initSpeed, float targetSpeed)
    {
        float smooth = initSpeed == 0 ? _stopMovementSmooth : _startMovementSmooth;
        float res = smooth * Mathf.Abs((targetSpeed - initSpeed) / _maxCurrentSpeed);

#if DEBUG_MODE
        Debug.Log("CalculateProportionalSmoothMovement (default = " + _movementSmooth + "); (proportional = " + res + ")");
#endif
        return res;
    }

    private Coroutine _reachMaxSpeedRoutine = null;
    private IEnumerator ReachTargetSpeed(float initSpeed, float targetSpeed)
    {
        float t = 0f, smooth = CalculateProportionalSmoothMovement(initSpeed, targetSpeed);
        do
        {
            t += Time.deltaTime / smooth;
            _currentVelocityX = Mathf.SmoothStep(initSpeed, targetSpeed, t);
            yield return null;
        }
        while (t > 0 && t < 1 && _canMove);
    }
    private void StartReachTargetSpeed(float xDir)
    {
        StopReachMaxSpeed();
        _reachMaxSpeedRoutine = StartCoroutine(ReachTargetSpeed(_rb2d.velocity.x, _maxCurrentSpeed * xDir));
    }
    private void StopReachMaxSpeed()
    {
        if (_reachMaxSpeedRoutine != null)
            StopCoroutine(_reachMaxSpeedRoutine);

        _reachMaxSpeedRoutine = null;
    }



    private bool CheckGrounded()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position - Vector3.down * 0.02f, Vector2.down, _minDistanceToJump, LayerMask.GetMask("Ground"));
        return ray.collider != null;
    }

    private void SetAnimations()
    {
        if (IsAttacking1 || IsAttacking2 || IsRolling)
            return;

        if (IsJumping)
        {
            if (_rb2d.velocity.y < 0)
                _animatorController.PlayAnimation(AnimatorController.AnimationType.Fall);
        }
        else if (IsGrounded)
        {
            if (IsMoving)
            {
                if (IsCrouching)
                    _animatorController.PlayAnimation(AnimatorController.AnimationType.Crouch);
                else
                    _animatorController.PlayAnimation(AnimatorController.AnimationType.Move);
            }  
            else
                _animatorController.PlayAnimation(AnimatorController.AnimationType.Idle);
        }
    }


    private void DisableAllActions()
    {
        _canMove = false;
        _canJump = false;
        _canCrouch = false;
        _canRoll = false;
        _canAttack = false;
    }

    private void EnableAllActions()
    {
        _canMove = true;
        _canJump = true;
        _canCrouch = true;
        _canRoll = true;
        _canAttack = true;
    }
}
