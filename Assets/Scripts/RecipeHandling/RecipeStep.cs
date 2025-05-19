using System.Collections.Generic;
using UnityEngine.Localization;

[System.Serializable]
public class RecipeStep
{
    public string stepDescription;
    public LocalizedString stepDescriptionLocalized;

    public ObtentionMethod method;
    public List<KitchenItem> inputItems;
    public KitchenItem resultItem;

    // Only used when method == SPAWN
    public SpawnLocation spawnLocation;
}
