using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Textbox : MonoBehaviour
{
    private Animator m_animator;

    private int m_page = 0;

    private Image m_background;
    [SerializeField] TextMeshProUGUI m_text;
    
    private DialogueSequence m_sequence;
    
    private bool m_textboxExists = false;

    private float m_texboxDissapearTimer;
    private float m_texboxDissapearTimerDuration = 0.25f;

    private Mario m_mario;
    
    // GA WERKEN MET DIE TIMERS
    
    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_background = GetComponentInChildren<Image>();
        
        m_mario = FindAnyObjectByType<Mario>();
    }

    private void Update()
    {
        if (!m_textboxExists)
        {
            m_background.color = Color.clear;
            m_text.color = Color.clear;
        }
    }

    public void StartDialogueSequence(DialogueSequence _sequence)
    {
        if (!m_textboxExists)
        {
            m_mario.canMove = false;
            m_textboxExists = true;
            m_sequence = _sequence;
        
            ResetTextbox();
        
            SpawnTextbox(m_sequence.dialogue[m_page]);    
        }
    }
    
    private void SpawnTextbox(string _text)
    {
        m_background.color = m_sequence.textBoxColor;
        m_text.color = m_sequence.textColor;
        
        m_text.SetText(_text);
        
        m_animator.SetTrigger("textboxIn");
    }

    public void OnSubmit(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_sequence != null)
        {
            print("submit");
            if (m_page < m_sequence.dialogue.Length - 1 && m_textboxExists)
            {
                m_page++;
                SpawnTextbox(m_sequence.dialogue[m_page]);
            }
            else if (m_textboxExists)
            {
                m_sequence.EndDialogueFunction();
                m_animator.SetTrigger("textboxOut");
                m_textboxExists = false;
                m_mario.canMove = true;
            }
        }
    }

    private void ResetTextbox()
    {
        m_page = 0;
    }
}
