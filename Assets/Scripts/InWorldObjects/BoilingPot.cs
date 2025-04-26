using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoilingPot : MonoBehaviour, ICookingUtensil, IFillable
{
    [SerializeField] private Transform boilingAttachPoint;

    [Header("WaterMaterials")]
    [SerializeField] private Material emptyWaterMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material boilingWaterMaterial;
    [SerializeField] private Material overboilingWaterMaterial;

    [Header("ProgressBar")]
    [SerializeField] private GameObject canvaProgressbar;
    [SerializeField] private Image boilingProgressBar;
    [SerializeField] private Image overboilingProgressBar;

    private MeshRenderer meshRenderer;
    private GameObject currentBoilingItem = null;
    private Boilable boilableCurrentItem = null;
    private bool isFilled = false;
    private bool isBoiling = false;
    private bool hasBoiled = false;
    private bool hasOverboiled = false;

    private float currentBoilingTime = 0f;
    private float currentBoilingMaxTime = 0f;
    private float currentOverBoilingMaxTime = 0f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetWaterMaterial(emptyWaterMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bouillable") || isBoiling)
            return;

        // Vérifie si l'objet est grab
        XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null && grabInteractable.isSelected)
            return;

        if (!other.TryGetComponent(out Boilable boilable))
        {
            Debug.LogWarning("Object tagged as Bouillable does not have a Boilable component!");
            return;
        }

        currentBoilingItem = other.gameObject;
        boilableCurrentItem = boilable;

        currentBoilingTime = 0;
        currentBoilingMaxTime = boilableCurrentItem.boilMaxTime;
        currentOverBoilingMaxTime = boilableCurrentItem.overBoilMaxtime;

        currentBoilingItem.transform.SetParent(boilingAttachPoint);
        currentBoilingItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        SetWaterMaterial(waterMaterial);
        isBoiling = true;
        hasBoiled = false;
        hasOverboiled = false;

        canvaProgressbar.SetActive(true);
        boilingProgressBar.fillAmount = 0;
        overboilingProgressBar.fillAmount = 0;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Bouillable") || !isBoiling)
            return;

        if (currentBoilingItem != null)
            currentBoilingItem.transform.SetParent(null);

        currentBoilingItem = null;
        boilableCurrentItem = null;
        isBoiling = false;
        canvaProgressbar.SetActive(false);
    }

    public void DoCookDamage(float cookDamage)
    {
        if (currentBoilingItem == null || boilableCurrentItem == null || !isBoiling || !isFilled)
            return;

        Debug.Log($"Doing cook damage: {cookDamage} to item: {currentBoilingItem.name}");

        currentBoilingTime += cookDamage;

        if (currentBoilingTime < currentBoilingMaxTime)
        {
            boilingProgressBar.fillAmount = currentBoilingTime / currentBoilingMaxTime;
        }
        else if (currentBoilingTime >= currentOverBoilingMaxTime)
        {
            if (!hasOverboiled)
            {
                overboilingProgressBar.fillAmount = 1;
                SetWaterMaterial(overboilingWaterMaterial);
                GameObject overboiledItem = Instantiate(boilableCurrentItem.overboiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
                Destroy(currentBoilingItem);
                currentBoilingItem = overboiledItem;
                hasOverboiled = true;
            }
        }
        else if (currentBoilingTime >= currentBoilingMaxTime)
        {
            if (!hasBoiled)
            {
                boilingProgressBar.fillAmount = 1;
                SetWaterMaterial(boilingWaterMaterial);
                GameObject boiledItem = Instantiate(boilableCurrentItem.boiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
                Destroy(currentBoilingItem);
                currentBoilingItem = boiledItem;
                hasBoiled = true;
            }
            else
            {
                overboilingProgressBar.fillAmount = (currentBoilingTime - currentBoilingMaxTime) / (currentOverBoilingMaxTime - currentBoilingMaxTime);
            }
        }
    }

    private void Update()
    {
        if (Vector3.Dot(transform.up, Vector3.down) > 0.7f)
        {
            Empty();
        }
    }

    public void Fill()
    {
        if (isFilled)
            return;

        isFilled = true;
        SetWaterMaterial(waterMaterial);
    }

    public void Empty()
    {
        if (!isFilled && currentBoilingItem == null)
            return;

        Debug.Log("Emptying boiling pot");
        SetWaterMaterial(emptyWaterMaterial);

        if (currentBoilingItem != null)
        {
            currentBoilingItem.transform.SetParent(null);
            currentBoilingItem = null;
            boilableCurrentItem = null;
        }

        isFilled = false;
        isBoiling = false;
        hasBoiled = false;
        hasOverboiled = false;
        canvaProgressbar.SetActive(false);
    }

    private void SetWaterMaterial(Material material)
    {
        Material[] mats = meshRenderer.materials;
        mats[1] = material;
        meshRenderer.materials = mats;
    }
}
