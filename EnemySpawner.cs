using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float timeBetweenSpawns = 30f;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float gameStartDelay = 10f;

    [Header("Spawn Area")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private float minSpawnDistance = 20f;
    [SerializeField] private float maxSpawnDistance = 30f;

    private Transform playerTransform;
    private int currentEnemyCount = 0;

    //private void Start()
    //{
    //    playerTransform = FindObjectOfType<CarController>().transform;
    //    if (terrain == null)
    //    {
    //        terrain = FindObjectOfType<Terrain>();
    //    }
    //    StartCoroutine(SpawnEnemiesOverTime());
    //}

    //private IEnumerator SpawnEnemiesOverTime()
    //{
    //    yield return new WaitForSeconds(gameStartDelay);

    //    while (true)
    //    {
    //        if (currentEnemyCount < maxEnemies)
    //        {
    //            SpawnEnemy();
    //            currentEnemyCount++;
    //        }

    //        yield return new WaitForSeconds(timeBetweenSpawns);
    //    }
    //}

    //private void SpawnEnemy()
    //{
    //    if (playerTransform == null || terrain == null) return;

    //    Vector3 terrainPosition = terrain.transform.position;
    //    Vector3 terrainSize = terrain.terrainData.size;

    //    // Get random point within spawn distance range from player
    //    Vector2 randomCircle = Random.insideUnitCircle.normalized;
    //    float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

    //    Vector3 spawnPosition = new Vector3(
    //        playerTransform.position.x + randomCircle.x * randomDistance,
    //        0,
    //        playerTransform.position.z + randomCircle.y * randomDistance
    //    );

    //    // Ensure spawn point is within terrain bounds
    //    spawnPosition.x = Mathf.Clamp(spawnPosition.x, terrainPosition.x, terrainPosition.x + terrainSize.x);
    //    spawnPosition.z = Mathf.Clamp(spawnPosition.z, terrainPosition.z, terrainPosition.z + terrainSize.z);

    //    // Sample terrain height at spawn position
    //    float terrainHeight = terrain.SampleHeight(spawnPosition) + terrainPosition.y;
    //    spawnPosition.y = terrainHeight + 1f; // Slight offset to prevent spawning inside terrain

    //    GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    //    StartCoroutine(TrackEnemyDestruction(newEnemy));
    //}

    private IEnumerator TrackEnemyDestruction(GameObject enemy)
    {
        yield return new WaitUntil(() => enemy == null);
        currentEnemyCount--;
    }
}