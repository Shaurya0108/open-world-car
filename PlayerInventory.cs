using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    int collectibleCount = 0;

    UIController uIController = null;

    private void Awake()
    {
        uIController = FindObjectOfType<UIController>();
    }

    public void AddCollectible()
    {
        if (uIController == null)
        {
            Debug.LogError("No UIController prefab in the scene. " +
                "UIController is needed to display collectible count!");
            return;
        }

        // update the count
        collectibleCount = collectibleCount + 1;
        uIController.UpdateCollectibleCount(collectibleCount);
    }
}
