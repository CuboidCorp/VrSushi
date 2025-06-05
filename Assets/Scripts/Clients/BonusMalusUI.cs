using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using XRMultiplayer;

public class BonusMalusUI : MonoBehaviour
{
    public static BonusMalusUI Instance;

    [Header("UI")]
    [SerializeField] private GameObject maingo;
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
    private GameObject selectedBonusGo;
    private GameObject selectedMalusGo;
    private bool bonusChosen = false;
    private bool malusChosen = false;

    [HideInInspector] public UnityEvent<EndBonus, EndMalus> onChoicesConfirmed;

    [Header("Notifications")]
    [SerializeField] private LocalizedString chooseString;
    [SerializeField] private LocalizedString noChoiceWarning;

    private void Start()
    {
        DayManager.Instance.OnDayEnd.AddListener(ShowChoices);
    }

    public void ShowChoices()
    {
        PlayerHudNotification.Instance.ShowText(chooseString.GetLocalizedString(), 3f);

        maingo.SetActive(true);
        bonusChosen = false;
        malusChosen = false;

        ShowBonusChoices();
        ShowMalusChoices();

        confirmButton.onClick.AddListener(Confirm);
    }

    private void ShowBonusChoices()
    {
        var bonusValues = System.Enum.GetValues(typeof(EndBonus)).Cast<EndBonus>().OrderBy(_ => Random.value).Take(3);

        foreach (EndBonus bonus in bonusValues)
        {
            int index = (int)bonus;
            GameObject go = Instantiate(choicePrefab, bonusContainer);
            Button button = go.GetComponent<Button>();
            SetupChoiceUI(go, bonusIcons[index], bonusDescriptions[index], () =>
            {
                // Unselect previous
                if (selectedBonusGo != null)
                    SetButtonHighlight(selectedBonusGo, false, true);

                // Update selection
                selectedBonus = bonus;
                selectedBonusGo = go;
                SetButtonHighlight(selectedBonusGo, true, true);

                UpdateConfirmInteractable();
            });
        }
    }


    private void ShowMalusChoices()
    {
        List<EndMalus> malusValues = System.Enum.GetValues(typeof(EndMalus)).Cast<EndMalus>().ToList();

        List<EndMalus> selectedMaluses = new List<EndMalus> { EndMalus.MORE_CLIENTS };
        var others = malusValues.Where(m => m != EndMalus.MORE_CLIENTS).OrderBy(_ => Random.value).Take(2);
        selectedMaluses.AddRange(others);

        foreach (EndMalus malus in selectedMaluses.OrderBy(_ => Random.value))
        {
            int index = (int)malus;
            GameObject go = Instantiate(choicePrefab, malusContainer);
            Button button = go.GetComponent<Button>();
            SetupChoiceUI(go, malusIcons[index], malusDescriptions[index], () =>
            {
                if (selectedMalusGo != null)
                    SetButtonHighlight(selectedMalusGo, false, false);

                selectedMalus = malus;
                selectedMalusGo = go;
                SetButtonHighlight(selectedMalusGo, true, false);

                UpdateConfirmInteractable();
            });
        }
    }

    private void SetButtonHighlight(GameObject go, bool selected, bool isBonus)
    {
        Image targetImage = go.transform.Find("Front").Find("Border").GetComponent<Image>();
        targetImage.color = selected ? (isBonus ? Color.green : Color.red) : new Color(1, 1, 1, 0);
    }

    private void UpdateConfirmInteractable()
    {
        confirmButton.interactable = bonusChosen && malusChosen;
    }

    private void SetupChoiceUI(GameObject go, Sprite icon, LocalizedString description, UnityEngine.Events.UnityAction onClick)
    {
        go.transform.Find("Front").Find("Icon").GetComponent<Image>().sprite = icon;
        LocalizeStringEvent descHolder = go.transform.GetComponentInChildren<LocalizeStringEvent>();
        descHolder.StringReference = description;
        go.GetComponent<Button>().onClick.AddListener(onClick);
    }

    private void Confirm()
    {
        if (!bonusChosen || !malusChosen)
        {
            PlayerHudNotification.Instance.ShowText(noChoiceWarning.GetLocalizedString(), 3f);
            return;
        }
        Debug.Log($"Confirmed Bonus: {selectedBonus}, Malus: {selectedMalus}");
        maingo.SetActive(false);

        onChoicesConfirmed.Invoke(selectedBonus, selectedMalus);
    }

}
