using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using XRMultiplayer;

public class DayStatsUI : MonoBehaviour
{
    [SerializeField] private Transform holder;

    [SerializeField] private TMP_Text dayText;

    [Header("Prefabs")]
    [SerializeField] private GameObject dayToggle;
    [SerializeField] private GameObject dayStatsList;

    private void Start()
    {
        dayText.text = GameData.Instance.currentDay.ToString();
        ShowAllDaysStats();
    }

    private void ShowAllDaysStats()
    {
        List<DayStats> dayStats = GameData.Instance.dayStats;
        foreach (DayStats stats in dayStats)
        {
            GameObject toggle = Instantiate(dayToggle, holder);
            toggle.name = $"Day{stats.day}Toggle";
            GameObjectToggle dayStatsToggle = toggle.GetComponent<GameObjectToggle>();
            LocalizeStringEvent localizeStringEvent = toggle.GetComponentInChildren<LocalizeStringEvent>();
            localizeStringEvent.StringReference.Arguments = new object[] { stats.day };

            GameObject statsList = Instantiate(dayStatsList, holder);
            statsList.name = $"Day{stats.day}Stats";
            dayStatsToggle.AddGameObject(statsList);

            DayStatsListUI statsListUI = statsList.GetComponent<DayStatsListUI>();
            statsListUI.nbClients.text = stats.totalClients.ToString();
            statsListUI.nbSatisfied.text = stats.satisfiedClients.ToString();
            statsListUI.nbUnsatisfied.text = stats.unsatisfiedClients.ToString();
            statsListUI.nbUnserved.text = stats.notServedClients.ToString();
            statsListUI.wastedIngredients.text = stats.wastedIngredients.ToString();
            statsListUI.score.text = stats.GetScore().ToString();
            statsList.SetActive(false);
        }
    }
}
