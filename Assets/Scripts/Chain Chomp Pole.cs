using UnityEngine;

public class ChainChompPole : MonoBehaviour
{
    [SerializeField] private int m_pos = 0;

    private float m_yPos = 0;

    [SerializeField] private float[] m_yPositions;
    
    [SerializeField] Animator m_chainChompAreaAnimator;
    
    private void Update()
    {
        transform.position = new Vector3(transform.position.x, m_yPos, transform.position.z);
        
        switch (m_pos)
        {
            case 0:
                m_yPos = m_yPositions[0];
                break;
            case 1:
                m_yPos = m_yPositions[1];
                break;
            case 2:
                m_yPos = m_yPositions[2];
                m_chainChompAreaAnimator.SetTrigger("breakGate");
                gameObject.SetActive(false);
                break;
        }
    }

    public void IncreasePolePosition()
    {
        if (m_pos < 2)
        { 
            m_pos++;
        }
    }
}
