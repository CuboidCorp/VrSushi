using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Collider))]
public class CuttingStation : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor socketInteractor;

    [Tooltip("The object to activate when progress is made on a cut")]
    [SerializeField] private GameObject cutProgressGameObject;
    [Tooltip("The image used to make a progress bar (circle)")]
    [SerializeField] private Image cutProgressImage;

    private GameObject currentItem = null;
    private int currentTargetMaxHealth = 0;
    private int currentDamage = 0;

    private void Awake()
    {
        if (GetComponent<Collider>().isTrigger == false)
        {
            Debug.LogError("CuttingStation collider must be a trigger");
            Destroy(this);
        }
        if (socketInteractor == null)
        {
            Debug.LogError("CuttingStation must have a socket interactor");
            Destroy(this);
        }
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
        currentDamage = 0;
        currentItem = args.interactableObject.transform.gameObject;
        if (currentItem.TryGetComponent(out Cuttable cuttable))
        {
            currentTargetMaxHealth = cuttable.cutMaxHealth;
        }
        else
        {
            Debug.LogWarning("Item is not cuttable: " + currentItem.name);
            currentTargetMaxHealth = 0;
        }
    }

    private void OnItemRemoved(SelectExitEventArgs args)
    {
        currentTargetMaxHealth = 0;
        currentDamage = 0;
        currentItem = null;
        if (cutProgressGameObject != null)
        {
            cutProgressGameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!socketInteractor.hasSelection || currentItem == null)
            return;
        if (other.TryGetComponent(out Knife knifeScript))
        {
            currentDamage += knifeScript.cuttingPower;
            if (cutProgressGameObject != null)
            {
                cutProgressGameObject.SetActive(true);
            }
            if (cutProgressImage != null)
            {
                cutProgressImage.fillAmount = (float)currentDamage / (float)currentTargetMaxHealth;
            }
            //TODO : Barre de progression au dessus de l'objet qui affiche a combien de degats on est
            if (currentDamage >= currentTargetMaxHealth)
            {
                Cuttable cuttable = currentItem.GetComponent<Cuttable>();
                GameObject cutPrefab = cuttable.cutObjectPrefab;

                GameObject cuttedObject = currentItem; //Cache pour supprimer l'objet
                IXRSelectInteractable interactable = socketInteractor.firstInteractableSelected;
                socketInteractor.interactionManager.CancelInteractableSelection(interactable);

                GameObject cutGO = Instantiate(cutPrefab);
                cutGO.transform.SetPositionAndRotation(cuttedObject.transform.position, Quaternion.Euler(cuttable.cutObjectPrefabPreferredRotation));

                Destroy(cuttedObject);

            }
        }
    }
}
