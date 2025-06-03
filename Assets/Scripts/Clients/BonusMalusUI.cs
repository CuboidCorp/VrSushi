using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class BonusMalusUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform bonusContainer;
    [SerializeField] private Transform malusContainer;
    [SerializeField] private GameObject choicePrefab;
    public Button confirmButton;

    [Header("Localized Descriptions")]
    [SerializeField] private LocalizedString[] bonusDescriptions;
    [SerializeField] private LocalizedString[] malusDescriptions;

    [Header("Icons")]
    [SerializeField] private Sprite[] bonusIcons;
    [SerializeField] private Sprite[] malusIcons;

    private EndBonus selectedBonus;
    private EndMalus selectedMalus;
    private Button selectedBonusButton;
    private Button selectedMalusButton;
    private bool bonusChosen = false;
    private bool malusChosen = false;

    public void ShowChoices()
    {
        bonusChosen = false;
        malusChosen = false;

        ShowBonusChoices();
        ShowMalusChoices();
    }

    private void ShowBonusChoices()
    {
        var bonusValues = System.Enum.GetValues(typeof(EndBonus)).Cast<EndBonus>().OrderBy(_ => Random.value).Take(3);

        foreach (var bonus in bonusValues)
        {
            var index = (int)bonus;
            var go = Instantiate(choicePrefab, bonusContainer);
            var button = go.GetComponent<Button>();
            SetupChoiceUI(go, bonusIcons[index], bonusDescriptions[index], () =>
            {
                // Unselect previous
                if (selectedBonusButton != null)
                    SetButtonHighlight(selectedBonusButton, false);

                // Update selection
                selectedBonus = bonus;
                selectedBonusButton = button;
                SetButtonHighlight(button, true);

                UpdateConfirmInteractable();
            });
        }
    }


    private void ShowMalusChoices()
    {
        var malusValues = System.Enum.GetValues(typeof(EndMalus)).Cast<EndMalus>().ToList();

        var selectedMaluses = new List<EndMalus> { EndMalus.MORE_CLIENTS };
        var others = malusValues.Where(m => m != EndMalus.MORE_CLIENTS).OrderBy(_ => Random.value).Take(2);
        selectedMaluses.AddRange(others);

        foreach (var malus in selectedMaluses.OrderBy(_ => Random.value))
        {
            var index = (int)malus;
            var go = Instantiate(choicePrefab, malusContainer);
            var button = go.GetComponent<Button>();
            SetupChoiceUI(go, malusIcons[index], malusDescriptions[index], () =>
            {
                if (selectedMalusButton != null)
                    SetButtonHighlight(selectedMalusButton, false);

                selectedMalus = malus;
                selectedMalusButton = button;
                SetButtonHighlight(button, true);

                UpdateConfirmInteractable();
            });
        }
    }

    private void SetButtonHighlight(Button button, bool selected)
    {
        var colors = button.colors;
        colors.normalColor = selected ? Color.green : Color.white;
        button.colors = colors;
    }

    private void UpdateConfirmInteractable()
    {
        confirmButton.interactable = bonusChosen && malusChosen;
    }

    private void SetupChoiceUI(GameObject go, Sprite icon, LocalizedString description, UnityEngine.Events.UnityAction onClick)
    {
        go.transform.Find("Icon").GetComponent<Image>().sprite = icon;
        LocalizeStringEvent descHolder = go.transform.GetComponentInChildren<LocalizeStringEvent>();
        descHolder.StringReference = description;
        go.GetComponent<Button>().onClick.AddListener(onClick);
    }

    public EndBonus GetSelectedBonus()
    {
        return selectedBonus;
    }

    public EndMalus GetSelectedMalus()
    {
        return selectedMalus;
    }

}
