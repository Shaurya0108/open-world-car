using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string targetSceneName; // Name of the scene to load
    [SerializeField] private float transitionDelay = 0.5f; // Optional delay before transitioning
    [SerializeField] private bool useLoadingScreen = false; // Option to use async loading

    private void Start()
    {
        // Make sure the portal has a trigger collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning("Portal needs a Collider component set to 'Is Trigger'");
        }

        // Verify the target scene exists
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set on the portal!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Player entered portal. Transitioning to {targetSceneName}...");
            StartCoroutine(TransitionToScene());
        }
    }

    private IEnumerator TransitionToScene()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(transitionDelay);

        if (useLoadingScreen)
        {
            // Start async loading
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            // Direct scene loading
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        // Create an async operation to load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);

        // Optional: Prevent scene activation until it's fully loaded
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"Loading progress: {progress * 100}%");

            // When the load is nearly complete
            if (asyncLoad.progress >= 0.9f)
            {
                // Allow scene activation
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}