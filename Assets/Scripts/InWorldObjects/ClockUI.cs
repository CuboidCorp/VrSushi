using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    private DayManager dayManager;
    public TMP_Text clockText;

    [Header("Clock Settings")]
    public int startHour = 10;
    public int endHour = 22;

    private int totalMinutes;

    private void Start()
    {
        dayManager = DayManager.Instance;
        totalMinutes = (endHour - startHour) * 60;
    }

    private void Update()
    {
        if (dayManager == null || !dayManager.isActiveAndEnabled) return;

        float t = dayManager.CurrentTimePercent;
        int minutesPassed = Mathf.RoundToInt(t * totalMinutes);

        int currentHour = startHour + (minutesPassed / 60);
        int currentMinute = minutesPassed % 60;

        string formattedTime = $"{currentHour:00}:{currentMinute:00}";
        clockText.text = formattedTime;
    }
}
