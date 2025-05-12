using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DayManager))]
public class DayManagerEditor : Editor
{
    // Hardcoded time range (10:00 to 22:00)
    private const int startHour = 10;
    private const int endHour = 22;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DayManager dayManager = (DayManager)target;

        if (dayManager.rushHourCurve == null)
            return;

        int totalMinutes = (endHour - startHour) * 60;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rush Hour Curve Time Mapping", EditorStyles.boldLabel);

        foreach (var key in dayManager.rushHourCurve.keys)
        {
            float percent = Mathf.Clamp01(key.time);
            int totalMins = Mathf.RoundToInt(percent * totalMinutes);
            int hour = startHour + (totalMins / 60);
            int minute = totalMins % 60;

            string formattedTime = $"{hour:00}:{minute:00}";
            EditorGUILayout.LabelField($"Key @ {key.time:F2} → {formattedTime} (Multiplier: {key.value:F2})");
        }
    }
}
