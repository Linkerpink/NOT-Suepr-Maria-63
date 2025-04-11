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
        }
    }
}
