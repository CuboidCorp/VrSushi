using UnityEngine;

public class Sink : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IFillable fillable))
        {
            fillable.Fill();
        }
    }
}
