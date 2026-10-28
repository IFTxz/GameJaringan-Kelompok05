using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySFXOnInteract : MonoBehaviour
{
    public AudioClip interactionSFX;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.maxDistance = 15f;

        if (interactionSFX != null)
        {
            audioSource.clip = interactionSFX;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (audioSource.clip != null)
            {
                audioSource.Play();
                Debug.Log("SFX diputar: " + audioSource.clip.name);
            }
            else
            {
                Debug.LogWarning("Audio Clip belum dipasang!");
            }
        }
    }
}