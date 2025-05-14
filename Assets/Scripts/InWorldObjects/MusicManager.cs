using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("Music Clips")]
    public AudioClip[] musicClips; // 8 clips, from calm to intense

    private AudioSource audioSource;
    private int currentClipIndex = -1;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;

        StartCoroutine(MusicLoopRoutine());
    }

    private IEnumerator MusicLoopRoutine()
    {
        while (DayManager.Instance != null && DayManager.Instance.CurrentTimePercent < 1f)
        {
            float rushValue = DayManager.Instance.GetClientSpawnMultiplier();

            int nextClipIndex = Mathf.Clamp(
                Mathf.FloorToInt((rushValue - 0.2f) / (1.8f / (musicClips.Length - 1))),
                0,
                musicClips.Length - 1
            );

            if (nextClipIndex != currentClipIndex)
            {
                currentClipIndex = nextClipIndex;
                audioSource.clip = musicClips[currentClipIndex];
            }

            audioSource.Play();

            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        // Day is over
        audioSource.Stop();
    }

}
