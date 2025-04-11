using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int hp = 1;
    public int damageAmount = 1;

    public NavMeshAgent agent;
    private Mario m_mario;
    private Transform m_marioTransform;
    
    private Animator m_animator;

    [SerializeField] private LayerMask groundLayer, playerLayer;

    // Patroling
    [SerializeField] private Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField] private float walkPointRange;
    
    // Speeds
    [SerializeField] private float patrolSpeed = 2;
    [SerializeField] private float runSpeed = 3;

    // States
    [SerializeField] private float sightRange;
    [SerializeField] private bool playerInSightRange, playerInAttackRange;
    
    public enum States
    {
        Patroling,
        ChargePlayer,
    }

    public States state = States.Patroling;

    private void Awake()
    {
        m_mario = FindAnyObjectByType<Mario>();
        m_marioTransform = m_mario.transform;
        agent = GetComponent<NavMeshAgent>();
        m_animator = GetComponentInChildren<Animator>();
    }
    
    private void Update()
    {
        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);

        if (!playerInSightRange)
        {
            state = States.Patroling;
        }

        if (playerInSightRange)
        {
            state = States.ChargePlayer;
        }
        
        // State switching
        switch (state)
        {
            // Patroling
            case States.Patroling:
                
                if (!walkPointSet)
                {
                    SearchWalkPoint();
                }

                if (walkPointSet)
                {
                    agent.SetDestination(walkPoint);
                }

                Vector3 distanceToWalkPoint = transform.position - walkPoint;

                // Walkpoint reached
                if (distanceToWalkPoint.magnitude < 1f)
                {
                    walkPointSet = false;
                }
                
                if (m_mario.canMove)
                {
                    agent.speed = patrolSpeed;
                }
                else
                {
                    agent.speed = 0;
                }
                
                break;
            
            // Carge Player
            case States.ChargePlayer:
                
                if (m_mario.canMove)
                {
                    agent.SetDestination(m_marioTransform.position);
                    agent.speed = runSpeed;    
                }
                else
                {
                    agent.speed = 0;
                }
                
                break;
        }
    }

    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
        {
            walkPointSet = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
    }

    private void DamagePlayer(int _damageAmount)
    {
        m_mario.TakeDamage(_damageAmount);
    }

    public void TakeDamage(int _damageAmount)
    {
        hp -= _damageAmount;

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if (_collision.gameObject.CompareTag("Player"))
        {
            DamagePlayer(1);
        }
    }
}