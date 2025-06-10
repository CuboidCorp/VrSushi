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
            Rigidbody knifeRb = knifeScript.GetComponent<Rigidbody>();
            if (knifeRb != null)
            {
                //On verif si le couteau va vers le bas avec assez de vitesse
                float downwardVelocity = Vector3.Dot(knifeRb.linearVelocity, Vector3.down);
                if (downwardVelocity > 0.1f)
                {
                    OnCut?.Invoke(knifeScript.cuttingPower);
                }
            }
        }
    }

}
