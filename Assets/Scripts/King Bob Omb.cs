using UnityEngine;
using UnityEngine.AI;

public class KingBobOmb : MonoBehaviour
{
    public int stage = 1;

    public NavMeshAgent agent;
    private Mario m_mario;
    private Transform m_marioTransform;
    private GameManager m_gameManager;
    
    private Animator m_animator;
    
    private bool isGrounded = false;

    [SerializeField] private LayerMask groundLayer, playerLayer;

    // Patroling
    [SerializeField] private Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField] private float walkPointRange;

    // Speeds
    [SerializeField] private float patrolSpeed = 2;
    [SerializeField] private float runSpeed = 3;
    
    // Timers
    private float setWalkPointTimer;
    private float setWalkPointTimerDuration = 1;
    private float goToWalkTimer;
    private float goToWalkTimerDuration = 2;

    public enum States
    {
        Idle,
        Walking,
        Grabbing,
        Throwing,
        Grabbed,
    }

    public States state = States.Walking;

    private void Awake()
    {
        m_mario = FindAnyObjectByType<Mario>();
        m_marioTransform = m_mario.transform;
        agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponentInChildren<Animator>();
        m_gameManager = FindAnyObjectByType<GameManager>();
    }
    
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f, groundLayer);
        
        switch (state)
        {
            case States.Idle:
                if (goToWalkTimer > 0)
                {
                    goToWalkTimer -= Time.deltaTime;
                }
                else
                {
                    state = States.Walking;
                }
                break;
                
            case States.Walking:
                agent.SetDestination(walkPoint);

                if (setWalkPointTimer > 0)
                {
                    setWalkPointTimer -= Time.deltaTime;
                }
                else
                {
                    SetWalkPoint(m_marioTransform.position);
                    setWalkPointTimer = setWalkPointTimerDuration;
                }

                if (stage < 2)
                {
                    agent.angularSpeed = 75;
                }
                else
                {
                    agent.angularSpeed = 180;
                    setWalkPointTimerDuration = 0.25f;
                }
                break;
            
            case States.Grabbed:
                Vector3 _marioTransform = m_marioTransform.position;
                transform.position = new Vector3(_marioTransform.x, _marioTransform.y + 2,_marioTransform.z);
                
                if (isGrounded)
                {
                    state = States.Idle;
                    goToWalkTimer = goToWalkTimerDuration;
                }
                break;
        }
    }

    private void SetWalkPoint(Vector3 _walkPoint)
    {
        walkPoint = _walkPoint;
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
            GUI.Label(new Rect(Screen.width - 400, 10, 300, 40), "state: " + state, m_Style);
        }
    }
}