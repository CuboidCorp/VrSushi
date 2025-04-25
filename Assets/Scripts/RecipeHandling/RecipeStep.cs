using System.Collections.Generic;

[System.Serializable]
public class RecipeStep
{
    public string stepDescription;

    public ObtentionMethod method;
    public List<KitchenItem> inputItems;
    public KitchenItem resultItem;

    // Only used when method == SPAWN
    public SpawnLocation spawnLocation;
}
