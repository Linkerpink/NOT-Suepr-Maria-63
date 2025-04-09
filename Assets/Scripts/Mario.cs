using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Mario : MonoBehaviour
{
    #region Variables
    
    private Rigidbody m_rigidbody;
    public Transform cameraTransform;
    public LayerMask groundLayer;
    public bool canMove = true;
    [SerializeField] private float stickDeadZone = 0.5f;
    
    private GameObject m_marioVisual;

    // Movement
    private float moveSpeed = 0f;
    [SerializeField] private float maxMoveSpeed = 6.0f;
    [SerializeField] private float maxCrouchMoveSpeed = 1.5f;
    private bool isCrouching = false;
    private float maxWalkMoveSpeed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    private Vector3 velocity;
    
    // Jump
    [SerializeField] private float jumpForce = 8f;
    private int jumpCount = 0;
    private float jumpTimer = 0f;
    [SerializeField] private float jumpTimerDuration = 0.5f;
    [SerializeField] private float groundedRayCastLength = 0.5f;
    
    // Ground pound
    private float groundPoundTimer = 0f;
    [SerializeField] private float groundPoundTimerDuration = 0.5f;
    private float groundPoundMoveSpeed = -15f;
    
    // Long Jump
    private float longJumpTimer = 0f;
    private float longJumpTimerDuration = 0.75f;
    
    // Attacks
    private int attackCount = 0;
    private float attackTimer = 0f;
    [SerializeField] private float attackTimerDuration = 1f;
    private float attackComboTimer = 0f;
    private float attackComboTimerDuration = 2.5f;
    
    // Sliding
    private float diveSlideTimer = 0f;
    [SerializeField] private float diveSlideTimerDuration = 0.5f;
    
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
    
    // Health & Enemy stuff
    public int hp = 8;
    private bool isOnEnemy;
    public LayerMask enemyLayer;
    private float iFrameTimer; // iFrame: Invincible Frames
    private float iFrameDuration = 1f;
    private float knockBack = 5f;
    private PowerMeter m_powerMeter;
    
    private float blinkTimer = 0f;
    [SerializeField] private float blinkTimerDuration = 0.1f;
    private bool m_showModel = true;

    public enum States
    {
        Idle,
        Walk,
        Run,
        Jump,
        Crouch,
        CrouchWalk,
        GroundPound,
        LongJump,
        Punch,
        Kick,
        Dive,
        DiveSlide,
        HighJump,
    }

    public States state = States.Idle;

    #endregion
    
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_gameManager = FindFirstObjectByType<GameManager>();
        m_animator = GetComponentInChildren<Animator>();
        m_marioVisual = m_animator.gameObject;
        m_powerMeter = FindAnyObjectByType<PowerMeter>();
    }

    private void Start()
    {
        maxWalkMoveSpeed = maxMoveSpeed / 3;
    }
    
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundedRayCastLength, groundLayer);
        isOnEnemy = Physics.Raycast(transform.position, Vector3.down, groundedRayCastLength, enemyLayer);
        
        
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
        
        // Set values for the Animator
        m_animator.SetFloat("stickMovement", moveDirection.sqrMagnitude);
        m_animator.SetBool("isGrounded", isGrounded);
        
        // Decide what state to switch to
        if (canMove && state != States.Jump && state != States.GroundPound && state != States.LongJump && isGrounded && state != States.GroundPound && state != States.Dive && state != States.Punch && state != States.Kick && state != States.DiveSlide)
        {
            if (!isCrouching)
            {
                // If not crouching
                
                if (inputDirection.sqrMagnitude >= 0.25f)
                {
                    state = States.Run;
                }
                else if (inputDirection.sqrMagnitude >= 0.01f)
                {
                    state = States.Walk;
                }
                else if (state != States.Kick && state != States.Punch)
                {
                    state = States.Idle;
                    //m_animator.SetTrigger("land");
                }
                
                m_animator.SetBool("crouch", false);
            }
            else
            {
                // If crouching
                
                if (inputDirection.sqrMagnitude >= stickDeadZone)
                {
                    state = States.CrouchWalk;
                    
                }
                else if (state != States.Kick && state != States.Punch)
                {
                    state = States.Crouch;
                    m_animator.SetBool("crouch", true);
                }
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
                isCrouching = false;
                break;
            
            // Walk
            case States.Walk:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxWalkMoveSpeed);
                isCrouching = false;
                break;
            
            // Run
            case States.Run:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxMoveSpeed);
                isCrouching = false;
                break;
            
            // Jump
            case States.Jump:
                jumpTimer = jumpTimerDuration;
                
                if (isGrounded)
                {
                    m_animator.SetTrigger("land");
                    state = States.Idle;
                }
                
                isCrouching = false;
                break;
            
            // Crouch
            case States.Crouch:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxCrouchMoveSpeed);
                isCrouching = true;
                break;
            
            // CrouchWalk
            case States.CrouchWalk:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxCrouchMoveSpeed);
                isCrouching = true;
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
                    m_animator.SetTrigger("land");
                }
                jumpCount = 0;
                isCrouching = false;
                break;
                
            // LongJump
            case States.LongJump:
                isCrouching = false;
                m_animator.SetTrigger("longJump");
                
                if (longJumpTimer < longJumpTimerDuration / 2)
                {
                    longJumpTimer += Time.deltaTime;
                    
                    float _jumpForce = jumpForce / 1.5f;
                    float _jumpSpeed = moveSpeed * 6;
                    m_rigidbody.linearVelocity = new Vector3(bufferedMoveDirection.x * _jumpSpeed, _jumpForce, bufferedMoveDirection.z * _jumpSpeed);
                }
                else if (longJumpTimer < longJumpTimerDuration)
                {
                    longJumpTimer += Time.deltaTime;
                    float _jumpForce = 0f;
                    float _jumpSpeed = moveSpeed * 6;
                    m_rigidbody.linearVelocity = new Vector3(bufferedMoveDirection.x * _jumpSpeed, _jumpForce, bufferedMoveDirection.z * _jumpSpeed);
                }
                else
                {
                    if (isGrounded)
                    {
                        state = States.Idle;
                        longJumpTimer = 0;
                        m_animator.SetTrigger("land");
                    }
                }
                break;
            
            // HighJump
            case States.HighJump:
                if (isGrounded)
                {
                    m_animator.SetTrigger("land");
                    state = States.Idle;
                }
                
                isCrouching = false;
                break;
            
            // DiveSlide
            case States.DiveSlide:
                moveDirection = new Vector3(bufferedMoveDirection.x, 0, bufferedMoveDirection.z);
                
                if (diveSlideTimer <= 0)
                {
                    state = States.Idle;
                    m_animator.SetTrigger("land");
                }
                isCrouching = false;
                break;
            
            //////////Attacks//////////////
            
            // Punch
            case States.Punch:
                if (attackTimer <= 0)
                {
                    state = States.Idle;
                }
                moveDirection = Vector3.zero;
                isCrouching = false;
                break;
            
            // Kick
            case States.Kick:
                if (attackTimer <= 0)
                {
                    state = States.Idle;
                }
                moveDirection = Vector3.zero;
                isCrouching = false;
                break;
            
            // Dive
            case States.Dive:
                if (isGrounded)
                {
                    state = States.DiveSlide;
                    diveSlideTimer = diveSlideTimerDuration;
                }
                jumpCount = 0;
                isCrouching = false;
                break;
        }
        
        // Set movement and rotations
        if (state != States.GroundPound && state != States.LongJump && state != States.Dive)
        {
            transform.rotation = Quaternion.Euler(0f, angle, 0f);    
        }
        
        if (moveDirection != Vector3.zero)
        {
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        velocity = moveDirection.normalized * moveSpeed;
        if (state != States.GroundPound && state != States.LongJump && state != States.Dive && state != States.DiveSlide)
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
            m_animator.SetTrigger("land");
        }

        if (diveSlideTimer > 0)
        {
            diveSlideTimer -= Time.deltaTime;
        }
        
        // Iframes
        if (iFrameTimer > 0)
        {
            iFrameTimer -= Time.deltaTime;

            if (blinkTimer <= 0)
            {
                blinkTimer = blinkTimerDuration;
                m_showModel = !m_showModel;
            }

            if (blinkTimer > 0)
            {
                blinkTimer -= Time.deltaTime;
            }
        }
        else
        {
            m_showModel = true;
        }

        if (m_showModel)
        {
            m_marioVisual.SetActive(true);
        }
        else
        {
            m_marioVisual.SetActive(false);
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
            if (state != States.LongJump && state != States.Dive)
            {
                bufferedMoveDirection = moveDirection;    
            }
            
            if (state == States.CrouchWalk && moveDirection.sqrMagnitude > stickDeadZone)
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
            
            // High jump
            if (state == States.Crouch)
            {
                m_animator.SetBool("crouch", false);
                
                state = States.HighJump;
                
                m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, jumpForce * 2f, m_rigidbody.linearVelocity.z);
                
                m_animator.SetTrigger("highJump");
            }

            if (state == States.DiveSlide && isGrounded)
            {
                state = States.Idle;
                m_animator.SetTrigger("land");
            }
        }
    }

    public void OnCrouch(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            if (isGrounded)
            {
                isGrounded = true;
                isCrouching = true;
                m_animator.SetBool("crouch", true);
            }
            else if (state != States.GroundPound && !isGrounded)
            {
                m_animator.SetTrigger("groundPound");
                state = States.GroundPound;
                groundPoundTimer = groundPoundTimerDuration;
            }
        }
        
        if (_context.canceled && state == States.Crouch || _context.canceled && state == States.CrouchWalk)
        {
            isCrouching = false;
            m_animator.SetBool("crouch", false);
        }
    }

    public void OnAttack(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            if (attackCount <= 2 && isGrounded && attackTimer <= 0 && moveDirection.sqrMagnitude < stickDeadZone)
            {
                // Punch
                attackCount++;
                state = States.Punch;
                m_animator.SetTrigger("punch");
                attackTimer = attackTimerDuration;
                attackComboTimer = attackComboTimerDuration;
            }
            else if (state != States.Run && attackTimer <= 0 && moveDirection.sqrMagnitude < stickDeadZone)
            {
                // Kick
                attackCount++;
                state = States.Kick;
                m_animator.SetTrigger("kick");
                attackTimer = attackTimerDuration;
                attackComboTimer = attackComboTimerDuration;
            }
            
            if (state != States.Dive && state != States.DiveSlide && moveDirection.sqrMagnitude > stickDeadZone && state != States.LongJump && !isGrounded)
            {
                //Dive
                attackCount++;
                state = States.Dive;
                m_animator.SetTrigger("dive");
                m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x * 1.5f, m_rigidbody.linearVelocity.y, m_rigidbody.linearVelocity.z * 1.5f);
            }
        }
    }

    public void TakeDamage(int _damageAmount)
    {
        if (!isOnEnemy && state != States.Punch && state != States.Kick && state != States.Dive && iFrameTimer <= 0)
        {
            hp -= _damageAmount;
            if (hp <= 0)
            {
                Die();
            }

            iFrameTimer = iFrameDuration;
            
            // Power Meter
            if (!m_powerMeter.showing)
            {
                print("show power meter");
                m_powerMeter.ShowPowerMeter();
            }
            else
            {
                print("skill issueee");
            }
        }
    }

    public void Heal(int _healAmount)
    {
        if (hp < 8)
        {
            hp += _healAmount;    
        }
        
        // Power Meter
        if (m_powerMeter.showing && hp >= 8)
        {
            print("hide power meter");
            m_powerMeter.HidePowerMeter();
        }
    }
    
    public void Die()
    {
        SceneManager.LoadScene("Peach's Castle Inside Main Room");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jump Hitbox"))
        {
            if (isOnEnemy)
            {
                other.GetComponentInParent<Enemy>().TakeDamage(1);
                m_rigidbody.AddForce(0f,10f,0f,ForceMode.Impulse);
            }
        }

        if (other.CompareTag("King Bob Omb"))
        {
            if (state == States.Dive || state == States.Punch || state == States.Kick)
            {
                
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
        
        GUIStyle m_greenText = new GUIStyle
        {
            fontSize = 48,
            normal = { textColor = Color.green }
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
            GUI.Label(new Rect(10, 710, 300, 40), "isCrouching: " + isCrouching, m_Style);
            GUI.Label(new Rect(10, 760, 300, 40), "longJumpTimer: " + longJumpTimer, m_Style);
            GUI.Label(new Rect(10, 810, 300, 40), "bufferedMoveDirection: " + bufferedMoveDirection, m_Style);
            GUI.Label(new Rect(10, 860, 300, 40), "HP: " + hp, m_greenText);
            GUI.Label(new Rect(10, 910, 300, 40), "iFrameTimer: " + iFrameTimer, m_greenText);
            //GUI.Label(new Rect(10, 960, 300, 40), "attackTimer: " + attackTimer, m_Style);
            GUI.Label(new Rect(10, 960, 300, 40), "power meter showing: " + m_powerMeter.showing, m_Style);
        }
    }
}