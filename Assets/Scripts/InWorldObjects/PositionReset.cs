using System.Collections;
using UnityEngine;

public class PositionReset : MonoBehaviour
{
    [SerializeField] private Vector3 resetPosition;

    [SerializeField] private float resetDelay = 3f;
    private Coroutine resetCoroutine;

    private void ResetObject()
    {
        transform.position = resetPosition;
    }

    public void DelayedReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(ResetCoroutine());
    }

    public void CancelReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }

    private IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetObject();
    }

}
