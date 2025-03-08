using UnityEngine;

public class FollowObject : MonoBehaviour
{
    private GameObject m_mario;

    private void Awake()
    {
        m_mario = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        transform.position = m_mario.transform.position;
    }
}