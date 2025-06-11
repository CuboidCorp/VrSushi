using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitGameButton;

    private void Start()
    {
        if (SaveManager.SaveExists())
        {
            continueButton.interactable = true;
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        else
        {
            continueButton.interactable = false;
        }

        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        quitGameButton.onClick.AddListener(OnQuitGameButtonClicked);
    }

    private void OnContinueButtonClicked()
    {
        GameObject go = new("NewGame");
        go.AddComponent<NewGame>().isNewGame = false;
        DontDestroyOnLoad(go);
        SceneManager.LoadScene("Sushi");
    }

    private void OnNewGameButtonClicked()
    {
        GameObject go = new("NewGame");
        go.AddComponent<NewGame>().isNewGame = true;

        //On check si y a game data si oui on le supprime
        if (GameData.Instance != null)
        {
            Destroy(GameData.Instance.gameObject);
        }

        DontDestroyOnLoad(go);
        SceneManager.LoadScene("Sushi");
    }

    private void OnQuitGameButtonClicked()
    {
        Debug.Log("Quit Game button clicked.");
        Application.Quit();
    }
}
