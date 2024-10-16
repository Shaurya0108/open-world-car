using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] Text collectibleTextUI = null;
    [SerializeField] Text winTextUI = null;

    void Start()
    {
        HideWinText();
    }

    public void HideWinText()
    {
        winTextUI.text = "";
        winTextUI.gameObject.SetActive(false);
    }

    public void ShowWinText(string textToShow)
    {
        winTextUI.text = textToShow;
        winTextUI.gameObject.SetActive(true);
    }

    public void UpdateCollectibleCount(int collectibleCount)
    {
        // add animation here
        collectibleTextUI.text = collectibleCount.ToString();
    }
}
