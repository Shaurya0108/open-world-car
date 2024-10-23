using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Modify CollectibleTracker.cs
// Modify CollectibleTracker.cs
public class CollectibleTracker : MonoBehaviour
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
        this.generator = generator;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory playerInventory = other.attachedRigidbody?.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            collectedCount++;
            playerInventory.AddCollectible(false); // Add false parameter to prevent UI update
            generator.CollectiblePicked(playerInventory.CollectibleCount); // Pass the actual inventory count
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
            SoundPlayer.Instance.PlaySFX(collectSoundPrefab, transform.position);
        }
    }
}