using UnityEngine;
using System.Collections.Generic;

public class WasteManager : MonoBehaviour
{
    public static WasteManager Instance { get; private set; }
    [SerializeField] private int penaltyPerIngredient = 5; // Points lost per wasted ingredient

    private List<GameObject> trackedIngredients = new List<GameObject>();

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

    public int GetRemainingIngredients()
    {
        trackedIngredients.RemoveAll(item => item == null); // Cleanup destroyed objects
        return trackedIngredients.Count;
    }

    public int CalculateWastePenalty()
    {
        int wastedCount = GetRemainingIngredients();
        return wastedCount * penaltyPerIngredient;
    }

    // For debugging or editor button
    [ContextMenu("Print Remaining Ingredients")]
    public void PrintRemainingIngredients()
    {
        Debug.Log($"Remaining unused ingredients: {GetRemainingIngredients()}");
    }
}
