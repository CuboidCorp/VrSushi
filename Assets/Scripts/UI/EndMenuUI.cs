using TMPro;
using UnityEngine;

public class EndMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nbClients;
    [SerializeField] private TMP_Text nbSatisfied;
    [SerializeField] private TMP_Text nbUnsatisfied;
    [SerializeField] private TMP_Text nbUnserved;
    [SerializeField] private TMP_Text score;


    private void Start()
    {
        BonusMalusUI.Instance.onChoicesConfirmed.AddListener(ShowEndMenu);
    }

    private void ShowEndMenu(EndBonus bonus, EndMalus malus)
    {
        Debug.Log("Day ended. Showing end menu...");
        transform.GetChild(0).gameObject.SetActive(true);
        DayStats dayStats = DayManager.Instance.dayStats;
        nbClients.text = dayStats.totalClients.ToString();
        nbSatisfied.text = dayStats.satisfiedClients.ToString();
        nbUnsatisfied.text = dayStats.unsatisfiedClients.ToString();
        nbUnserved.text = dayStats.notServedClients.ToString();
        score.text = dayStats.GetScore().ToString();
    }
}
