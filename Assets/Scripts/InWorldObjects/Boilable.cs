using UnityEngine;

public class Boilable : MonoBehaviour
{
    [Tooltip("The object this transforms into once boiled")]
    public GameObject boiledObjectPrefab;

    [Tooltip("The amount of time that must be done for the object to transform")]
    public float boilMaxTime = 10f;

    [Tooltip("The object this transforms into once overboiled")]
    public GameObject overboiledObjectPrefab;

    [Tooltip("The amount of damage that must be done for the object to transform into the overboiled version")]
    public float overBoilMaxtime = 15f;
}
