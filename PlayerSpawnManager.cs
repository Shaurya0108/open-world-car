using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        // Disable automatic spawning of the player prefab
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // Convert clientId to int before modulo operation
            int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);
            var spawnPoint = spawnPoints[spawnIndex];

            // Make sure we have valid spawn points
            if (spawnPoint == null)
            {
                Debug.LogError("Spawn point is null! Please assign spawn points in the inspector.");
                return;
            }

            var playerCar = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
            var networkObject = playerCar.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
        }
    }
}