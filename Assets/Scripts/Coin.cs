using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private Star m_100CoinStar;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mario m_mario = other.GetComponent<Mario>();

            if (m_mario != null)
            {
                m_mario.Heal(1);
            }

            GameManager.Instance.coinsCollected++;
            
            if (GameManager.Instance.coinsCollected == 100 && !GameManager.Instance.spawned100CoinStar)
            {
                GameManager.Instance.spawned100CoinStar = true;
                GameManager.Instance.SpawnStar(m_100CoinStar, new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z));
            }
            
            gameObject.SetActive(false);
        }
    }
}
