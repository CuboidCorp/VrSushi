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
        DontDestroyOnLoad(go);
        SceneManager.LoadScene("Sushi");
    }

    private void OnQuitGameButtonClicked()
    {
        Debug.Log("Quit Game button clicked.");
        Application.Quit();
    }
}
