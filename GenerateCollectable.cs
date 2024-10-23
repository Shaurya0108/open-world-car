using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCollectable : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int numberOfCollectibles = 10; // Total number of collectibles.
    [SerializeField] private Terrain terrain; // Reference to the Terrain object.

    private UIController uiController;

    void Start()
    {
        uiController = FindObjectOfType<UIController>(); // Find the UIController in the scene.
        GenerateCollectibles();
    }

    void GenerateCollectibles()
    {
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        for (int i = 0; i < numberOfCollectibles; i++)
        {
            float randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainSize.x);
            float randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainSize.z);
            float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ))
                                  + terrainPosition.y;

            Vector3 spawnPosition = new Vector3(randomX, terrainHeight, randomZ);

            // Instantiate collectible and assign callback to increment the collectible count.
            GameObject collectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);
            collectible.AddComponent<CollectibleTracker>().Initialize(this);
        }
    }

    // Method to notify the UIController when a collectible is picked up.
    public void CollectiblePicked(int collectedCount)
    {
        uiController.UpdateCollectibleCount(collectedCount);

        if (collectedCount >= numberOfCollectibles)
        {
            uiController.ShowWinText("You collected all the items!");
        }
    }
}
