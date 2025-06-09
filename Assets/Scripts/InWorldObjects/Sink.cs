using UnityEngine;

public class Sink : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterParticles;
    [SerializeField] private AudioSource waterSound;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IFillable fillable))
        {
            waterParticles.Play();
            waterSound.Play();
            fillable.Fill();
        }
    }
}
