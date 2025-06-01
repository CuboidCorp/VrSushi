using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public int currentDay = 1;
    public List<DayStats> dayStats = new();

    public int nbClients = 10;

    [Header("Multipliers")]
    public float clientWaitTimeMultiplier = 1f;
    public float knifeDamageMultiplier = 1f;
    public float fishLifeMultiplier = 1f;

    public float overcookTimeMultiplier = 1f;
    public float cookingSpeedMultiplier = 1f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DayManager.Instance.OnDayEnd.AddListener(OnDayEnded);
    }

    private void OnDayEnded()
    {
        dayStats.Add(DayManager.Instance.dayStats);
    }

    public void StartNewDay()
    {
        currentDay++;
        SceneManager.LoadScene("Sushi");
    }
}
