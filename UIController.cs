using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI Text Elements")]
    [SerializeField] Text collectibleTextUI = null;
    [SerializeField] Text winTextUI = null;
    [SerializeField] Text AirTimeCount = null;
    [SerializeField] Text boostMessageUI = null;
    [SerializeField] Text plusOneTextUI = null;
    [SerializeField] Text minusFiveTextUI = null;
    [SerializeField] Text minusTenTextUI = null;
    [SerializeField] private Text spinRewardTextUI = null;

    [Header("Display Durations")]
    [SerializeField] private float boostMessageDuration = 1.5f;
    [SerializeField] private float plusOneDisplayDuration = 1.0f;
    [SerializeField] private float minusDisplayDuration = 1.5f;
    [SerializeField] private float spinRewardDisplayDuration = 1.5f;

    public void ShowBoostMessage()
    {
        if (boostMessageUI != null)
        {
            boostMessageUI.text = "BOOST ACTIVATED!";
            boostMessageUI.gameObject.SetActive(true);
            StartCoroutine(HideBoostMessage());
        }
    }
    
    private IEnumerator HideBoostMessage()
    {
        yield return new WaitForSeconds(boostMessageDuration);
        if (boostMessageUI != null)
        {
            boostMessageUI.gameObject.SetActive(false);
        }
    }

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

	public void ShowPlusOne(Vector3 collectiblePosition)
	{
    	if (plusOneTextUI != null)
    	{
        	plusOneTextUI.gameObject.SetActive(true);
        
        	// Optional: You can position the +1 text at the collectible's screen position
        	Vector3 screenPos = Camera.main.WorldToScreenPoint(collectiblePosition);
        	plusOneTextUI.transform.position = screenPos;
        
        	StartCoroutine(HidePlusOne());
    	}
	}

	private IEnumerator HidePlusOne()
	{
    	yield return new WaitForSeconds(plusOneDisplayDuration);
    	if (plusOneTextUI != null)
    	{
        	plusOneTextUI.gameObject.SetActive(false);
    	}
	}
    public void ShowMinusFive()
    {
        if (minusFiveTextUI != null)
        {
            minusFiveTextUI.gameObject.SetActive(true);
            StartCoroutine(HideText(minusFiveTextUI));
        }
    }

    public void ShowMinusTen()
    {
        if (minusTenTextUI != null)
        {
            minusTenTextUI.gameObject.SetActive(true);
            StartCoroutine(HideText(minusTenTextUI));
        }
    }

    private IEnumerator HideText(Text textUI)
    {
        yield return new WaitForSeconds(minusDisplayDuration);
        if (textUI != null)
        {
            textUI.gameObject.SetActive(false);
        }
    }

    public void ShowSpinReward(string rewardText)
    {
        if (spinRewardTextUI != null)
        {
            spinRewardTextUI.text = rewardText;
            spinRewardTextUI.gameObject.SetActive(true);
            StartCoroutine(HideSpinReward());
        }
    }

    private IEnumerator HideSpinReward()
    {
        yield return new WaitForSeconds(spinRewardDisplayDuration);
        if (spinRewardTextUI != null)
        {
            spinRewardTextUI.gameObject.SetActive(false);
        }
    }
}