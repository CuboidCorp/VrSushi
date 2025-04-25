using System.Collections;
using UnityEngine;

public class CookingPlate : MonoBehaviour
{
    private ICookingUtensil cookingUtensil;
    private Coroutine cookdamageCoroutine;

    [SerializeField] private float cookDamageInterval = .1f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CookingUtensil"))
        {
            if (other.TryGetComponent(out cookingUtensil))
            {
                cookdamageCoroutine ??= StartCoroutine(CookCoroutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CookingUtensil"))
        {
            if (cookdamageCoroutine != null)
            {
                StopCoroutine(cookdamageCoroutine);
                cookdamageCoroutine = null;
            }
        }
    }

    private IEnumerator CookCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cookDamageInterval);
            cookingUtensil.DoCookDamage(cookDamageInterval);
        }
    }
}
