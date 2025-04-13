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
        KoopaTheQuickStartRace,
        KoopaTheQuickWinRace,
        KoopaTheQuickLoseRace,
        PinkBobOmbOpenCannon,
    }
    
    public DialogueTypes dialogueType;

    private Mario m_mario;

    public void EndDialogueFunction()
    {
        switch (dialogueType)
        {
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
                GameManager _gameManager = FindAnyObjectByType<GameManager>();
                if (!_gameManager.cannonsOpened)
                {
                    m_mario = FindObjectOfType<Mario>();
                    Animator m_animator = FindAnyObjectByType<BobOmbBattlefieldLevelEvents>().GetComponent<Animator>();
                    m_animator.SetTrigger("openCannon");
                    m_mario.canMove = false;
                }
                break;
        }
    }
}
