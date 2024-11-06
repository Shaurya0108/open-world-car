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
    [SerializeField] private float hitboxScale = 2f; // How much larger the hitbox should be
    private UIController uiController;
    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        uiController = FindObjectOfType<UIController>();
        GeneratePortals();
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

                // Create larger hitbox
                CreateEnhancedHitbox(portal);

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

    void CreateEnhancedHitbox(GameObject portal)
    {
        // Method 1: Create a new, larger trigger collider
        GameObject hitboxObject = new GameObject("PortalHitbox");
        hitboxObject.transform.SetParent(portal.transform);
        hitboxObject.transform.localPosition = Vector3.zero;
        hitboxObject.transform.localRotation = Quaternion.identity;

        // Add a box collider that's larger than the portal
        BoxCollider hitboxCollider = hitboxObject.AddComponent<BoxCollider>();
        BoxCollider originalCollider = portal.GetComponent<BoxCollider>();

        if (originalCollider != null)
        {
            // Copy the original collider's size and scale it up
            hitboxCollider.size = originalCollider.size * hitboxScale;
            hitboxCollider.center = originalCollider.center;
            hitboxCollider.isTrigger = true;

            // Optionally disable the original collider
            // originalCollider.enabled = false;
        }
        else
        {
            // If there's no original collider, create a new one based on the renderer
            Renderer renderer = portal.GetComponent<Renderer>();
            if (renderer != null)
            {
                hitboxCollider.size = renderer.bounds.size * hitboxScale;
                hitboxCollider.isTrigger = true;
            }
        }

        // Add a script to handle collisions if you don't already have one
        PortalTrigger triggerScript = hitboxObject.AddComponent<PortalTrigger>();
        triggerScript.parentPortal = portal;
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

    public void CollectiblePicked(int collectedCount)
    {
        if (collectedCount >= 1)
        {
            uiController.ShowWinText("You got to the portal");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
    }
}

// Add this new class to handle the portal trigger
public class PortalTrigger : MonoBehaviour
{
    public GameObject parentPortal { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Get the GeneratePortal script and notify it
            GeneratePortal portalGenerator = FindObjectOfType<GeneratePortal>();
            if (portalGenerator != null)
            {
                portalGenerator.CollectiblePicked(1);
            }

            // Optionally destroy the portal
            Destroy(parentPortal);
        }
    }
}