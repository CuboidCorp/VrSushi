using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewKitchenItem", menuName = "CookingGame/KitchenItem")]
public class KitchenItem : ScriptableObject
{
    public LocalizedString itemNameLocalized;
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
}
