using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nbClients;
    [SerializeField] private TMP_Text nbSatisfied;
    [SerializeField] private TMP_Text nbUnsatisfied;
    [SerializeField] private TMP_Text nbUnserved;
    [SerializeField] private TMP_Text wastedIngredients;
    [SerializeField] private TMP_Text score;
    [SerializeField] private Button nextDayBtn;

    private void Start()
    {
        BonusMalusUI.Instance.onChoicesConfirmed.AddListener(ShowEndMenu);

        nextDayBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            GameData.Instance.StartNewDay();
        });
    }

    private void ShowEndMenu(EndBonus bonus, EndMalus malus)
    {
        Debug.Log("Day ended. Showing end menu...");
        WasteManager.Instance.SetWaste();
        transform.GetChild(0).gameObject.SetActive(true);
        DayStats dayStats = DayManager.Instance.dayStats;
        nbClients.text = dayStats.totalClients.ToString();
        nbSatisfied.text = dayStats.satisfiedClients.ToString();
        nbUnsatisfied.text = dayStats.unsatisfiedClients.ToString();
        nbUnserved.text = dayStats.notServedClients.ToString();
        wastedIngredients.text = dayStats.wastedIngredients.ToString();
        score.text = dayStats.GetScore().ToString();
    }
}
