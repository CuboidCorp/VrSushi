using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class CurrentMultsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nbClients;
    [SerializeField] private TMP_Text nbClientsPremium;
    [SerializeField] private TMP_Text clientWaitTimeMultiplier;
    [SerializeField] private TMP_Text knifeDamageMultiplier;
    [SerializeField] private TMP_Text fishLifeMultiplier;
    [SerializeField] private TMP_Text overcookTimeMultiplier;
    [SerializeField] private TMP_Text cookingSpeedMultiplier;
    [SerializeField] private LocalizeStringEvent stoveFailureBool;

    [Header("Localize Strings")]
    [SerializeField] private LocalizedString trueString;
    [SerializeField] private LocalizedString falseString;

    private void Start()
    {
        nbClients.text = GameData.Instance.nbClients.ToString();
        nbClientsPremium.text = GameData.Instance.nbClientsPremium.ToString();
        clientWaitTimeMultiplier.text = GameData.Instance.clientWaitTimeMultiplier.ToString("F2");
        knifeDamageMultiplier.text = GameData.Instance.knifeDamageMultiplier.ToString("F2");
        fishLifeMultiplier.text = GameData.Instance.fishLifeMultiplier.ToString("F2");
        overcookTimeMultiplier.text = GameData.Instance.overcookTimeMultiplier.ToString("F2");
        cookingSpeedMultiplier.text = GameData.Instance.cookingSpeedMultiplier.ToString("F2");
        stoveFailureBool.StringReference = GameData.Instance.stoveFailure ? trueString : falseString;
    }

}
