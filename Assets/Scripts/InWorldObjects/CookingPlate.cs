using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CookingPlate : MonoBehaviour
{

    private Coroutine cookdamageCoroutine;

    [Header("Cooking Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject particles;

    [SerializeField] private float cookDamageInterval = .1f;

    private Rigidbody currentRigidbody;
    private ICookingUtensil cookingUtensil;
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null)
            return;

        if (other.attachedRigidbody.CompareTag("CookingUtensil"))
        {
            if (other.attachedRigidbody.TryGetComponent(out cookingUtensil))
            {
                currentRigidbody = other.attachedRigidbody;
                if (isActive)
                {
                    cookdamageCoroutine ??= StartCoroutine(CookCoroutine());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null)
            return;
        if (other.attachedRigidbody.CompareTag("CookingUtensil"))
        {
            StopCooking();
            if (other.attachedRigidbody.TryGetComponent(out cookingUtensil))
            {
                currentRigidbody = null;
                cookingUtensil = null;
            }
        }
    }

    public void Enable()
    {
        Debug.Log("Cooking plate enable");
        isActive = true;
        particles.SetActive(true);
        if (currentRigidbody != null)
        {
            if (currentRigidbody.TryGetComponent(out cookingUtensil))
            {
                cookdamageCoroutine ??= StartCoroutine(CookCoroutine());
            }
        }
    }

    public void Disable()
    {
        isActive = false;
        particles.SetActive(false);
        StopCooking();
    }

    private void StopCooking()
    {
        if (cookdamageCoroutine != null)
        {
            StopCoroutine(cookdamageCoroutine);
            cookdamageCoroutine = null;
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
