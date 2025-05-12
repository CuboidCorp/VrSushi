using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class ValveKnobController : MonoBehaviour
{
    [SerializeField] private Transform rotationPart;
    private XRBaseInteractable interactable;


    [Header("Rotation Settings")]
    [SerializeField] private float onThreshold = 5f;  // Within 5° of 0 = ON
    [SerializeField] private float offThreshold = 5f; // Within 5° of 90 = OFF

    [Header("Events")]
    public UnityEvent onValveTurnedOn;
    public UnityEvent onValveTurnedOff;

    private bool isActive = false;
    private float currentAngle;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

    }

    private void OnEnable()
    {
        interactable.activated.AddListener(OnRemoteTurn);
    }

    private void OnDisable()
    {
        interactable.activated.RemoveListener(OnRemoteTurn);
    }

    void Update()
    {
        ClampRotation();
        CheckStateChange();
    }

    void ClampRotation()
    {
        float angle = rotationPart.localEulerAngles.y;

        if (angle > 180f) angle -= 360f;

        float clamped = Mathf.Clamp(angle, 0f, 90f);
        rotationPart.localEulerAngles = new Vector3(0f, clamped, 0f);

        currentAngle = clamped;
    }

    void CheckStateChange()
    {
        if (!isActive && currentAngle <= onThreshold)
        {
            isActive = true;
            onValveTurnedOn?.Invoke();
        }
        else if (isActive && currentAngle >= 90f - offThreshold)
        {
            isActive = false;
            onValveTurnedOff?.Invoke();
        }
    }


    private void OnRemoteTurn(ActivateEventArgs _)
    {
        float targetAngle = isActive ? 90f : 0f;
        rotationPart.localEulerAngles = new Vector3(0f, targetAngle, 0f);
        currentAngle = targetAngle;
    }


}
