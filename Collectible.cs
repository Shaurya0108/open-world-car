using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private static int collectedCount = 0;
    private GenerateCollectable generator;

    [Header("Audio/Visual")]
    [SerializeField]
    [Tooltip("Sound effect played when collected")]
    private AudioSource collectSoundPrefab = null;
    [SerializeField]
    [Tooltip("Particle spawned when collected")]
    private ParticleSystem collectParticlePrefab = null;

    public void Initialize(GenerateCollectable generator)
    {
        if (generator == null)
        {
            Debug.LogError("Trying to initialize Collectible with null generator!");
            return;
        }
        this.generator = generator;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (generator == null)
        {
            Debug.LogError("Generator reference is null! Make sure Initialize is called.");
            return;
        }

        PlayerInventory playerInventory = other.attachedRigidbody?.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            collectedCount++;
            playerInventory.AddCollectible(true);
            generator.CollectiblePicked(playerInventory.CollectibleCount);

            var uiController = FindObjectOfType<UIController>();
            if (uiController != null)
            {
                uiController.ShowPlusOne(transform.position);
            }

            PlayFX();
            Destroy(gameObject);
        }
    }

    void PlayFX()
    {
        if (collectParticlePrefab != null)
        {
            ParticleSystem newParticle = Instantiate(collectParticlePrefab,
                transform.position, Quaternion.identity);
            newParticle.Play();
        }
        if (collectSoundPrefab != null)
        {
            Debug.Log($"SoundPlayer exists: {SoundPlayer.Instance != null}, Sound prefab has clip: {collectSoundPrefab.clip != null}");
            SoundPlayer.Instance.PlaySFX(collectSoundPrefab, transform.position);
        }
    }
}