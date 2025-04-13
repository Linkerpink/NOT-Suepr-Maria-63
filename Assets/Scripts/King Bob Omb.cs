using UnityEngine;
using UnityEngine.AI;

public class KingBobOmb : MonoBehaviour
{
    public int stage = 0;

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
    
    [SerializeField] private DialogueSequence m_loseDialogueSequence;
    private Textbox m_textbox;

    public enum States
    {
        Idle,
        Walking,
        Grabbing,
        Throwing,
        Grabbed,
        Thrown,
    }

    public States state = States.Idle;

    [SerializeField] private Star m_kingBobOmbStar;

    private void Awake()
    {
        m_mario = FindAnyObjectByType<Mario>();
        m_marioTransform = m_mario.transform;
        agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponentInChildren<Animator>();
        m_gameManager = FindAnyObjectByType<GameManager>();
        m_textbox = FindAnyObjectByType<Textbox>();
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
                if (stage > 0 && stage < 3)
                {
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
                }
                else
                {
                    state = States.Idle;
                }
                break;
            
            case States.Grabbed:
                
                break;
            
            case States.Thrown:
                if (isGrounded)
                {
                    state = States.Walking;
                    stage++;
                    m_gameManager.ScreenShake(10,10,0.25f);
                }
                break;
        }

        if (stage > 3)
        {
            m_textbox.StartDialogueSequence(m_loseDialogueSequence);
            stage = -1;
        }
    }

    private void SetWalkPoint(Vector3 _walkPoint)
    {
        walkPoint = _walkPoint;
    }

    public void StartFight()
    {
        state = States.Walking;
        stage = 1;
    }

    public void EndFight()
    {
        m_gameManager.SpawnStar(m_kingBobOmbStar, new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z));
        gameObject.SetActive(false);
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
            GUI.Label(new Rect(Screen.width - 400, 60, 300, 40), "stage: " + stage, m_Style);
        }
    }
}