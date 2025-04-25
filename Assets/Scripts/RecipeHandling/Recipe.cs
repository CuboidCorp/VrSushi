using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "CookingGame/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public KitchenItem finalProduct;

    public List<RecipeStep> steps;
}
