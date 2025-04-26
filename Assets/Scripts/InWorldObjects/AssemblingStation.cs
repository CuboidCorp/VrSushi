using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class AssemblingStation : MonoBehaviour
{
    [SerializeField] private GameObject uiChoice;
    [SerializeField] private Transform uiHolder;

    [SerializeField] private GameObject ingredientUIPrefab;

    private XRSocketInteractor socketInteractor;

    private GameObject socketedItem;
    private Assemblable socketedAssemblable;


    private void Awake()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();
        uiChoice.SetActive(false);
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

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        socketedItem = args.interactableObject.transform.gameObject;
        if (socketedItem.TryGetComponent(out socketedAssemblable))
        {
            uiChoice.SetActive(true);
            foreach (KitchenItem item in socketedAssemblable.possibleAssemblable)
            {
                GameObject ingredientUI = Instantiate(ingredientUIPrefab, uiHolder);
                ingredientUI.transform.GetChild(1).GetChild(2).GetComponent<Image>().sprite = item.icon;
                ingredientUI.GetComponent<Button>().onClick.AddListener(() => SelectItem(item));
                ingredientUI.GetComponentInChildren<TMP_Text>().text = item.name;
            }
        }
        else
        {
            Debug.LogWarning("Item is not assemblable: " + socketedItem.name);
        }
    }

    private void OnItemRemoved(SelectExitEventArgs args)
    {
        socketedItem = null;
        socketedAssemblable = null;
        uiChoice.SetActive(false);
        foreach (Transform child in uiHolder)
        {
            Destroy(child.gameObject);
        }
    }

    private void SelectItem(KitchenItem item)
    {
        if (socketedItem == null)
            return;
        Instantiate(item.prefab, transform.position, Quaternion.identity);
        Destroy(socketedItem);
    }
}
