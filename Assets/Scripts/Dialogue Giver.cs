using UnityEngine;

public class DialogueGiver : MonoBehaviour
{
    public DialogueSequence dialogueSequence;

    [SerializeField] private GameObject m_talkArrow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && m_talkArrow != null)
        {
            m_talkArrow.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && m_talkArrow != null)
        {
            m_talkArrow.SetActive(false);
        }
    }
}
