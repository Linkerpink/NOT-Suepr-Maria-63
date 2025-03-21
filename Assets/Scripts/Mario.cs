using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Mario : MonoBehaviour
{
    #region Variables
    
    private Rigidbody m_rigidbody;
    public Transform cameraTransform;
    public LayerMask groundLayer;
    public bool canMove = true;
    [SerializeField] private float stickDeadZone = 0.5f;

    // Movement
    private float moveSpeed = 0f;
    [SerializeField] private float maxMoveSpeed = 6.0f;
    private float maxWalkMoveSpeed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    private Vector3 velocity;
    
    // Jump
    [SerializeField] private float jumpForce = 8f;
    private int jumpCount = 0;
    private float jumpTimer = 0f;
    [SerializeField] private float jumpTimerDuration = 1f;
    
    // Ground pound
    private float groundPoundTimer = 0f;
    [SerializeField] private float groundPoundTimerDuration = 0.5f;
    private float groundPoundMoveSpeed = -15f;
    
    // Attacks
    private int attackCount = 0;
    private float attackTimer = 0f;
    [SerializeField] private float attackTimerDuration = 1f;
    private float attackComboTimer = 0f;
    private float attackComboTimerDuration = 2.5f;
    
    // Sliding
    private float diveSlideTimer = 0f;
    private float diveSlideTimerDuration = 2.5f;
    
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private bool isGrounded;
    
    // Directions
    private Vector3 moveDirection;
    private Vector3 bufferedMoveDirection; 
    private Vector2 inputDirection;
    private Vector3 jumpDirection;
    
    private GameManager m_gameManager;

    // Angles
    private float angle;
    private float targetAngle;
    private float lastAngle;
    
    private Animator m_animator;

    private enum States
    {
        Idle,
        Walk,
        Run,
        Jump,
        Crouch,
        GroundPound,
        LongJump,
        Punch,
        Kick,
        Dive,
        DiveSlide,
    }

    private States state = States.Idle;

    #endregion
    
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_gameManager = FindFirstObjectByType<GameManager>();
        m_animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        maxWalkMoveSpeed = maxMoveSpeed / 3;
    }
    
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        
        if (inputDirection.sqrMagnitude > 0.01f && state != States.DiveSlide) 
        {
            moveDirection = new Vector3(inputDirection.x, 0, inputDirection.y);
            targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            lastAngle = angle;
        }
        else
        {
            moveDirection = Vector3.zero;
            angle = lastAngle;
        }

        if (canMove && state != States.Jump && state != States.GroundPound && state != States.LongJump && state != States.Crouch && isGrounded && state != States.GroundPound && state != States.Dive && state != States.Punch && state != States.Kick && state != States.DiveSlide)
        {
            if (inputDirection.sqrMagnitude >= 0.25f)
            {
                state = States.Run;
                m_animator.SetTrigger("run");
            }
            else if (inputDirection.sqrMagnitude >= 0.01f)
            {
                state = States.Walk;
            }
            else if (state != States.Kick && state != States.Punch)
            {
                state = States.Idle;
            }    
        }

        if (!isGrounded && state != States.GroundPound && state != States.LongJump && state != States.Dive && state != States.Punch && state != States.Kick)
        {
            state = States.Jump;
        }
        
        // State switching
        switch (state)
        {
            //////////Movement//////////////
            
            // Idle
            case States.Idle:
                moveSpeed = Mathf.Max(moveSpeed - deceleration * Time.deltaTime, 0);
                jumpCount = 0;
                break;
            
            // Walk
            case States.Walk:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxWalkMoveSpeed);
                break;
            
            // Run
            case States.Run:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxMoveSpeed);
                break;
            
            // Jump
            case States.Jump:
                if (jumpCount < 3)
                {
                    jumpTimer = jumpTimerDuration;    
                }
                else
                {
                    jumpTimer = jumpTimerDuration / 4;
                }
                
                if (isGrounded)
                {
                    m_animator.SetTrigger("land");
                    state = States.Idle;
                }
                break;
            
            // Crouch
            case States.Crouch:
                break;
            
            // GroundPound
            case States.GroundPound:
                float _verticalMovement = 0f;

                if (groundPoundTimer <= 0)
                {
                    _verticalMovement = groundPoundMoveSpeed;
                }
                
                m_rigidbody.linearVelocity = new Vector3(0, _verticalMovement, 0);

                if (isGrounded)
                {
                    state = States.Idle;
                }
                jumpCount = 0;
                break;
                
            // LongJump
            case States.LongJump:
                float _jumpForce = jumpForce / 2;
                m_rigidbody.linearVelocity = new Vector3(bufferedMoveDirection.x * 1.5f, _jumpForce, bufferedMoveDirection.z * 1.5f);
                break;
            
            // DiveSlide
            case States.DiveSlide:
                moveDirection = new Vector3(bufferedMoveDirection.x, 0, bufferedMoveDirection.z);
                
                if (diveSlideTimer <= 0)
                {
                    state = States.Idle;
                }
                break;
            
            //////////Attacks//////////////
            
            // Punch
            case States.Punch:
                if (attackTimer <= 0)
                {
                    state = States.Idle;
                }
                moveDirection = Vector3.zero;
                break;
            
            // Kick
            case States.Kick:
                if (attackTimer <= 0)
                {
                    state = States.Idle;
                }
                moveDirection = Vector3.zero;
                break;
            
            // Dive
            case States.Dive:
                if (isGrounded)
                {
                    state = States.DiveSlide;
                    diveSlideTimer = diveSlideTimerDuration;
                }
                jumpCount = 0;
                break;
        }
        
        // Set movement and rotations
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        if (moveDirection != Vector3.zero) 
        {
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        velocity = moveDirection.normalized * moveSpeed;
        if (state != States.GroundPound)
        {
            m_rigidbody.linearVelocity = new Vector3(velocity.x, m_rigidbody.linearVelocity.y, velocity.z);
        }

        // Timers
        if (jumpTimer > 0 && isGrounded)
        {
            jumpTimer -= Time.deltaTime;
        }
        
        if (groundPoundTimer > 0 && state == States.GroundPound)
        {
            groundPoundTimer -= Time.deltaTime;
        }
        
        if (jumpTimer <= 0)
        {
            jumpCount = 0;
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (attackComboTimer > 0)
        {
            attackComboTimer -= Time.deltaTime;
        }
        else
        {
            attackCount = 0;
        }

        if (attackCount > 3)
        {
            attackCount = 0;
        }

        if (diveSlideTimer > 0)
        {
            diveSlideTimer -= Time.deltaTime;
        }
        
        m_animator.SetInteger("jumpCount", jumpCount);
    }
    
    public void OnMove(InputAction.CallbackContext _context)
    {
        inputDirection = _context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext _context)
    {

        if (_context.performed)
        {
            // Long jump
            bufferedMoveDirection = moveDirection;
        
            if (state == States.Crouch && moveDirection.sqrMagnitude > stickDeadZone)
            {
                jumpCount = 1;
            
                state = States.LongJump;
            }
        
            // Jump
            if (isGrounded && state != States.Jump && state != States.LongJump && state != States.DiveSlide)
            {
                if (jumpCount < 3)
                {
                    jumpCount++;                
                }
            
                float _jumpForce = jumpForce + jumpCount;
                m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, _jumpForce, m_rigidbody.linearVelocity.z);
                state = States.Jump;
            }

            if (state == States.DiveSlide && isGrounded)
            {
                state = States.Idle;
                m_animator.SetTrigger("landed");
            }
        }
    }

    public void OnCrouch(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            print("button pressed");
            if (isGrounded)
            {
                state = States.Crouch;
                Debug.Log("Crouch");
            }
            else if (state != States.GroundPound)
            {
                m_animator.SetTrigger("groundPound");
                state = States.GroundPound;
                groundPoundTimer = groundPoundTimerDuration;
            }
        }

        if (_context.canceled)
        {
            if (state == States.Crouch)
            {
                state = States.Idle;    
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            attackCount++;
            
            if (attackCount <= 2 && isGrounded && attackTimer <= 0 && moveDirection.sqrMagnitude < stickDeadZone)
            {
                // Punch
                state = States.Punch;
                m_animator.SetTrigger("punch");
                attackTimer = attackTimerDuration;
                attackComboTimer = attackComboTimerDuration;
            }
            else if (state != States.Run && attackTimer <= 0 && moveDirection.sqrMagnitude < stickDeadZone)
            {
                // Kick
                state = States.Kick;
                m_animator.SetTrigger("kick");
                attackTimer = attackTimerDuration;
                attackComboTimer = attackComboTimerDuration;
            } 
            
            if (state != States.Dive && moveDirection.sqrMagnitude > stickDeadZone)
            {
                //Dive
                state = States.Dive;
                m_animator.SetTrigger("dive");
                bufferedMoveDirection = moveDirection;
                m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, m_rigidbody.linearVelocity.y, m_rigidbody.linearVelocity.z * 1.5f);
            }
        }
    }
    
    private void OnGUI()
    {
        GUIStyle m_Style = new GUIStyle
        {
            fontSize = 48,
            normal = { textColor = Color.white }
        };

        if (m_gameManager.enableDebug)
        {
            GUI.Label(new Rect(10, 10, 300, 40), "Player move speed: " + moveSpeed, m_Style);
            GUI.Label(new Rect(10, 60, 300, 40), "Is grounded: " + isGrounded, m_Style);
            GUI.Label(new Rect(10, 110, 300, 40), "Input Direction: " + inputDirection, m_Style);
            GUI.Label(new Rect(10, 160, 300, 40), "Move Direction: " + moveDirection, m_Style);
            GUI.Label(new Rect(10, 210, 300, 40), "Player State: " + state, m_Style);
            GUI.Label(new Rect(10, 260, 300, 40), "Jump Count: " + jumpCount, m_Style);
            GUI.Label(new Rect(10, 310, 300, 40), "Jump Timer: " + jumpTimer, m_Style); 
            GUI.Label(new Rect(10, 360, 300, 40), "Angle: " + angle, m_Style);
            GUI.Label(new Rect(10, 410, 300, 40), "Target Angle: " + targetAngle, m_Style);
            GUI.Label(new Rect(10, 460, 300, 40), "Last Angle: " + lastAngle, m_Style);
            GUI.Label(new Rect(10, 510, 300, 40), "jumpTimer: " + jumpTimer, m_Style);
            GUI.Label(new Rect(10, 560, 300, 40), "GroundPoundTimer: " + groundPoundTimer, m_Style);
            GUI.Label(new Rect(10, 610, 300, 40), "canMove: " + canMove, m_Style);
            GUI.Label(new Rect(10, 660, 300, 40), "attackCount: " + attackCount, m_Style);
        }
    }
}