using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [SerializeField] private IngredientList ingredientList;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private GameObject ingredientUIPrefab;
    [SerializeField] private Transform uiHolder;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        foreach (KitchenItem item in ingredientList.ingredients)
        {
            GameObject ingredientUI = Instantiate(ingredientUIPrefab, uiHolder);
        }
    }

}
