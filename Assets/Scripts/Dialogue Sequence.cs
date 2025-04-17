using System;
using UnityEngine;

[CreateAssetMenu]
public class DialogueSequence : ScriptableObject
{
    public string[] dialogue;

    public Color textBoxColor;
    public Color textColor;

    public enum DialogueTypes
    {
        Nothing,
        KingBobOmbStartFight,
        KingBobOmbEndFight,
        KoopaTheQuickStartRace,
        KoopaTheQuickWinRace,
        KoopaTheQuickLoseRace,
        PinkBobOmbOpenCannon,
    }
    
    public DialogueTypes dialogueType;

    private Mario m_mario;
    
    private GameManager m_gameManager;

    private void Awake()
    {
        m_gameManager = FindObjectOfType<GameManager>();
    }

    public void EndDialogueFunction()
    {
        switch (dialogueType)
        {
            case DialogueTypes.KingBobOmbStartFight:
                KingBobOmb _kingBobOmb = FindAnyObjectByType<KingBobOmb>();
                _kingBobOmb.StartFight();
                break;
            
            case DialogueTypes.KingBobOmbEndFight:
                _kingBobOmb = FindAnyObjectByType<KingBobOmb>();
                _kingBobOmb.EndFight();
                break;
            
            case DialogueTypes.KoopaTheQuickStartRace:
                KoopaTheQuick _koopaTheQuick = FindAnyObjectByType<KoopaTheQuick>();
                _koopaTheQuick.StartRace();
                break;
            
            case DialogueTypes.KoopaTheQuickWinRace:
                _koopaTheQuick = FindAnyObjectByType<KoopaTheQuick>();
                _koopaTheQuick.WinRace();
                break;
            
            case DialogueTypes.KoopaTheQuickLoseRace:
                m_mario = FindObjectOfType<Mario>();
                m_mario.Die();
                break;
            
            case DialogueTypes.PinkBobOmbOpenCannon:
                // Open Cannons
                m_gameManager = FindAnyObjectByType<GameManager>();
                if (!GameManager.Instance.cannonsOpened)
                {
                    m_mario = FindObjectOfType<Mario>();
                    Animator m_animator = GameObject.Find("Cannon Covers").GetComponent<Animator>();
                    m_animator.SetTrigger("openCannon");
                    m_mario.canMove = false;
                }
                break;
        }
    }
}
