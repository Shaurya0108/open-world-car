using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Terrain terrain; // Reference to the Terrain object
    [SerializeField] private GameObject player; // Direct reference to the player object

    [Header("Reset Settings")]
    [SerializeField] private float resetHeight = -10f; // Height at which player should be reset
    [SerializeField] private float searchRadius = 10f; // Radius to search for valid terrain position
    [SerializeField] private int searchResolution = 8; // Number of points to check in the radius
    [SerializeField] private float heightOffset = 2f; // How high above terrain to place player

    private void Update()
    {
        // Check if player has fallen below reset height
        if (HasPlayerFallen())
        {
            ResetPlayerPosition();
        }
    }

    private bool HasPlayerFallen()
    {
        if (player != null)
        {
            return player.transform.position.y < resetHeight;
        }
        Debug.LogWarning("Player reference is missing in Ocean script!");
        return false;
    }

    private void ResetPlayerPosition()
    {
        if (player == null || terrain == null)
        {
            Debug.LogError("Missing required references in Ocean script!");
            return;
        }

        Vector3 playerPos = player.transform.position;
        Vector3 bestPosition = FindNearestTerrainPosition(playerPos);

        // Reset player position and zero out their velocity if they have a Rigidbody
        player.transform.position = bestPosition;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 FindNearestTerrainPosition(Vector3 fromPosition)
    {
        Vector3 bestPosition = fromPosition;
        float lowestDistance = float.MaxValue;

        // Search in a circle around the player's position
        for (int i = 0; i < searchResolution; i++)
        {
            float angle = i * (360f / searchResolution);
            float radian = angle * Mathf.Deg2Rad;

            // Calculate position on the search circle
            Vector3 searchPos = fromPosition + new Vector3(
                Mathf.Cos(radian) * searchRadius,
                0f,
                Mathf.Sin(radian) * searchRadius
            );

            // Sample height at this position
            float terrainHeight = terrain.SampleHeight(searchPos) + terrain.transform.position.y;
            Vector3 potentialPosition = new Vector3(
                searchPos.x,
                terrainHeight + heightOffset,
                searchPos.z
            );

            // Calculate distance on xz plane only
            float distance = Vector2.Distance(
                new Vector2(fromPosition.x, fromPosition.z),
                new Vector2(potentialPosition.x, potentialPosition.z)
            );

            // Update best position if this is closer
            if (distance < lowestDistance)
            {
                lowestDistance = distance;
                bestPosition = potentialPosition;
            }
        }

        return bestPosition;
    }
}