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

    //NYI
    public int nbClientsPremium = 0;

    [Header("Multipliers")]
    public float clientWaitTimeMultiplier = 1f;
    public float knifeDamageMultiplier = 1f;
    public float fishLifeMultiplier = 1f;

    public float overcookTimeMultiplier = 1f;
    public float cookingSpeedMultiplier = 1f;

    //NYI
    public bool stoveFailure = false;


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

                stoveFailure = gameDataSave.stoveFailure;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BonusMalusUI.Instance.onChoicesConfirmed.AddListener(OnDayEnded);
    }

    private void OnDayEnded(EndBonus bonus, EndMalus malus)
    {
        dayStats.Add(DayManager.Instance.dayStats);

        HandleBonus(bonus);
        HandleMalus(malus);

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
            cookingSpeedMultiplier = cookingSpeedMultiplier,
            stoveFailure = stoveFailure
        });
    }

    private void HandleBonus(EndBonus bonus)
    {
        switch (bonus)
        {
            case EndBonus.MORE_CLIENT_WAIT_TIME:
                clientWaitTimeMultiplier += 0.05f;
                break;
            case EndBonus.BETTER_KNIFE:
                knifeDamageMultiplier += 0.05f;
                break;
            case EndBonus.MORE_OVERCOOK_TIME:
                overcookTimeMultiplier += 0.1f;
                break;
            case EndBonus.FASTER_COOKING:
                cookingSpeedMultiplier += 0.05f;
                break;
        }
    }

    private void HandleMalus(EndMalus malus)
    {
        switch (malus)
        {
            case EndMalus.MORE_CLIENTS:
                nbClients += 2;
                break;
            case EndMalus.STRONGER_FISHES:
                fishLifeMultiplier += 0.1f;
                break;
            case EndMalus.LESS_OVERCOOK_TIME:
                overcookTimeMultiplier -= 0.1f;
                if (overcookTimeMultiplier < 0.1f) overcookTimeMultiplier = 0.1f; // Prevent negative overcook time
                break;
            case EndMalus.RUSHED_CLIENT:
                nbClientsPremium += 1;
                break;
            case EndMalus.PLATE_MALFUNCTION:
                stoveFailure = true;
                break;

        }
    }

    public void StartNewDay()
    {
        currentDay++;
        SceneManager.LoadScene("Sushi");
    }
}
