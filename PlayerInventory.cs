using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Modify PlayerInventory.cs
// Modify PlayerInventory.cs
public class PlayerInventory : MonoBehaviour
{
    private const int COLLECTIBLES_NEEDED_FOR_BOOST = 5;
    private int collectibleCount = 0;
    public int CollectibleCount => collectibleCount;
    private UIController uIController = null;

    private void Awake()
    {
        uIController = FindObjectOfType<UIController>();
    }

    // Add boolean parameter to control UI update
    public void AddCollectible(bool updateUI = true)
    {
        if (uIController == null && updateUI)
        {
            Debug.LogError("No UIController prefab in the scene. " +
                           "UIController is needed to display collectible count!");
            return;
        }

        collectibleCount++;
        Debug.Log($"Added collectible. New count: {collectibleCount}");
        
        if (updateUI)
        {
            UpdateUI();
        }
    }
    
    public bool CanBoost()
    {
        return collectibleCount >= COLLECTIBLES_NEEDED_FOR_BOOST;
    }

    public void ConsumeBoost()
    {
        if (CanBoost())
        {
            collectibleCount -= COLLECTIBLES_NEEDED_FOR_BOOST;
            Debug.Log($"Used boost! Remaining collectibles: {collectibleCount}");
            UpdateUI();
            uIController.ShowBoostMessage();
        }
    }

    private void UpdateUI()
    {
        if (uIController != null)
        {
            uIController.UpdateCollectibleCount(collectibleCount);
        }
    }
}