using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
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
            
            gameObject.SetActive(false);
        }
    }
}
