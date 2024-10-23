using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If this is not specified, " +
        "it will search for the player")]
    private Transform objectToFollow = null;
    [SerializeField]
    private Camera cam = null;

    [Header("Camera Position")]
    [SerializeField]
    [Range(15, 30)]
    private float camOffsetVerticalDistance = 20;
    [SerializeField]
    [Range(10, 25)]
    private float camOffsetBehindDistance = 20;
    [SerializeField]
    private bool lookAtObject = true;

    [Header("Smoothing")]
    [SerializeField]
    private bool useSmoothing = false;
    [SerializeField][Range(3,7)]
    private float smoothSpeed = 5;

    [Header("Camera Shake")]
    [SerializeField][Range(.01f, 1)]
    private float shakeMagnitude = .4f;

    private Vector3 cameraOffset = Vector3.zero;
    private Coroutine cameraShakeRoutine = null;

    // rotation
    [Header("Camera Rotation")]
    [SerializeField]
    [Range(1f, 10f)]
    private float rotationSpeed = 3f;
    [SerializeField]
    private bool invertMouseX = false;
    [SerializeField]
    private bool invertMouseY = false;
    [SerializeField]
    [Range(-60f, 0f)]
    private float minVerticalAngle = -30f;
    [SerializeField]
    [Range(0f, 60f)]
    private float maxVerticalAngle = 30f;

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 initialRotation;

    [SerializeField]
    [Tooltip("If true, will only rotate when right mouse button is held")]
    private bool requireMouseButton = true;

    private void Awake()
    {
        // find Player if no objectToFollow is specified
        if (objectToFollow == null)
        {
            objectToFollow = FindObjectOfType<CarController>().transform;
            Debug.LogWarning("CameraController: follow object not specified. Searching" +
                "the scene to fill with Player object, if found.");
        }
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>();
        }
        // calculate camerarig offset position
        cameraOffset = new Vector3(0, camOffsetVerticalDistance,
            -camOffsetBehindDistance);

        // Store initial rotation
        initialRotation = transform.eulerAngles;
        currentRotationX = initialRotation.y;
        currentRotationY = initialRotation.x;

        // set default positions
        UpdateCameraPosition(true);

        // ensure camera child object does not have transforms
        cam.transform.localPosition = new Vector3(0, 0, 0);
        cam.transform.Rotate(0, 0, 0);
        cam.transform.localScale = new Vector3(1, 1, 1);
    }

    private void LateUpdate()
    {
        if (objectToFollow != null)
        {
            HandleRotationInput();
            UpdateCameraPosition(false);
        }
    }

    private void HandleRotationInput()
    {
        // Check if we should process rotation input
        if (!Input.GetMouseButton(1)) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Apply inversion if needed
        mouseX *= invertMouseX ? -1 : 1;
        mouseY *= invertMouseY ? -1 : 1;

        // Update rotation
        currentRotationX += mouseX;
        currentRotationY -= mouseY; // Subtract to maintain standard mouse Y behavior

        // Clamp vertical rotation
        currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition(bool immediate)
    {
        // Create rotation quaternion
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);

        // Calculate new position based on rotated offset
        Vector3 targetPosition = objectToFollow.position + (rotation * cameraOffset);

        if (useSmoothing && !immediate)
        {
            // Smooth camera movement
            transform.position = Vector3.Lerp(transform.position,
                targetPosition, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                rotation, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Immediate position update
            transform.position = targetPosition;
            transform.rotation = rotation;
        }

        if (lookAtObject)
        {
            transform.LookAt(objectToFollow);
        }
    }

    public void ShakeCamera(float duration)
    {
        if (cameraShakeRoutine != null)
            StopCoroutine(cameraShakeRoutine);
        cameraShakeRoutine = StartCoroutine
            (ShakeCameraRoutine(duration));
    }

    IEnumerator ShakeCameraRoutine(float duration)
    {
        Vector3 originalPos = new Vector3(0,0,0);
        float elapsed = 0;
        // start our shake loop
        while(elapsed < duration)
        {
            // calculate random positions
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            // set new camera shake position
            cam.transform.localPosition = new Vector3(x, y, originalPos.z);
            // increase elapsed time
            elapsed += Time.deltaTime;

            yield return null;
        }

        cam.transform.localPosition = originalPos;
    }

}
