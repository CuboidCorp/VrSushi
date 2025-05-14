using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nbClients;
    [SerializeField] private TMP_Text nbSatisfied;
    [SerializeField] private TMP_Text nbUnsatisfied;
    [SerializeField] private TMP_Text nbUnserved;
    [SerializeField] private TMP_Text totalSatisfaction;


    private void Start()
    {
        DayManager.Instance.OnDayEnd.AddListener(ShowEndMenu);
    }

    private void ShowEndMenu()
    {
        gameObject.SetActive(true);
        DayStats dayStats = DayManager.Instance.dayStats;
        nbClients.text = dayStats.totalClients.ToString();
        nbSatisfied.text = dayStats.satisfiedClients.ToString();
        nbUnsatisfied.text = dayStats.unsatisfiedClients.ToString();
        nbUnserved.text = dayStats.notServedClients.ToString();
        totalSatisfaction.text = dayStats.AverageSatisfaction.ToString("F2");

    }

    /// <summary>
    /// Redemarre le jeu, pour le moment, il ne fait que recharger la scene actuelle.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restarting game... ");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
