using UnityEngine;
using UnityEngine.Events;

public class Cuttable : MonoBehaviour
{
    [Tooltip("The object this transforms into once cut")]
    public GameObject cutObjectPrefab;

    [Tooltip("The amount of damage that must be done for the object to transform")]
    public int cutMaxHealth = 100;

    [Tooltip("The preferred rotation of the object this transforms into")]
    public Vector3 cutObjectPrefabPreferredRotation = Vector3.zero;

    [Tooltip("Attach point position this object")]
    public Vector3 cutObjectAttachPoint = Vector3.zero;

    [Tooltip("Attach point rotation this object")]
    public Vector3 cutObjectAttachRotation = Vector3.zero;

    [HideInInspector] public UnityEvent<int> OnCut;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Knife knifeScript))
        {
            OnCut?.Invoke(knifeScript.cuttingPower);
        }
    }
}
