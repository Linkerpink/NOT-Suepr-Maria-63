using System;
using TMPro;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public float raceTime;

    public bool racing = false;

    [SerializeField] private TextMeshProUGUI m_raceTimerText;

    public enum RaceStates
    {
        Nothing,
        Racing,
        Won,
        Lost
    }
    
    public RaceStates raceState;
    
    private void Update()
    {
        print(raceState);
        if (racing)
        {
            raceTime += Time.deltaTime;
            m_raceTimerText.SetText("TIME: " + raceTime.ToString("N"));
        }
    }

    public void StartRace()
    {
        racing = true;
        raceState = RaceStates.Racing;
    }

    public void EndRace()
    {
        racing = false;
    }
}