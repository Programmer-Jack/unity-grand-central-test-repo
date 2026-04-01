using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerMotionState
{
    Stationary,
    Walking,
    Sneaking,
    Sprinting,
    Jumping,
    Falling
}

public class PlayerMovement : MonoBehaviour
{
    [Flags]
    private enum PlayerInputFlags
    {
        None = 0,
        Sprint = 1,
        Sneak = 1 << 1,
        Jump = 1 << 2
    }

    // Input
    private InputSystem_Actions _inputActions;
    private InputActionMap _playerActions;
    private InputAction _move,
                        _look,
                        _sneak,
                        _sprint,
                        _jump;

    [SerializeField] private CharacterController _charController;

    // Walk
    private Vector3 _moveVectorLocalXZ = Vector3.zero;
    private Vector3 _velocityVector = Vector3.zero;
    [SerializeField] private float _walkSpeed = 3f,
                                   _sneakSpeed = 1.5f,
                                   _sprintSpeed = 5f;
    private float _currentSpeed;

    // Look
    private bool _didLookUpdate = false;
    private float _lookDeltaX;
    [SerializeField] private float _horizontalLookSensitivity = 5.0f;

    // Jump
    private bool _wasGrounded = false;
    private readonly float _gravityAcceleration = -9.81f;
    [SerializeField] private float _jumpHeight = 1.1f;
    //[SerializeField] private float _inAirMoveDamp = 0.5f;

    // State
    private PlayerInputFlags _InputFlags = 0;
    private PlayerMotionState _MotionState;
    public PlayerMotionState MotionState
    {
        get
        {
            return _MotionState;
        }
        set
        {
            _MotionState = value;

            _currentSpeed = value switch
            {
                PlayerMotionState.Walking => _walkSpeed,
                PlayerMotionState.Sneaking => _sneakSpeed,
                PlayerMotionState.Sprinting => _sprintSpeed,
                _ => _currentSpeed
            };
            //print($"{value}");
            // _camAnimator.SetState(value);
        }
    }

    // [SerializeField] private CameraAnimator _camAnimator;

    private void Awake()
    {
        _inputActions = ControlsManager.Instance.InputActions;
        _playerActions = _inputActions.Player;

        _move = _playerActions.FindAction("Move", true);
        _look = _playerActions.FindAction("Look", true);

        _sneak = _playerActions.FindAction("Sneak", true);
        _sprint = _playerActions.FindAction("Sprint", true);
        _jump = _playerActions.FindAction("Jump", true);
    }

    private void OnEnable()
    {
        _move.performed += SetWalkVector;
        _move.canceled += SetWalkVector;
        _look.performed += Look;

        _jump.performed += _ => SetInputFlags(PlayerInputFlags.Jump, true);
        _jump.canceled += _ => SetInputFlags(PlayerInputFlags.Jump, false);
        //_sneak.performed += _ => SetInputFlags(PlayerInputFlags.Sneak, true);
        //_sneak.canceled += _ => SetInputFlags(PlayerInputFlags.Sneak, false);
        _sneak.performed += _ => ToggleInputFlag(PlayerInputFlags.Sneak);
        //_sprint.performed += _ => SetInputFlags(PlayerInputFlags.Sprint, true);
        //_sprint.canceled += _ => SetInputFlags(PlayerInputFlags.Sprint, false);
        _sprint.performed += _ => ToggleInputFlag(PlayerInputFlags.Sprint);
    }

    private void OnDisable()
    {
        _move.performed -= SetWalkVector;
        _move.canceled -= SetWalkVector;

        _jump.performed -= _ => SetInputFlags(PlayerInputFlags.Jump, true);
        _jump.canceled -= _ => SetInputFlags(PlayerInputFlags.Jump, false);
        //_sneak.performed -= _ => SetInputFlags(PlayerInputFlags.Sneak, true);
        //_sneak.canceled -= _ => SetInputFlags(PlayerInputFlags.Sneak, false);
        _sneak.performed -= _ => ToggleInputFlag(PlayerInputFlags.Sneak);
        //_sprint.performed -= _ => SetInputFlags(PlayerInputFlags.Sprint, true);
        //_sprint.canceled -= _ => SetInputFlags(PlayerInputFlags.Sprint, false);
        _sprint.performed -= _ => ToggleInputFlag(PlayerInputFlags.Sprint);
    }

    private void Start()
    {
        MotionState = PlayerMotionState.Stationary;
    }

    // Faster than enum.HasFlag()
    private bool HaveCommonFlags(PlayerInputFlags a, PlayerInputFlags b) => (a & b) != 0;

    private void SetInputFlagsNoUpdate(PlayerInputFlags flag, bool condition)
    {
        if (condition) _InputFlags |= flag;
        else _InputFlags &= ~flag;
    }

    private void SetInputFlags(PlayerInputFlags flag, bool condition)
    {
        SetInputFlagsNoUpdate(flag, condition);
        EvaluateState();
    }

    private void ToggleInputFlag(PlayerInputFlags state)
    {
        SetInputFlags(state, !HaveCommonFlags(_InputFlags, state));
    }

    private void SetWalkVector(InputAction.CallbackContext ctx)
    {
        Vector2 rawMotionVector = ctx.ReadValue<Vector2>();
        _moveVectorLocalXZ = new(rawMotionVector.x, 0, rawMotionVector.y);
        EvaluateState();
    }

    private void Look(InputAction.CallbackContext ctx)
    {
        _didLookUpdate = true;
        _lookDeltaX = ctx.ReadValue<Vector2>().x;
    }

    private void LateUpdate()
    {
        if (_didLookUpdate)
        {
            _didLookUpdate = false;
            transform.Rotate(new(0, _lookDeltaX * _horizontalLookSensitivity, 0));
        }
    }

    private void EvaluateState()
    {
        if (HaveCommonFlags(_InputFlags, PlayerInputFlags.Jump))
        {
            MotionState = PlayerMotionState.Jumping;
            return;
        }
        if (!_charController.isGrounded)
        {
            MotionState = PlayerMotionState.Falling;
            return;
        }
        if (HaveCommonFlags(_InputFlags, PlayerInputFlags.Sneak))
        {
            MotionState = PlayerMotionState.Sneaking;
            SetInputFlagsNoUpdate(PlayerInputFlags.Sprint, false);
            return;
        }
        if (_moveVectorLocalXZ.magnitude > 0)
        {
            // Sprint flag set and moving forward?
            if (HaveCommonFlags(_InputFlags, PlayerInputFlags.Sprint) &&
                _moveVectorLocalXZ.z > 0)
            {
                MotionState = PlayerMotionState.Sprinting;
                return;
            }
            MotionState = PlayerMotionState.Walking;
            return;
        }
        SetInputFlagsNoUpdate(PlayerInputFlags.Sprint, false);
        MotionState = PlayerMotionState.Stationary;
    }

    private void Update()
    {
        Vector3 localDirection = transform.TransformDirection(_moveVectorLocalXZ);
        _velocityVector.x = _currentSpeed * localDirection.x;
        _velocityVector.z = _currentSpeed * localDirection.z;

        _velocityVector.y += _gravityAcceleration * Time.deltaTime;

        if (_charController.isGrounded)
        {
            if (MotionState == PlayerMotionState.Jumping)
            {
                _velocityVector.y += Mathf.Sqrt(_jumpHeight * -2 * _gravityAcceleration);
            }
            if (_velocityVector.y < -0.1f)
            //else
            {
                // Applies slight downward velocity to ensure it stays grounded.
                _velocityVector.y = -0.1f;
            }
            if (!_wasGrounded)
            {
                _wasGrounded = true;
                EvaluateState();
            }
        }
        else
        {
            if (_wasGrounded)
            {
                _wasGrounded = false;
            }
        }
        _charController.Move(_velocityVector * Time.deltaTime);
    }
}