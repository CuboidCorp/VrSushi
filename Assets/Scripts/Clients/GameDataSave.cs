using System;
using System.Collections.Generic;

[Serializable]
public class GameDataSave
{
    public int currentDay;
    public List<DayStats> dayStats;
    public int nbClients;
    public int nbClientsPremium;

    public float clientWaitTimeMultiplier;
    public float knifeDamageMultiplier;
    public float fishLifeMultiplier;
    public float overcookTimeMultiplier;
    public float cookingSpeedMultiplier;
}
