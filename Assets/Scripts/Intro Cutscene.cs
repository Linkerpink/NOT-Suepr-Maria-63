using UnityEngine;

public class IntroCutscene : MonoBehaviour
{
    private Mario m_mario;
    private Textbox m_textbox;

    [SerializeField] private DialogueSequence m_introDialogueSequence;
    
    private void Awake()
    {
        m_mario = FindObjectOfType<Mario>();
        m_textbox = FindObjectOfType<Textbox>();
    }
    
    public void DisableMario()
    {
        m_mario.canMove = false;
    }
    
    public void EnableMario()
    {
        m_mario.canMove = true;
        
        m_textbox.StartDialogueSequence(m_introDialogueSequence);
        
        gameObject.SetActive(false);
    }
}
