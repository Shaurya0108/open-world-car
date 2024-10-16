using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    // allow global access
    public static SoundPlayer Instance { get; private set; }

    private void Awake()
    {
        // Singleton Pattern, ensure only one
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioSource soundPrefab, Vector3 spawnLocation)
    {
        if (soundPrefab != null
            && soundPrefab.clip != null)
        {
            // spawn sound object
            AudioSource audioSource = Instantiate
                (soundPrefab, spawnLocation, Quaternion.identity);
            // play the sound
            audioSource.Play();
            // destroy it when the sound is complete
            Destroy(audioSource.gameObject, audioSource.clip.length);
        }
    }
}
