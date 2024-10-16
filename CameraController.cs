using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // specify an object to follow
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

    private void Awake()
    {
        // find Player if no objectToFollow is specified
        if (objectToFollow == null)
        {
            objectToFollow = FindObjectOfType<CarController>().transform;
            Debug.LogWarning("CameraController: follow object not specified. Searching" +
                "the scene to fill with Player object, if found.");
        }
        if(cam == null)
        {
            cam = GetComponentInChildren<Camera>();
        }
        // calculate camerarig offset position
        cameraOffset = new Vector3(0, camOffsetVerticalDistance, 
            -camOffsetBehindDistance);
        // set default positions
        transform.position = objectToFollow.position + cameraOffset;
        transform.LookAt(objectToFollow);
        // ensure camera child object does not have transforms
        cam.transform.localPosition = new Vector3(0, 0, 0);
        cam.transform.Rotate(0, 0, 0);
        cam.transform.localScale = new Vector3(1, 1, 1);
    }

    private void LateUpdate()
    {
        // move camera position to maintain the original offset
        if (objectToFollow != null)
        {
            // look at object
            if (lookAtObject)
            {
                transform.LookAt(objectToFollow);
            }

            // smooth camera movement
            if (useSmoothing)
            {
                Vector3 targetPosition = objectToFollow.position + cameraOffset;
                Vector3 smoothedPosition = Vector3.Lerp(transform.position,
                    targetPosition, smoothSpeed * Time.deltaTime);
                // move the camera to the new position
                transform.position = smoothedPosition;
            }
            // otherwise, use non-smoothed movement
            else
            {
                transform.position = objectToFollow.position + cameraOffset;
            }
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
