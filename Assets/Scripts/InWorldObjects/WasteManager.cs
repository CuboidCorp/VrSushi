using UnityEngine;
using System.Collections.Generic;

public class WasteManager : MonoBehaviour
{
    public static WasteManager Instance { get; private set; }

    private List<GameObject> trackedIngredients = new List<GameObject>();

    private int deletedIngredients = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddIngredient(GameObject ingredient)
    {
        if (!trackedIngredients.Contains(ingredient))
        {
            trackedIngredients.Add(ingredient);
        }
    }

    public void UseIngredient(GameObject ingredient)
    {
        if (trackedIngredients.Contains(ingredient))
        {
            trackedIngredients.Remove(ingredient);
        }
    }

    public void DeleteIngredient(GameObject ingredient)
    {
        if (trackedIngredients.Contains(ingredient))
        {
            trackedIngredients.Remove(ingredient);
            deletedIngredients++;
        }
    }

    public int GetRemainingIngredients()
    {
        trackedIngredients.RemoveAll(item => item == null); // Cleanup destroyed objects
        return trackedIngredients.Count + deletedIngredients;
    }

    public void SetWaste()
    {
        DayManager.Instance.dayStats.wastedIngredients = GetRemainingIngredients();
    }

    // For debugging or editor button
    [ContextMenu("Print Remaining Ingredients")]
    public void PrintRemainingIngredients()
    {
        Debug.Log($"Remaining unused ingredients: {GetRemainingIngredients()}");
    }
}
