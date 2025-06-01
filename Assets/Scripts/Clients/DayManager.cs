using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Settings")]
    public float dayDurationInSeconds = 600f; // 10 minutes
    public AnimationCurve rushHourCurve; // Customizable via Unity Editor (time of day vs intensity)


    [Header("Daylight Settings")]
    public Light directionalLight; // Assign your sun light in the Inspector
    public Gradient lightColorOverTime; // Assign color gradient (morning -> noon -> evening)
    public AnimationCurve lightIntensityOverTime; // Optional: Vary intensity with time
    public Vector3 sunriseRotation = new Vector3(15f, 0f, 0f);
    public Vector3 sunsetRotation = new Vector3(170f, 0f, 0f);

    private float dayTimer = 0f;

    public DayStats dayStats = new();

    [HideInInspector] public UnityEvent OnDayEnd;

    public float CurrentTimePercent => Mathf.Clamp01(dayTimer / dayDurationInSeconds);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dayStats = new DayStats();
        // Initialize the day
        StartDay();
    }

    public void StartDay()
    {
        dayTimer = 0f;
        StartCoroutine(DayRoutine());
    }

    private IEnumerator DayRoutine()
    {
        while (dayTimer < dayDurationInSeconds)
        {
            dayTimer += Time.deltaTime;

            float t = CurrentTimePercent;

            // Rotate the sun from sunrise to sunset
            directionalLight.transform.rotation = Quaternion.Lerp(
                Quaternion.Euler(sunriseRotation),
                Quaternion.Euler(sunsetRotation),
                t
            );

            // Change sun color over time
            directionalLight.color = lightColorOverTime.Evaluate(t);

            // Adjust sun intensity
            directionalLight.intensity = lightIntensityOverTime.Evaluate(t);

            yield return null;
        }
    }


    public float GetClientSpawnMultiplier()
    {
        // Use a curve that returns a multiplier based on time of day
        return rushHourCurve.Evaluate(CurrentTimePercent);
    }

    public void DayEnd()
    {
        OnDayEnd?.Invoke();
        dayStats.PrintSummary();
    }

}
