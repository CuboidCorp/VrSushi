using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TwoHandGrabInteractable : XRGrabInteractable
{
    private HashSet<IXRSelectInteractor> grabCandidates = new HashSet<IXRSelectInteractor>();

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        grabCandidates.Add(args.interactorObject);

        if (grabCandidates.Count == 2)
        {
            base.OnSelectEntering(args);
        }
        else
        {
            interactionManager.CancelInteractableSelection(this as IXRSelectInteractable);
        }
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        grabCandidates.Remove(args.interactorObject);

        // Ensure standard cleanup happens
        if (isSelected && grabCandidates.Count < 2)
        {
            base.OnSelectExiting(args);
        }
    }

    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return grabCandidates.Count >= 1 || base.IsSelectableBy(interactor);
    }
}
