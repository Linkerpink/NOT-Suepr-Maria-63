using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Mario : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    public Transform cameraTransform;
    public LayerMask groundLayer;

    private float moveSpeed = 0f;
    [SerializeField] private float maxMoveSpeed = 6.0f;
    private float maxWalkMoveSpeed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    
    [SerializeField] private float jumpForce = 8f;
    private int jumpCount = 0;
    private float jumpTimer = 0f;
    [SerializeField] private float jumpTimerDuration = 1f;
    
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private bool isGrounded;

    private Vector3 moveDirection;
    private Vector2 inputDirection;
    private Vector3 jumpDirection;
    private GameManager m_gameManager;

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
    }

    private States state = States.Idle;
    
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
        
        if (inputDirection.sqrMagnitude > 0.01f) 
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

        if (state != States.Jump || !isGrounded)
        {
            if (inputDirection.sqrMagnitude >= 0.25f)
            {
                state = States.Run;
            }
            else if (inputDirection.sqrMagnitude >= 0.01f)
            {
                state = States.Walk;
            }
            else
            {
                state = States.Idle;
            }    
        }

        if (!isGrounded)
        {
            state = States.Jump;
        }
        
        switch (state)
        {
            case States.Idle:
                moveSpeed = Mathf.Max(moveSpeed - deceleration * Time.deltaTime, 0);
            
                break;
            
            case States.Walk:
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxWalkMoveSpeed);
                
                break;
            
            case States.Run:
                
                moveSpeed = Mathf.Min(moveSpeed + acceleration * Time.deltaTime, maxMoveSpeed);
                
                break;
            
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
                    state = States.Idle;
                }
                
                break;
        }
        
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        if (moveDirection != Vector3.zero) 
        {
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        
        Vector3 velocity = moveDirection.normalized * moveSpeed;
        
        m_rigidbody.linearVelocity = new Vector3(velocity.x, m_rigidbody.linearVelocity.y, velocity.z);

        if (jumpTimer > 0 && isGrounded)
        {
            jumpTimer -= Time.deltaTime;
        }

        if (jumpTimer <= 0)
        {
            jumpCount = 0;
        }
        
        m_animator.SetInteger("jumpCount", jumpCount);
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && context.performed && state != States.Jump)
        {
            if (jumpCount < 3)
            {
                jumpCount++;                
            }
            
            float _jumpForce = jumpForce + jumpCount;
            
            //m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, _jumpForce, m_rigidbody.linearVelocity.z);
            state = States.Jump;
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
        }
    }
}
