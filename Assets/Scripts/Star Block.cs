using System;
using UnityEngine;

public class StarBlock : MonoBehaviour
{
    [SerializeField] private Star m_star;

    private GameManager m_gameManager;

    private Mario m_mario;

    private void Awake()
    {
        m_gameManager = FindObjectOfType<GameManager>();
        m_mario = FindObjectOfType<Mario>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && m_mario.state == Mario.States.Jump)
        {
            GameManager.Instance.SpawnStar(m_star, transform.position);
            gameObject.SetActive(false);
        }
    }
}