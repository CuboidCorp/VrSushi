using System.Linq;
using UnityEngine;

public class Base : MonoBehaviour
{
    [SerializeField] private KitchenItem[] combinables;
    [SerializeField] private Transform[] transformPositions;
    [SerializeField] private GameObject[] targetObjectsPrefab;
    [SerializeField] private bool hasMultipleTargetObjects;

    private int nbCombinablesPlaced = 0;

    private void Awake()
    {
        if (combinables.Length > 1 && hasMultipleTargetObjects == false)
        {
            if (combinables.Length != transformPositions.Length)
            {
                Debug.LogError("Combinables and Transform Positions arrays must be of the same length.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with: " + collision.transform.name);
        Debug.Log("Collision with tag: " + collision.transform.tag);
        if (collision.transform.CompareTag("Combinable") && collision.gameObject.TryGetComponent(out Combinable comb))
        {
            if (combinables.Contains(comb.item))
            {
                nbCombinablesPlaced++;
                if (hasMultipleTargetObjects)
                {
                    //On recup l'index de l'item
                    int index = System.Array.IndexOf(combinables, comb.item);
                    Instantiate(targetObjectsPrefab[index], transform.position, Quaternion.identity);
                    Destroy(collision.gameObject);
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    if (nbCombinablesPlaced == combinables.Length)
                    {
                        Instantiate(targetObjectsPrefab[0], transform.position, Quaternion.identity);
                        Destroy(gameObject);
                        return;
                    }
                }

                GameObject go = collision.gameObject;
                go.transform.SetParent(transformPositions[nbCombinablesPlaced - 1]);
                go.transform.SetPositionAndRotation(transformPositions[nbCombinablesPlaced - 1].position, transformPositions[nbCombinablesPlaced - 1].rotation);
            }
        }
    }
}
