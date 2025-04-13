using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private string m_sceneName; // The scene to change to
    [SerializeField] private string m_transitionInAnimation;
    [SerializeField] private string m_transitionOutAnimation;
    [SerializeField] private float m_transitionDelay;
    private float m_transitionTimer;
    private bool m_startTransition = false;
    
    private GameManager m_gameManager;

    private void Awake()
    {
        m_gameManager = FindObjectOfType<GameManager>();
    }
    
    private void Update()
    {
        if (m_startTransition)
        {
            if (m_transitionTimer < m_transitionDelay)
            {
                m_transitionTimer += Time.deltaTime;
            }
            else
            {
                SceneManager.LoadScene(m_sceneName);    
            }
        }
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartTransition();
        }
    }

    public void StartTransition()
    {
        m_startTransition = true;
        m_gameManager.StartTransitionAnimation(m_transitionInAnimation);
        m_gameManager.transitionOutAnimation = m_transitionOutAnimation;
    }
}