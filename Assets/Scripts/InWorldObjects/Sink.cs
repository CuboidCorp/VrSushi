using UnityEngine;

public class Sink : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterParticles;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IFillable fillable))
        {
            waterParticles.Play();
            fillable.Fill();
        }
    }
}
