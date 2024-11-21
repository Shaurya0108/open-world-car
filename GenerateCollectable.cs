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
            Vector3 spawnPosition = new Vector3(randomX, terrainHeight + 1, randomZ);

            // Create the collectible
            GameObject collectibleObj = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);

            // Get the Collectible component and initialize it
            Collectible collectible = collectibleObj.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.Initialize(this);
            }
            else
            {
                Debug.LogError("Collectible component not found on instantiated prefab!");
            }
        }
    }

    // Method to notify the UIController when a collectible is picked up.
    public void CollectiblePicked(int collectedCount)
    {

        if (collectedCount >= numberOfCollectibles)
        {
            uiController.ShowWinText("You collected all the items!");
        }
    }
}
