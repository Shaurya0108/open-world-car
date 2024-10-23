using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleTracker : MonoBehaviour
{
    private static int collectedCount = 0; // Track collectibles
    private GenerateCollectable generator;

    public void Initialize(GenerateCollectable generator)
    {
        this.generator = generator;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody?.GetComponent<PlayerInventory>() != null)
        {
            collectedCount++;
            generator.CollectiblePicked(collectedCount); // Notify generator
            Destroy(gameObject); // Destroy the collectible.
        }
    }
}
