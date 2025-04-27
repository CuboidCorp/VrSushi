using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class PositionReset : MonoBehaviour
{
    [Header("Reset Settings")]
    [SerializeField] private float resetDistance = 30f;
    [SerializeField] private float checkInterval = 1f; // Check every second by default

    [Header("Optional Socket Settings")]
    [SerializeField] private XRSocketInteractor targetSocket;
    [SerializeField] private Vector3 resetPosition;

    private IXRSelectInteractable grabInteractable;
    private bool isSelected = false;
    private Vector3 initialPosition;
    private Coroutine distanceCheckCoroutine;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
        initialPosition = transform.position;
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
        StopDistanceCheck();
    }

    private void Start()
    {
        // Start the distance checking coroutine
        StartDistanceCheck();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isSelected = true;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isSelected = false;
    }

    private void StartDistanceCheck()
    {
        if (distanceCheckCoroutine != null)
        {
            StopCoroutine(distanceCheckCoroutine);
        }
        distanceCheckCoroutine = StartCoroutine(DistanceCheckCoroutine());
    }

    private void StopDistanceCheck()
    {
        if (distanceCheckCoroutine != null)
        {
            StopCoroutine(distanceCheckCoroutine);
            distanceCheckCoroutine = null;
        }
    }

    private IEnumerator DistanceCheckCoroutine()
    {
        while (true)
        {
            if (!isSelected)
            {
                if (Vector3.Distance(transform.position, initialPosition) > resetDistance)
                {
                    if (targetSocket != null)
                    {
                        ReturnToSocket();
                    }
                    else
                    {
                        ResetToFallbackPosition();
                    }
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void ReturnToSocket()
    {
        Debug.Log("Targetsocket.interactablesSelected.Count: " + targetSocket.interactablesSelected.Count);
        Debug.Log("Targetsocket.CanSelect(grabInteractable): " + targetSocket.CanSelect(grabInteractable));

        if (targetSocket.interactablesSelected.Count == 0 && targetSocket.CanSelect(grabInteractable))
        {
            targetSocket.StartManualInteraction(grabInteractable);
            Debug.Log($"Started manual interaction with {targetSocket.name} for {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Target socket is not available for {gameObject.name}");
            ResetToFallbackPosition();
        }
    }

    private void ResetToFallbackPosition()
    {
        transform.SetPositionAndRotation(resetPosition, Quaternion.identity);
        initialPosition = transform.position; // Update initial position after reset
    }
}