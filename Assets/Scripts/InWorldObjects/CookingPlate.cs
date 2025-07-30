using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CookingPlate : MonoBehaviour
{

    private Coroutine cookdamageCoroutine;

    [SerializeField] private bool canHaveFailure = false;

    [Header("Cooking Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private GameObject particles;

    [SerializeField] private float cookDamageInterval = .1f;

    private AudioSource audioSourceGazStoveSfx;

    [SerializeField] private XRSocketInteractor socketInteractor;

    private Rigidbody currentRigidbody;
    private ICookingUtensil cookingUtensil;

    private void Awake()
    {
        audioSourceGazStoveSfx = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        socketInteractor.selectEntered.AddListener(OnItemPlaced);
        socketInteractor.selectExited.AddListener(OnItemRemoved);
    }

    private void OnDisable()
    {
        socketInteractor.selectEntered.RemoveListener(OnItemPlaced);
        socketInteractor.selectExited.RemoveListener(OnItemRemoved);
    }

    private void Start()
    {
        if (canHaveFailure && GameData.Instance.stoveFailure)
        {
            enabled = false;
            socketInteractor.enabled = false;
            GameData.Instance.stoveFailure = false;
        }
    }

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        GameObject currentObject = args.interactableObject.transform.gameObject;
        if (currentObject.TryGetComponent(out cookingUtensil))
        {
            currentRigidbody = currentObject.GetComponent<Rigidbody>();
            if (isActive)
            {
                cookdamageCoroutine ??= StartCoroutine(CookCoroutine());
            }
        }
        else
        {
            Debug.LogWarning("Item is not a cooking utensil: " + currentObject.name);
            return;
        }

    }

    private void OnItemRemoved(SelectExitEventArgs args)
    {
        StopCooking();
        currentRigidbody = null;
        cookingUtensil = null;
    }

    public void Enable()
    {
        isActive = true;
        Debug.Log("Enabling Cooking Plate");
        audioSourceGazStoveSfx.Play();
        particles.SetActive(true);
        Debug.Log("Current Rigidbody: " + currentRigidbody);
        if (currentRigidbody != null)
        {
            cookdamageCoroutine ??= StartCoroutine(CookCoroutine());
        }
    }

    public void Disable()
    {
        isActive = false;
        audioSourceGazStoveSfx.Stop();
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
