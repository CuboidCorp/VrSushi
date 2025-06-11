using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [SerializeField] private string[] ingredientsTags; // Tags to identify ingredients

    private void OnTriggerEnter(Collider other)
    {
        foreach (string tag in ingredientsTags)
        {
            if (other.CompareTag(tag))
            {
                WasteManager.Instance.DeleteIngredient(other.gameObject);
                Destroy(other.gameObject);
                break;
            }
        }
    }
}
