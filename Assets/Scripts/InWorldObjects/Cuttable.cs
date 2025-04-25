using UnityEngine;

public class Cuttable : MonoBehaviour
{
    [Tooltip("The object this transforms into once cut")]
    public GameObject cutObjectPrefab;

    [Tooltip("The amount of damage that must be done for the object to transform")]
    public int cutMaxHealth = 100;

    [Tooltip("The preferred rotation of the object this transforms into")]
    public Vector3 cutObjectPrefabPreferredRotation = Vector3.zero;
}
