using System;
using System.Collections;
using Unity.Cinemachine;
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
    [SerializeField] private LayerMask poleLayer;
    public bool canMove = false;
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
    private bool isCollidingWithPole;
    
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
        InsideCannon,
    }

    public States state = States.Idle;

    [SerializeField] private GameObject heldObject = null;

    // Dialogue
    private Textbox m_textbox;
    private bool m_isCollidingWithDialogueHitbox = false;
    private Collider m_dialogueHitbox;
    
    private PlayerInput m_playerInput;
    
    // Stars
    private bool m_pickedUpStar = false;
    private bool m_startedVictoryPose = false;
    
    // Bob Omb Battlefield Star 1
    [SerializeField] private DialogueSequence m_kingBobOmbStartBattleDialogueSequence;
    private bool isCollidingWithKingBobOmb = false;
    public GameObject m_kingBobOmbObject;
    public KingBobOmb m_kingBobOmb;

    #endregion
    
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_gameManager = FindFirstObjectByType<GameManager>();
        m_animator = GetComponentInChildren<Animator>();
        m_marioVisual = m_animator.gameObject;
        m_powerMeter = FindAnyObjectByType<PowerMeter>();
        
        m_textbox = FindAnyObjectByType<Textbox>();

        m_playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        maxWalkMoveSpeed = maxMoveSpeed / 3;
    }
    
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundedRayCastLength, groundLayer);
        isOnEnemy = Physics.Raycast(transform.position, Vector3.down, groundedRayCastLength, enemyLayer);
        isCollidingWithPole = Physics.Raycast(transform.position, Vector3.down, groundedRayCastLength, poleLayer);
        
        if (inputDirection.sqrMagnitude > 0.01f && state != States.DiveSlide && canMove) 
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
        if (canMove && state != States.Jump && state != States.GroundPound && state != States.LongJump && isGrounded && state != States.GroundPound && state != States.Dive && state != States.Punch && state != States.Kick && state != States.DiveSlide && state != States.InsideCannon)
        {
            if (!isCrouching)
            {
                // If not crouching
                
                if (inputDirection.sqrMagnitude >= 0.25f)
                {
                    state = States.Run;
                    m_animator.Play("Run");
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

            switch (jumpCount)
            {
                case 1:
                    m_animator.Play("Jump");
                    break;
                
                case 2:
                    m_animator.Play("Double Jump");
                    break;
                
                case 3:
                    m_animator.Play("Triple jump");
                    break;
            }
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
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxMoveSpeed);
                jumpTimer = jumpTimerDuration;
                
                if (isGrounded && canMove || isCollidingWithPole && canMove)
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

                if (isGrounded && canMove)
                {
                    state = States.Idle;
                    m_animator.SetTrigger("land");
                }

                if (isCollidingWithPole && canMove)
                {
                    state = States.Idle;
                    m_animator.SetTrigger("land");

                    ChainChompPole _pole = GameObject.Find("Chain Chomp Area").GetComponentInChildren<ChainChompPole>();
                    
                    _pole.IncreasePolePosition();
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
                    if (isGrounded && canMove || isCollidingWithPole && canMove)
                    {
                        state = States.Idle;
                        longJumpTimer = 0;
                        m_animator.SetTrigger("land");
                    }
                }
                break;
            
            // HighJump
            case States.HighJump:
                if (isGrounded && canMove || isCollidingWithPole && canMove)
                {
                    m_animator.SetTrigger("land");
                    state = States.Idle;
                }
                
                isCrouching = false;
                break;
            
            // DiveSlide
            case States.DiveSlide:
                moveDirection = new Vector3(bufferedMoveDirection.x, 0, bufferedMoveDirection.z);
                
                if (diveSlideTimer <= 0 && canMove)
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
                if (isGrounded || isCollidingWithPole && canMove)
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

        if (attackCount > 3 && canMove)
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
        
        if (canMove)
        {
            m_playerInput.actions.FindActionMap("Player").Enable();
            m_playerInput.actions.FindActionMap("UI").Disable();
        }
        else
        {
            m_playerInput.actions.FindActionMap("Player").Enable();
            m_playerInput.actions.FindActionMap("UI").Disable();
        }
        
        // Star stuff
        if (m_pickedUpStar)
        {
            canMove = false;

            if (isGrounded && !m_startedVictoryPose)
            {
                m_startedVictoryPose = true;
                m_animator.SetTrigger("victoryPose");
            }
        }
        
        // Holding objects
        if (heldObject != null)
        {
            heldObject.transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            heldObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            if (heldObject.name == "King Bob Omb" && m_kingBobOmb != null)
            {
                m_kingBobOmb.state = KingBobOmb.States.Grabbed;
            }
        }
    }
    
    public void OnMove(InputAction.CallbackContext _context)
    {
        inputDirection = _context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext _context)
    {
        if (_context.performed && canMove && state != States.InsideCannon)
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
            if (isGrounded && state != States.Jump && state != States.LongJump && state != States.DiveSlide || isCollidingWithPole && state != States.Jump && state != States.LongJump && state != States.DiveSlide)
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

            if (state == States.DiveSlide && isGrounded && canMove)
            {
                state = States.Idle;
                m_animator.SetTrigger("land");
            }
        }

        if (_context.performed && state == States.InsideCannon)
        {
            float _jumpForce = jumpForce * 10f;
            m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, _jumpForce, m_rigidbody.linearVelocity.z);
            state = States.Jump;
        }
    }

    public void OnCrouch(InputAction.CallbackContext _context)
    {
        if (_context.performed && canMove)
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
        if (_context.performed && canMove)
        {
            // Hold king bob omb
            if (heldObject == null && isCollidingWithKingBobOmb && m_kingBobOmbObject != null)
            {
                heldObject = m_kingBobOmbObject;
            }

            // Throw object
            if (heldObject != null)
            {
                print("throw object");
                
                heldObject.transform.position = new Vector3(heldObject.transform.position.x, transform.position.y + 4, heldObject.transform.position.z) + transform.forward * 4f;

                if (heldObject.name == "King Bob Omb" && m_kingBobOmb != null && m_kingBobOmb.state == KingBobOmb.States.Grabbed)
                {
                    m_kingBobOmb.state = KingBobOmb.States.Thrown;
                    heldObject = null;
                }
            }
            
            // Dialogue
            if (m_isCollidingWithDialogueHitbox && m_dialogueHitbox != null)
            {
                DialogueSequence _dialogueSequence = m_dialogueHitbox.GetComponent<DialogueGiver>().dialogueSequence;
                m_textbox.StartDialogueSequence(_dialogueSequence);
            }
            
            // Attacking
            if (attackCount <= 2 && isGrounded && attackTimer <= 0 && moveDirection.sqrMagnitude < stickDeadZone)
            {
                // Punch
                attackCount++;
                state = States.Punch;
                m_animator.Play("Punch");
                attackTimer = attackTimerDuration;
                attackComboTimer = attackComboTimerDuration;
            }
            else if (state != States.Run && attackTimer <= 0 && moveDirection.sqrMagnitude < stickDeadZone)
            {
                // Kick
                attackCount++;
                state = States.Kick;
                m_animator.Play("Kick");
                attackTimer = attackTimerDuration;
                attackComboTimer = attackComboTimerDuration;
            }
            
            if (state != States.Dive && state != States.DiveSlide && moveDirection.sqrMagnitude > stickDeadZone && state != States.LongJump && !isGrounded)
            {
                //Dive
                attackCount++;
                state = States.Dive;
                m_animator.Play("Dive");
                m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x * 1.5f, m_rigidbody.linearVelocity.y, m_rigidbody.linearVelocity.z * 1.5f);
            }
        }
    }

    public void TakeDamage(int _damageAmount)
    {
        if (iFrameTimer <= 0)
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
            isCollidingWithKingBobOmb = true;
            m_kingBobOmbObject = other.gameObject;
        }
        
        if (other.CompareTag("Dialogue Hitbox"))
        {
            m_isCollidingWithDialogueHitbox = true;
            m_dialogueHitbox = other;
        }

        if (other.CompareTag("Star"))
        {
            print("should pick up star");
            GameManager.Instance.GetStar(other.gameObject.GetComponent<StarHolder>().star);
            other.gameObject.SetActive(false);
            m_pickedUpStar = true;
        }

        if (other.CompareTag("Top Hitbox"))
        {
            RaceManager _raceManager = FindObjectOfType<RaceManager>();

            if (_raceManager != null)
            {
                _raceManager.EndRace();

                if (_raceManager.raceState == RaceManager.RaceStates.Racing)
                {
                    _raceManager.raceState = RaceManager.RaceStates.Won;
                }    
            }

            if (GameManager.Instance.currentStar != null && m_kingBobOmb != null)
            {
                if (m_kingBobOmb.stage == 0 && GameManager.Instance.currentStar.name == "Star 1")
                {
                    m_textbox.StartDialogueSequence(m_kingBobOmbStartBattleDialogueSequence);    
                }
            }

            if (!GameManager.Instance.shownCastleIntroDialogue)
            {
                if (other.CompareTag("Castle Dialogue"))
                {
                    GameManager.Instance.shownCastleIntroDialogue = true;
                    m_textbox.StartDialogueSequence(GameManager.Instance.castleIntroDialogueSequence);

                }
            }
        }

        if (other.CompareTag("Enemy"))
        {
            Enemy _enemy = other.GetComponent<Enemy>();
            
            if (!isOnEnemy && state != States.Punch && state != States.Kick && state != States.Dive &&
                state != States.Jump && state != States.GroundPound)
            {
                TakeDamage(_enemy.damageAmount);
            }
            else
            {
                _enemy.TakeDamage(1);
            }
        }

        if (other.CompareTag("Death"))
        {
            Die();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Dialogue Hitbox"))
        {
            m_isCollidingWithDialogueHitbox = false;
        }
        
        if (other.CompareTag("King Bob Omb"))
        {
            isCollidingWithKingBobOmb = false;
            m_kingBobOmbObject = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Cannon"))
        {
            state = States.InsideCannon;
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
        
        float sineWaveValue = Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f;
        int baseFontSize = Mathf.RoundToInt(Screen.height * 0.035f);
        int dynamicFontSize = Mathf.RoundToInt(baseFontSize * (0.8f + sineWaveValue * 0.4f));

        GUIStyle m_bouncyText = new GUIStyle
        {
            fontSize = dynamicFontSize,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };


        if (GameManager.Instance.enableDebug)
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
            GUI.Label(new Rect(10, 960, 300, 40), "isCollidingWithPole: " + isCollidingWithPole, m_Style);
        }

        if (state == States.InsideCannon)
        {
            GUI.Label(new Rect(Screen.width - Screen.width * 0.6f, Screen.height * 0.75f, 300, 40), "Press *Jump* to launch yourself out of the cannon", m_bouncyText);
        }
    }
}