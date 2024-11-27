using UnityEngine;
using System.Collections.Generic;

public class TruckSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject truckPrefab;
    [SerializeField] private Terrain terrain;
    [SerializeField] private int initialTruckCount = 3;
    [SerializeField] private float minDistanceFromPlayer = 20f;
    [SerializeField] private float minDistanceBetweenTrucks = 15f;

    [Header("Spawn Triggers")]
    [SerializeField] private int collectiblesPerNewTruck = 1;

    private PlayerInventory playerInventory;
    private Transform playerTransform;
    private List<GameObject> activeTrucks = new List<GameObject>();
    private int lastSpawnThreshold = 0;

    private void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        playerTransform = FindObjectOfType<CarController>().transform;

        if (playerInventory == null || terrain == null || truckPrefab == null)
        {
            Debug.LogError("TruckSpawner: Missing required references!");
            enabled = false;
            return;
        }

        // Spawn initial trucks
        for (int i = 0; i < initialTruckCount; i++)
        {
            SpawnTruck();
        }
    }

    private void Update()
    {
        int currentThreshold = playerInventory.CollectibleCount / collectiblesPerNewTruck;

        // Spawn new truck when player crosses threshold
        if (currentThreshold > lastSpawnThreshold)
        {
            SpawnTruck();
            lastSpawnThreshold = currentThreshold;
        }
    }

    private void SpawnTruck()
    {
        Vector3 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition != Vector3.zero)
        {
            GameObject truck = Instantiate(truckPrefab, spawnPosition, Quaternion.identity);
            activeTrucks.Add(truck);
        }
    }

    private Vector3 FindValidSpawnPosition(int maxAttempts = 30)
    {
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate random position within terrain bounds
            float randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainSize.x);
            float randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainSize.z);

            // Sample terrain height at position
            float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ))
                                + terrainPosition.y;

            Vector3 potentialPosition = new Vector3(randomX, terrainHeight + 1f, randomZ);

            // Check if position is valid
            if (IsValidSpawnPosition(potentialPosition))
            {
                return potentialPosition;
            }
        }

        Debug.LogWarning("Could not find valid spawn position for truck after " + maxAttempts + " attempts");
        return Vector3.zero;
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        // Check distance from player
        if (Vector3.Distance(position, playerTransform.position) < minDistanceFromPlayer)
        {
            return false;
        }

        // Check distance from other trucks
        foreach (GameObject truck in activeTrucks)
        {
            if (truck != null && Vector3.Distance(position, truck.transform.position) < minDistanceBetweenTrucks)
            {
                return false;
            }
        }

        return true;
    }

    // Call this when a truck is destroyed or deactivated
    public void RemoveTruck(GameObject truck)
    {
        if (activeTrucks.Contains(truck))
        {
            activeTrucks.Remove(truck);
        }
    }
}