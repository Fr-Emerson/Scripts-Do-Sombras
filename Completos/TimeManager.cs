using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance {get; set;}
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }
    public int dayInGame = 0;
    public int weekInGame = 1;
    public int getCurrentDay()
    {
        return dayInGame;
    }
    public void TriggerNextDay()
    { 
        dayInGame++;
    }
    public void TriggerNextweek()
    {
        weekInGame++;
    }
}
