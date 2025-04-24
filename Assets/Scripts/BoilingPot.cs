using UnityEngine;

public class BoilingPot : MonoBehaviour
{
    [SerializeField] private Transform boilingAttachPoint;

    private GameObject currentBoilingItem = null;
    private Boilable boilableCurrentItem = null;
    private bool isBoiling = false;

    private int currentBoilingDamage = 0;
    private int currentMaxBoilingHealth = 0;
    private int currentOverBoilingHealth = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bouillable") && isBoiling == false)
        {
            //TODO : Check if the item is grabbed if yes, nothing 
            //Otherwise, attach the item to the boiling pot
            currentBoilingItem = other.gameObject;
            boilableCurrentItem = currentBoilingItem.GetComponent<Boilable>();
            currentBoilingDamage = 0;
            currentMaxBoilingHealth = boilableCurrentItem.boilMaxHealth;
            currentOverBoilingHealth = boilableCurrentItem.overBoilMaxHealth;
            if (boilableCurrentItem != null)
            {
                currentBoilingItem.transform.SetParent(boilingAttachPoint);
                currentBoilingItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                isBoiling = true;
            }
        }
    }

    public void DoCookDamage(int cookDamage)
    {
        currentBoilingDamage += cookDamage;
        if (currentBoilingDamage >= currentMaxBoilingHealth)
        {
            GameObject boiledItem = Instantiate(boilableCurrentItem.boiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
            Destroy(currentBoilingItem);
            currentBoilingItem = boiledItem;
        }
        else if (currentBoilingDamage >= currentOverBoilingHealth)
        {
            GameObject overboiledItem = Instantiate(boilableCurrentItem.overboiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
            Destroy(currentBoilingItem);
            currentBoilingItem = overboiledItem;
        }
    }
}
