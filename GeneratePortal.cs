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
    private bool portalsGenerated = false;
    private const int COLLECTIBLES_NEEDED = 10;

    void Start()
    {
        uiController = FindObjectOfType<UIController>();
        // Find and subscribe to the player's inventory
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory != null)
        {
            StartCoroutine(CheckCollectibles(playerInventory));
        }
        else
        {
            Debug.LogError("PlayerInventory not found in the scene!");
        }
    }

    private IEnumerator CheckCollectibles(PlayerInventory playerInventory)
    {
        while (!portalsGenerated)
        {
            if (playerInventory.CollectibleCount >= COLLECTIBLES_NEEDED)
            {
                GeneratePortals();
                portalsGenerated = true;
                if (uiController != null)
                {
                    uiController.ShowMessage("Portals have appeared!");
                }
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // Check every half second
        }
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