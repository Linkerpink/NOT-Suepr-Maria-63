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
    
    private enum States
    {
        Idle,
        Racing,
    }
    
    private States state = States.Idle;
    
    private void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        switch (state)
        {
            case States.Idle:
                break;
            
            case States.Racing:
                
                float _remainingDist = Vector3.Distance(transform.position, waypoints[m_currentWaypoint].position);
                
                print(_remainingDist);
                if (_remainingDist <= 1f  &&  m_currentWaypoint < waypoints.Length - 1)
                {
                    m_currentWaypoint++;
                    SetWaypoint();
                }
                
                /*
                if (transform.position == waypoints[m_currentWaypoint].position &&)
                {
                    print("ka");
                }
                */
                break;
        }
        
        print(m_currentWaypoint);
    }

    public void StartRace()
    {
        state = States.Racing;
        SetWaypoint();
    }

    private void SetWaypoint()
    {
        print("SET WAYPOINT 5 BIG BOOOOMB");
        m_agent.destination = waypoints[m_currentWaypoint].position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Top Hitbox") && state == States.Racing)
        {
            m_gameManager.SpawnStar(m_star, transform);
            state = States.Idle;
            gameObject.SetActive(false);
        }
    }
}
