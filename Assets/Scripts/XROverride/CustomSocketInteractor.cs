using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CustomSocketInteractor : XRSocketInteractor
{
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    protected override void Awake()
    {
        base.Awake();

        originalLocalPosition = attachTransform.localPosition;
        originalLocalRotation = attachTransform.localRotation;
    }

    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        attachTransform.SetLocalPositionAndRotation(
            originalLocalPosition,
            originalLocalRotation);
        if (interactable is XRGrabInteractable grabInteractable && grabInteractable.gameObject.TryGetComponent(out Cuttable cuttable))
        {
            attachTransform.SetLocalPositionAndRotation(
                cuttable.cutObjectAttachPoint,
                Quaternion.Euler(cuttable.cutObjectAttachRotation));

            return attachTransform;
        }

        // Otherwise fall back to default behavior
        return base.GetAttachTransform(interactable);
    }
}
