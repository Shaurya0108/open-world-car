using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Audio/Visual")]
    [SerializeField]
    [Tooltip("Sound effect played when collected")]
    private AudioSource collectSoundPrefab = null;
    [SerializeField]
    [Tooltip("Particle spawned when collected")]
    private ParticleSystem collectParticlePrefab = null;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.name);
        // check if other object has the PlayerInventory script
        PlayerInventory inventory =
            other.attachedRigidbody.GetComponent<PlayerInventory>();
        // if it does, collect
        if (inventory != null)
        {
            // we found the inventory!
            inventory.AddCollectible();
            PlayFX();
            // remove this gameObject from the scene
            Destroy(gameObject);
        }
    }

    void PlayFX()
    {
        // play vfx
        if (collectParticlePrefab != null)
        {
            // spawn a particle effect from assets
            ParticleSystem newParticle = Instantiate(collectParticlePrefab,
                transform.position, Quaternion.identity);
            newParticle.Play();
        }
        // play sfx
        SoundPlayer.Instance.PlaySFX(collectSoundPrefab, transform.position);
    }
}
