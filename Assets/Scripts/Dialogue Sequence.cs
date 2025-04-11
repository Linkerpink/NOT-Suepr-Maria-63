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
    }
    
    public DialogueTypes dialogueType;

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
                Mario _mario = FindAnyObjectByType<Mario>();
                _mario.Die();
                break;
        }
    }
}
