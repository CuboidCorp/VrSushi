using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientSpawner : MonoBehaviour
{

    [Header("Spawner Settings")]
    [SerializeField] private string spawnerName;
    [SerializeField] private TMP_Text spawnerNameText;
    [SerializeField] private IngredientList ingredientList;
    [SerializeField] private Transform spawnPoint;

    [Header("UI Settings")]
    [SerializeField] private GameObject ingredientUIPrefab;
    [SerializeField] private Transform uiHolder;
    [SerializeField] private Button spawnBtn;

    private GameObject selectedIngredientUI;
    private KitchenItem selectedItem;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        spawnBtn.onClick.AddListener(SpawnItem);
        foreach (KitchenItem item in ingredientList.ingredients)
        {
            GameObject ingredientUI = Instantiate(ingredientUIPrefab, uiHolder);
            Image image = ingredientUI.transform.GetChild(1).GetChild(2).GetComponent<Image>();
            image.sprite = item.icon;

            Button button = ingredientUI.GetComponent<Button>();
            button.onClick.AddListener(() => SelectItem(item, ingredientUI));

            TMP_Text text = ingredientUI.GetComponentInChildren<TMP_Text>();
            text.text = item.name;

        }
    }

    private void SelectItem(KitchenItem item, GameObject selectIcon)
    {
        if (selectedIngredientUI != null)
        {
            selectedIngredientUI.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        }
        selectedIngredientUI = selectIcon;
        selectedIngredientUI.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
        Debug.Log($"Selected item: {item.name}");
        selectedItem = item;
    }

    private void SpawnItem()
    {
        if (selectedItem == null)
            return;
        Debug.Log($"Spawning item: {selectedItem.name}");
        Instantiate(selectedItem.prefab, spawnPoint.position, Quaternion.identity);
    }
}
