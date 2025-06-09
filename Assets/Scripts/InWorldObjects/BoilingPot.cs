using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoilingPot : MonoBehaviour, ICookingUtensil, IFillable
{
    [SerializeField] private Transform boilingAttachPoint;
    [SerializeField] private GameObject boilingParticles;


    [Header("Audio")]
    [SerializeField] private AudioSource boilingAudioSource;
    [SerializeField] private AudioSource pouringAudioSource;
    [SerializeField] private float baseVolume;
    [SerializeField] private float overboilingVolume;

    [Header("WaterMaterials")]
    [SerializeField] private Material emptyWaterMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material boilingWaterMaterial;
    [SerializeField] private Material overboilingWaterMaterial;

    [Header("ProgressBar")]
    [SerializeField] private GameObject canvaProgressbar;
    [SerializeField] private Image boilingProgressBar;
    [SerializeField] private Image overboilingProgressBar;

    [Header("AudioSources")]


    private MeshRenderer meshRenderer;
    private GameObject currentBoilingItem = null;
    private Boilable boilableCurrentItem = null;
    private GameObject spawnedBoiledItem = null;
    private bool isFilled = false;
    private bool isBoiling = false;
    private bool hasBoiled = false;
    private bool hasOverboiled = false;

    private float currentBoilingTime = 0f;
    private float currentBoilingMaxTime = 0f;
    private float currentOverBoilingMaxTime = 0f;

    private void Awake()
    {
        boilingAudioSource.volume = baseVolume;
        boilingParticles.SetActive(false);
        meshRenderer = GetComponent<MeshRenderer>();
        SetWaterMaterial(emptyWaterMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't allow boiling if no water or already boiling something
        if (!other.CompareTag("Bouillable") || isBoiling || !isFilled)
            return;

        XRGrabInteractable grabInteractable = other.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null && grabInteractable.isSelected)
            return;

        grabInteractable.enabled = false;

        if (!other.TryGetComponent(out Boilable boilable))
        {
            Debug.LogWarning("Object tagged as Bouillable does not have a Boilable component!");
            return;
        }

        Debug.Log($"Boiling pot detected bouillable: {other.name}");

        currentBoilingItem = other.gameObject;
        boilableCurrentItem = boilable;

        currentBoilingTime = 0;
        currentBoilingMaxTime = boilable.boilMaxTime;
        currentOverBoilingMaxTime = boilable.overBoilMaxtime;

        currentBoilingItem.transform.SetParent(boilingAttachPoint);
        currentBoilingItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        isBoiling = true;
    }

    public void Empty()
    {
        if (!isFilled && currentBoilingItem == null)
            return;

        Debug.Log("Emptying boiling pot");

        boilingAudioSource.Stop();
        boilingAudioSource.volume = baseVolume;

        pouringAudioSource.Play();

        SetWaterMaterial(emptyWaterMaterial);
        boilingParticles.SetActive(false);

        if (currentBoilingItem != null)
        {
            if (spawnedBoiledItem != null)
            {
                spawnedBoiledItem.transform.SetParent(null);

                if (spawnedBoiledItem.TryGetComponent(out Rigidbody itemRb))
                {
                    itemRb.AddForce((transform.up * -1 + transform.forward) * 0.5f, ForceMode.Impulse);
                }

                if (spawnedBoiledItem.TryGetComponent(out XRGrabInteractable interactable))
                {
                    interactable.enabled = true;
                }

                Destroy(currentBoilingItem);
                spawnedBoiledItem = null;
            }
            else
            {
                currentBoilingItem.transform.SetParent(null);

                if (currentBoilingItem.TryGetComponent(out Rigidbody itemRb))
                {
                    itemRb.AddForce((transform.up * -1 + transform.forward) * 0.5f, ForceMode.Impulse);
                }

                if (currentBoilingItem.TryGetComponent(out XRGrabInteractable interactable))
                {
                    interactable.enabled = true;
                }
            }


            currentBoilingItem = null;
            boilableCurrentItem = null;
        }

        isFilled = false;
        isBoiling = false;
        hasBoiled = false;
        hasOverboiled = false;
        canvaProgressbar.SetActive(false);
    }

    public void DoCookDamage(float cookDamage)
    {
        if (currentBoilingItem == null || boilableCurrentItem == null || !isBoiling || !isFilled)
            return;

        if (currentBoilingTime == 0)
        {
            boilingAudioSource.Play();
            boilingParticles.SetActive(true);

            canvaProgressbar.SetActive(true);
            boilingProgressBar.fillAmount = 0;
            overboilingProgressBar.fillAmount = 0;

            hasBoiled = false;
            hasOverboiled = false;
        }

        currentBoilingTime += cookDamage;

        if (currentBoilingTime >= currentOverBoilingMaxTime)
        {
            if (!hasOverboiled)
            {

                overboilingProgressBar.fillAmount = 1;
                SetWaterMaterial(overboilingWaterMaterial);

                WasteManager.Instance.UseIngredient(spawnedBoiledItem);

                if (spawnedBoiledItem != null) Destroy(spawnedBoiledItem);

                spawnedBoiledItem = Instantiate(boilableCurrentItem.overboiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
                spawnedBoiledItem.transform.SetParent(boilingAttachPoint);
                spawnedBoiledItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                if (spawnedBoiledItem.TryGetComponent(out XRGrabInteractable newInteractable))
                {
                    newInteractable.enabled = false;
                }
                WasteManager.Instance.AddIngredient(spawnedBoiledItem);
                HideOriginalItem();
                hasOverboiled = true;
            }
        }
        else if (currentBoilingTime >= currentBoilingMaxTime)
        {
            if (!hasBoiled)
            {
                boilingAudioSource.volume = overboilingVolume;
                boilingProgressBar.fillAmount = 1;
                SetWaterMaterial(boilingWaterMaterial);
                WasteManager.Instance.UseIngredient(currentBoilingItem);
                spawnedBoiledItem = Instantiate(boilableCurrentItem.boiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
                spawnedBoiledItem.transform.SetParent(boilingAttachPoint);
                spawnedBoiledItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                if (spawnedBoiledItem.TryGetComponent(out XRGrabInteractable newInteractable))
                {
                    newInteractable.enabled = false;
                }
                WasteManager.Instance.AddIngredient(spawnedBoiledItem);
                HideOriginalItem();
                hasBoiled = true;
            }
            else
            {
                float overboilProgress = (currentBoilingTime - currentBoilingMaxTime) / (currentOverBoilingMaxTime - currentBoilingMaxTime);
                overboilingProgressBar.fillAmount = overboilProgress;
            }
        }
        else
        {
            boilingProgressBar.fillAmount = currentBoilingTime / currentBoilingMaxTime;
        }
    }

    private void HideOriginalItem()
    {
        if (currentBoilingItem == null)
            return;

        foreach (Renderer renderer in currentBoilingItem.GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        foreach (Collider collider in currentBoilingItem.GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }

    private void Update()
    {
        if (Vector3.Dot(transform.up, Vector3.down) > 0.6f && isFilled)
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

        if (currentBoilingItem != null && !isBoiling)
        {
            isBoiling = true;
            canvaProgressbar.SetActive(true);
        }
    }

    private void SetWaterMaterial(Material material)
    {
        Material[] mats = meshRenderer.materials;
        mats[1] = material;
        meshRenderer.materials = mats;
    }
}
