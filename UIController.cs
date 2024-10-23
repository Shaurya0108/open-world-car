using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] Text collectibleTextUI = null;
    [SerializeField] Text winTextUI = null;
    [SerializeField] Text AirTimeCount = null;

    void Start()
    {
        HideWinText();
        if (AirTimeCount != null)
        {
            UpdateAirTime(0f);
        }
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
        collectibleTextUI.text = collectibleCount.ToString();
    }

    public void UpdateAirTime(float airTime)
    {
        if (AirTimeCount != null)
        {
            AirTimeCount.text = $"Air Time:\n{airTime:F2}s";
        }
    }
}