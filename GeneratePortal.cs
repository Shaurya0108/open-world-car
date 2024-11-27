using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private int numberOfPortals = 5;
    [SerializeField] private Terrain terrain;
    [SerializeField] private float minDistanceBetweenPortals = 5f;
    [SerializeField] private float triggerThickness = 1f;
    [SerializeField] private GameObject portalCollectEffect;
    private UIController uiController;
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private List<GameObject> activePortals = new List<GameObject>();
    private const int COLLECTIBLES_NEEDED = 10;
    private bool previouslyMetThreshold = false;

    void Start()
    {
        uiController = FindObjectOfType<UIController>();
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory != null)
        {
            StartCoroutine(MonitorCollectibles(playerInventory));
        }
        else
        {
            Debug.LogError("PlayerInventory not found in the scene!");
        }
    }

    private IEnumerator MonitorCollectibles(PlayerInventory playerInventory)
    {
        while (true)
        {
            bool meetsThreshold = playerInventory.CollectibleCount >= COLLECTIBLES_NEEDED;

            // Portal spawning condition
            if (meetsThreshold && !previouslyMetThreshold)
            {
                GeneratePortals();
                if (uiController != null)
                {
                    uiController.ShowMessage("Portals have appeared!");
                }
            }
            // Portal despawning condition
            else if (!meetsThreshold && previouslyMetThreshold)
            {
                DespawnPortals();
                if (uiController != null)
                {
                    uiController.ShowMessage("Portals have disappeared!");
                }
            }

            previouslyMetThreshold = meetsThreshold;
            yield return new WaitForSeconds(0.5f); // Check every half second
        }
    }

    private void DespawnPortals()
    {
        foreach (GameObject portal in activePortals)
        {
            if (portal != null)
            {
                // Optional: Add despawn effect here
                Destroy(portal);
            }
        }
        activePortals.Clear();
        spawnedPositions.Clear();
    }

    void GeneratePortals()
    {
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        for (int i = 0; i < numberOfPortals; i++)
        {
            Vector3 spawnPosition;
            int maxAttempts = 100;
            int attempts = 0;

            do
            {
                float randomX = Random.Range(0f, terrainSize.x);
                float randomZ = Random.Range(0f, terrainSize.z);
                float height = terrain.SampleHeight(new Vector3(randomX + terrainPosition.x,
                                                              0f,
                                                              randomZ + terrainPosition.z));
                spawnPosition = new Vector3(randomX + terrainPosition.x,
                                          height + 3,
                                          randomZ + terrainPosition.z);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning($"Could not find valid position for portal {i}. Skipping...");
                    break;
                }
            }
            while (IsTooCloseToOtherPortals(spawnPosition) && attempts < maxAttempts);

            if (attempts < maxAttempts)
            {
                GameObject portal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);

                RaycastHit hit;
                if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit))
                {
                    portal.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }

                portal.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.World);
                portal.transform.Rotate(Vector3.right, 90f, Space.Self);

                float portalHeight = portalPrefab.GetComponent<Renderer>().bounds.size.y;
                portal.transform.position += Vector3.up * (portalHeight / 2f);

                spawnedPositions.Add(spawnPosition);
                activePortals.Add(portal);
            }
        }
    }

    bool IsTooCloseToOtherPortals(Vector3 position)
    {
        foreach (Vector3 spawnedPosition in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPosition) < minDistanceBetweenPortals)
            {
                return true;
            }
        }
        return false;
    }
}