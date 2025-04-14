using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class KoopaTheQuick : MonoBehaviour
{
    private NavMeshAgent m_agent;
    private Animator m_animator;
    
    [SerializeField] private Transform[] waypoints;
    private int m_currentWaypoint = 0;
    
    [SerializeField] private Star m_star;
    private GameManager m_gameManager;
    private RaceManager m_raceManager;
    
    private bool racing = false;
    
    private Textbox m_textbox;

    [SerializeField] private DialogueSequence m_lostDialogue;
    [SerializeField] private DialogueSequence m_wonDialogue;
    
    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
        
        m_gameManager = FindAnyObjectByType<GameManager>();
        m_textbox = FindAnyObjectByType<Textbox>();
    }
    
    private void Update()
    {
        print(racing);
        
        if (racing)
        {
            float _remainingDist = Vector3.Distance(transform.position, waypoints[m_currentWaypoint].position);
                
            if (_remainingDist <= 1f  &&  m_currentWaypoint < waypoints.Length - 1)
            {
                m_currentWaypoint++;
                SetWaypoint();
            }
    
        }
        
        print(m_currentWaypoint);
    }

    public void StartRace()
    {
        racing = true;
        SetWaypoint(); 
        m_raceManager= FindAnyObjectByType<RaceManager>();
        m_raceManager.StartRace();
    }

    private void SetWaypoint()
    {
        print("SET WAYPOINT 5 BIG BOOOOMB");
        m_agent.destination = waypoints[m_currentWaypoint].position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Top Hitbox"))
        {
            if (m_raceManager.raceState == RaceManager.RaceStates.Racing)
            {
                m_raceManager.raceState = RaceManager.RaceStates.Lost;
                
                m_textbox.StartDialogueSequence(m_lostDialogue);
            }
            
            if (m_raceManager.raceState == RaceManager.RaceStates.Won)
            {
                m_textbox.StartDialogueSequence(m_wonDialogue);
            }
        }
    }

    public void WinRace()
    {
        m_agent.enabled = false;
        print("race gewonnen :O");
        GameManager.Instance.SpawnStar(m_star, new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z));
        racing = false;
        gameObject.SetActive(false);
    }
}
