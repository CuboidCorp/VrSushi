using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationHandler : MonoBehaviour
{

    [Header("Localization")]
    [SerializeField] TMP_Dropdown m_LanguageDropdown;

    private void Awake()
    {
        LoadLanguages();
    }

    /// <summary>
    /// Charge les langues disponibles dans le dropdown des options de localisation.
    /// </summary>
    private void LoadLanguages()
    {
        m_LanguageDropdown.ClearOptions();
        List<Locale> availableLanguages = LocalizationSettings.AvailableLocales.Locales;
        List<string> languageOptions = new();

        foreach (Locale locale in availableLanguages)
        {
            string nativeName = locale.Identifier.CultureInfo.NativeName;
            string titleCaseName = locale.Identifier.CultureInfo.TextInfo.ToTitleCase(nativeName.ToLower());
            languageOptions.Add(titleCaseName);
        }

        m_LanguageDropdown.AddOptions(languageOptions);

        Locale currentLocale = LocalizationSettings.SelectedLocale;
        int currentIndex = availableLanguages.IndexOf(currentLocale);
        if (currentIndex >= 0)
        {
            m_LanguageDropdown.value = currentIndex;
        }
    }


    /// <summary>
    /// Gère le changement de langue lorsque l'utilisateur sélectionne une nouvelle option dans le dropdown.
    /// </summary>
    /// <param name="index">Index de la langue sélectionnée.</param>
    public void HandleLanguageChange(int index)
    {
        Locale selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;
        Debug.Log($"Langue changée en : {selectedLocale.Identifier.CultureInfo.DisplayName}");
    }
}
