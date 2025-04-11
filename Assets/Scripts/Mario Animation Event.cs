using UnityEngine;

public class MarioAnimationEvent : MonoBehaviour
{
    private ChangeScene m_changeScene;

    private void Awake()
    {
        m_changeScene = GetComponent<ChangeScene>();
    }
    
    public void ChangeScene()
    {
        print("should change scene");
        m_changeScene.enabled = true;
        m_changeScene.StartTransition();
    }
}
