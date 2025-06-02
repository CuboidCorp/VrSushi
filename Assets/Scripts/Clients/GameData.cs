using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    private GameDataSave gameDataSave;

    public int currentDay = 1;
    public List<DayStats> dayStats = new();

    public int nbClients = 10;

    public int nbClientsPremium = 0;

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
            NewGame ng = FindAnyObjectByType<NewGame>();
            if (ng != null && !ng.isNewGame && SaveManager.SaveExists())
            {
                gameDataSave = SaveManager.LoadGame();
                currentDay = gameDataSave.currentDay;
                dayStats = gameDataSave.dayStats;
                nbClients = gameDataSave.nbClients;
                nbClientsPremium = gameDataSave.nbClientsPremium;

                clientWaitTimeMultiplier = gameDataSave.clientWaitTimeMultiplier;
                knifeDamageMultiplier = gameDataSave.knifeDamageMultiplier;
                fishLifeMultiplier = gameDataSave.fishLifeMultiplier;
                overcookTimeMultiplier = gameDataSave.overcookTimeMultiplier;
                cookingSpeedMultiplier = gameDataSave.cookingSpeedMultiplier;
            }
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

        //On fait choisir bonus/malus au joueur

        SaveManager.SaveGame(new GameDataSave
        {
            currentDay = currentDay,
            dayStats = dayStats,
            nbClients = nbClients,
            nbClientsPremium = nbClientsPremium,
            clientWaitTimeMultiplier = clientWaitTimeMultiplier,
            knifeDamageMultiplier = knifeDamageMultiplier,
            fishLifeMultiplier = fishLifeMultiplier,
            overcookTimeMultiplier = overcookTimeMultiplier,
            cookingSpeedMultiplier = cookingSpeedMultiplier
        });
    }

    public void StartNewDay()
    {
        currentDay++;
        SceneManager.LoadScene("Sushi");
    }
}
