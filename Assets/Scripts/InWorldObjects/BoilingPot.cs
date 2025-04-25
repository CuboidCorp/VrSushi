using UnityEngine;
using UnityEngine.UI;

public class BoilingPot : MonoBehaviour, ICookingUtensil
{
    [SerializeField] private Transform boilingAttachPoint;

    [Header("WaterMaterials")]
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
    private bool isBoiling = false;
    private bool hasBoiled = false;
    private bool hasOverboiled = false;

    private float currentBoilingTime = 0f;
    private float currentBoilingMaxTime = 0f;
    private float currentOverBoilingMaxTime = 0f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[1] = waterMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bouillable") && isBoiling == false)
        {
            //TODO : Check if the item is grabbed if yes, nothing 
            //Otherwise, attach the item to the boiling pot
            meshRenderer.materials[1] = waterMaterial;
            currentBoilingItem = other.gameObject;
            boilableCurrentItem = currentBoilingItem.GetComponent<Boilable>();
            currentBoilingTime = 0;
            currentBoilingMaxTime = boilableCurrentItem.boilMaxTime;
            currentOverBoilingMaxTime = boilableCurrentItem.overBoilMaxtime;
            canvaProgressbar.SetActive(true);
            boilingProgressBar.fillAmount = 0;
            overboilingProgressBar.fillAmount = 0;
            if (boilableCurrentItem != null)
            {
                currentBoilingItem.transform.SetParent(boilingAttachPoint);
                currentBoilingItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                isBoiling = true;
                hasBoiled = false;
                hasOverboiled = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bouillable") && isBoiling == true)
        {
            meshRenderer.materials[1] = waterMaterial;
            currentBoilingItem.transform.SetParent(null);
            currentBoilingItem = null;
            boilableCurrentItem = null;
            isBoiling = false;
            canvaProgressbar.SetActive(false);
        }
    }

    public void DoCookDamage(float cookDamage)
    {
        if (currentBoilingItem == null || boilableCurrentItem == null || isBoiling == false)
            return;

        currentBoilingTime += cookDamage;
        if (currentBoilingTime < currentBoilingMaxTime)
        {
            boilingProgressBar.fillAmount = currentBoilingTime / currentBoilingMaxTime;
        }
        else if (currentBoilingTime >= currentBoilingMaxTime)
        {
            if (!hasBoiled)
            {
                boilingProgressBar.fillAmount = 1;
                meshRenderer.materials[1] = boilingWaterMaterial;
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
        else if (currentBoilingTime >= currentOverBoilingMaxTime)
        {
            if (!hasOverboiled)
            {
                overboilingProgressBar.fillAmount = 1;
                meshRenderer.materials[1] = overboilingWaterMaterial;
                GameObject overboiledItem = Instantiate(boilableCurrentItem.overboiledObjectPrefab, boilingAttachPoint.position, Quaternion.identity);
                Destroy(currentBoilingItem);
                currentBoilingItem = overboiledItem;
                hasOverboiled = true;
            }
            else
            {
                //Overboiled item, do nothing
            }

        }
    }
}
