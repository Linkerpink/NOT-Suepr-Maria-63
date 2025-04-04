using UnityEngine;
using UnityEngine.UI;

public class PowerMeter : MonoBehaviour
{
    private Animator m_animator;

    private Mario m_mario;

    private int hp;

    public bool showing = false;
    
    [SerializeField] private Texture[] powerMeterSprites;
    private RawImage powerMeterImage;

    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();

        m_mario = FindAnyObjectByType<Mario>();
        
        powerMeterImage = GetComponentInChildren<RawImage>();
    }

    private void Update()
    {
        hp = m_mario.hp;
        
        switch (hp)
        {
            case 0:
                powerMeterImage.texture = powerMeterSprites[0];
                break;
            case 1:
                powerMeterImage.texture = powerMeterSprites[1];
                break;
            case 2:
                powerMeterImage.texture = powerMeterSprites[2];
                break;
            case 3:
                powerMeterImage.texture = powerMeterSprites[3];
                break;
            case 4:
                powerMeterImage.texture = powerMeterSprites[4];
                break;
            case 5:
                powerMeterImage.texture = powerMeterSprites[5];
                break;
            case 6:
                powerMeterImage.texture = powerMeterSprites[6];
                break;
            case 7:
                powerMeterImage.texture = powerMeterSprites[7];
                break;
            case 8:
                powerMeterImage.texture = powerMeterSprites[8];
                break;
        }
    }

    public void ShowPowerMeter()
    {
        m_animator.SetTrigger("show");
        showing = true;
    }

    public void HidePowerMeter()
    {
        m_animator.SetTrigger("hide");
        showing = false;
    }
}