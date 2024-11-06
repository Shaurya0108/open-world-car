using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float speedIncreasePerCollectible = 0.1f;

    [Header("Target")]
    [SerializeField] private float minimumDistanceToPlayer = 2f;

    private Transform playerTransform;
    private PlayerInventory playerInventory;
    private Rigidbody rb;
    private float currentSpeed;
    private Vector3 startPosition;
    private Terrain terrain;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = FindObjectOfType<CarController>().transform;
        playerInventory = playerTransform.GetComponent<PlayerInventory>();
        terrain = FindObjectOfType<Terrain>();
        startPosition = transform.position;
        currentSpeed = baseSpeed;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || terrain == null) return;

        // Calculate speed based on collectibles
        float targetSpeed = Mathf.Min(
            baseSpeed + (playerInventory.CollectibleCount * speedIncreasePerCollectible),
            maxSpeed
        );
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime);

        // Get direction to player
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > minimumDistanceToPlayer)
        {
            // Rotate towards player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Move towards player
            Vector3 movePosition = transform.position + transform.forward * currentSpeed * Time.deltaTime;

            // Sample terrain height at new position
            float terrainHeight = terrain.SampleHeight(movePosition) + terrain.transform.position.y;
            movePosition.y = terrainHeight + 1f; // Slight offset to stay above terrain

            rb.MovePosition(movePosition);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CarController player = collision.gameObject.GetComponent<CarController>();
        if (player != null)
        {
            StartCoroutine(ResetPositionAfterDelay(2f));
            player.RestartCarAfterHit();
        }
    }

    private IEnumerator ResetPositionAfterDelay(float delay)
    {
        enabled = false;
        yield return new WaitForSeconds(delay);
        transform.position = startPosition;
        currentSpeed = baseSpeed;
        enabled = true;
    }
}