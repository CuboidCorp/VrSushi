using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class RecipeStepPopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject singleInputGo;
    [SerializeField] private GameObject multiInputGo1;
    [SerializeField] private GameObject multiInputGo2;

    [SerializeField] private GameObject arrow1Go;

    [SerializeField] private GameObject transformationGo;

    [SerializeField] private GameObject arrow2Go;

    [SerializeField] private GameObject resultGo;

    [SerializeField] private Button closeButton;

    [SerializeField] private LocalizeStringEvent descText;

    [Header("Sprites")]
    [SerializeField, Tooltip("Sprites of all spawn locations")] private Sprite[] spawnLocations;

    [SerializeField, Tooltip("Sprite of ustensils")] private Sprite[] utensils;
    [SerializeField, Tooltip("Arrow sprite")] private Sprite arrowSprite;

    [Header("Localization")]
    [SerializeField] private LocalizedString[] spawnNames;

    private RecipeStep currentStep;

    public void Show(RecipeStep step)
    {
        gameObject.SetActive(true);
        closeButton.onClick.AddListener(Hide);

        switch (step.inputItems.Count)
        {
            case 0:
                singleInputGo.SetActive(false);
                multiInputGo1.SetActive(false);
                multiInputGo2.SetActive(false);
                break;
            case 1:
                singleInputGo.SetActive(true);
                SetKitchenInfo(singleInputGo, step.inputItems[0]);
                multiInputGo1.SetActive(false);
                multiInputGo2.SetActive(false);
                break;
            case 2:
                singleInputGo.SetActive(false);
                multiInputGo1.SetActive(true);
                multiInputGo2.SetActive(true);
                SetKitchenInfo(multiInputGo1, step.inputItems[0]);
                SetKitchenInfo(multiInputGo2, step.inputItems[1]);
                break;
        }

        if (step.method == ObtentionMethod.COMBINE)
        {
            arrow1Go.SetActive(false);
            arrow2Go.SetActive(false);
            transformationGo.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = arrowSprite;
        }
        else
        {
            if (step.method == ObtentionMethod.SPAWN)
                arrow1Go.SetActive(false);
            else
                arrow1Go.SetActive(true);
            arrow2Go.SetActive(true);
            transformationGo.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = GetSprite(step.method, step.spawnLocation);

        }
        transformationGo.GetComponentInChildren<LocalizeStringEvent>().StringReference = spawnNames[(int)step.method];


        resultGo.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = step.resultItem.icon;
        resultGo.GetComponentInChildren<LocalizeStringEvent>().StringReference = step.resultItem.itemNameLocalized;

        descText.StringReference = step.stepDescriptionLocalized;

    }

    private static void SetKitchenInfo(GameObject go, KitchenItem item)
    {
        go.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = item.icon;
        go.GetComponentInChildren<LocalizeStringEvent>().StringReference = item.itemNameLocalized;
    }

    public void Hide()
    {
        closeButton.onClick.RemoveListener(Hide);
        gameObject.SetActive(false);
    }

    private Sprite GetSprite(ObtentionMethod method, SpawnLocation? spawn)
    {
        if (method == ObtentionMethod.SPAWN)
            return spawnLocations[(int)spawn];
        return utensils[(int)method];
    }
}
