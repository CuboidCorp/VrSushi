using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketTagFilter : XRBaseTargetFilter
{
    [Tooltip("Only objects with this tag can be inserted into the socket.")]
    public string requiredTag;

    public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
    {
        results.Clear();
        foreach (IXRInteractable target in targets)
        {
            if (target.transform != null && target.transform.CompareTag(requiredTag))
            {
                results.Add(target);
            }
        }
    }
}
