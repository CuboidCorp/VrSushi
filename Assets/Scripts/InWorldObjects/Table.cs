using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Audio;

public class Table : MonoBehaviour
{
    public Transform chairPos;
    [HideInInspector] public Client currentClient;

    public bool isOccupied = false;

    private XRSocketInteractor socketInteractor;

    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private Image itemImage;

    [SerializeField] private AudioSource audioSource;

    public KitchenItem expectedPlat;
    private float timeLimit; // Temps limite pour placer le plat
    private Coroutine progressCoroutine;

    // Events
    public Action<bool> OnPlatPlaced;
    public Action OnPlatTimeout;

    private void Awake()
    {
        socketInteractor = GetComponentInChildren<XRSocketInteractor>();
        socketInteractor.enabled = false;

        // Abonnez-vous à l'événement de sélection de l'interacteur
        socketInteractor.selectEntered.AddListener(OnPlatPlacedInSocket);
    }

    public void SetClient(Client client)
    {
        currentClient = client;
        isOccupied = true;
    }

    public void RemoveClient()
    {
        currentClient = null;
        isOccupied = false;
    }

    public void SetPlat(KitchenItem item, float timeLimit)
    {
        audioSource.Play();
        expectedPlat = item;
        Debug.Log($"Plat attendu : {expectedPlat.name}");
        Debug.Log($"Temps attendu : {timeLimit}");
        this.timeLimit = timeLimit; // Définit le temps limite
        socketInteractor.enabled = true;

        // Active la barre de progression et démarre le remplissage
        if (progressBar != null)
        {
            progressBar.SetActive(true);
            itemImage.sprite = item.icon;
            progressCoroutine = StartCoroutine(FillProgressBar());
        }
    }

    public void RemovePlat(GameObject socketedPlate = null)
    {
        expectedPlat = null;

        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }

        if (progressBar != null)
        {
            progressBar.SetActive(false);
            itemImage.sprite = null;
            progressBarImage.fillAmount = 0f;
        }

        if (socketedPlate != null)
        {
            WasteManager.Instance.UseIngredient(socketedPlate);
            Destroy(socketedPlate);
        }
    }

    private void OnPlatPlacedInSocket(SelectEnterEventArgs args)
    {
        if (expectedPlat == null)
        {
            Debug.LogWarning("Aucun plat attendu n'est défini pour cette table.");
            return;
        }

        // Récupérez le script Plat de l'objet placé
        if (!args.interactableObject.transform.TryGetComponent(out Plat placedPlat))
        {
            Debug.LogWarning("L'objet placé n'a pas de script Plat.");
            return;
        }
        GameObject plateGameObject = args.interactableObject.transform.gameObject;
        socketInteractor.enabled = false;
        bool isCorrectPlat = placedPlat.item.Equals(expectedPlat);

        OnPlatPlaced?.Invoke(isCorrectPlat);

        RemovePlat(plateGameObject);
    }

    private IEnumerator FillProgressBar()
    {
        float elapsedTime = 0f;

        while (elapsedTime < timeLimit)
        {
            elapsedTime += Time.deltaTime;
            progressBarImage.fillAmount = elapsedTime / timeLimit;
            yield return null;
        }

        Debug.LogWarning("Le temps pour placer le plat est écoulé !");
        OnPlatTimeout?.Invoke();

        if (progressBar != null)
        {
            progressBar.SetActive(false);
            itemImage.sprite = null;
            progressBarImage.fillAmount = 0f;
        }

        socketInteractor.enabled = false;
    }
}
