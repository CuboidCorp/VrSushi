using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class SmartDualHandGrabInteractable : XRGrabInteractable
{
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);

        // If there's still one interactor holding the object
        if (interactorsSelecting.Count == 1)
        {
            var remainingInteractor = interactorsSelecting[0];

            // Set attachTransform to secondaryAttach if applicable
            if (secondaryAttachTransform != null)
            {
                attachTransform = secondaryAttachTransform;
            }

            // Re-attach smoothly by updating attach position to match
            remainingInteractor.GetAttachTransform(this).SetPositionAndRotation(attachTransform.position, attachTransform.rotation);
        }
        else
        {
            // Reset to primary when nobody is holding
            attachTransform = base.attachTransform;
        }
    }
}
