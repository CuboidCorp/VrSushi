using System.Collections;
using UnityEngine;

public class CookingPlate : MonoBehaviour
{

    private Coroutine cookdamageCoroutine;

    [SerializeField] private bool canHaveFailure = false;

    [Header("Cooking Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject particles;

    [SerializeField] private float cookDamageInterval = .1f;

    private AudioSource audioSourceGazStoveSfx;

    private Rigidbody currentRigidbody;
    private ICookingUtensil cookingUtensil;

    private void Awake()
    {
        audioSourceGazStoveSfx = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (canHaveFailure && GameData.Instance.stoveFailure)
        {
            enabled = false;
            GameData.Instance.stoveFailure = false;
        }
    }
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
        audioSourceGazStoveSfx.Play();
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
        audioSourceGazStoveSfx.Stop();
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
