using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredientList", menuName = "CookingGame/IngredientList")]
public class IngredientList : ScriptableObject
{
    public KitchenItem[] ingredients;
}
