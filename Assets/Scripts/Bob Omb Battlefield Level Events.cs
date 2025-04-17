using System;
using UnityEngine;

public class BobOmbBattlefieldLevelEvents : MonoBehaviour
{
    [SerializeField] private DialogueSequence m_dialogueSequence;

    private Textbox m_textbox;
    
    private GameManager m_gameManager;

    private Mario m_mario;
    
    private void Awake()
    {
        m_textbox = FindAnyObjectByType<Textbox>();
        m_gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        gameObject.SetActive(!m_gameManager.cannonsOpened);
    }

    public void TurnOffMarioMovement()
    {
        m_mario = FindObjectOfType<Mario>();
        m_mario.canMove = false;
    }

    public void TurnOnMarioMovement()
    {
        m_mario = FindObjectOfType<Mario>();
        m_mario.canMove = true;
    }

    public void StartDialogue()
    {
        m_mario = FindObjectOfType<Mario>();
        m_textbox.StartDialogueSequence(m_dialogueSequence);
        m_gameManager.cannonsOpened = true;
    }
}