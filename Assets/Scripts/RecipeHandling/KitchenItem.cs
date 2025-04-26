using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewKitchenItem", menuName = "CookingGame/KitchenItem")]
public class KitchenItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
}
