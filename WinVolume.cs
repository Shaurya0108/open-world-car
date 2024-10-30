using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinVolume : MonoBehaviour
{
    private UIController UIController;
    [SerializeField] private string winText = "you win";
    [SerializeField] private AudioSource winSoundPrefab;

    private void Awake()
    {
        UIController = FindObjectOfType<UIController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //other.gameObject.SetActive(false);

        CarController car = other.attachedRigidbody.gameObject.GetComponent<CarController>();

        if (car != null)
        {
            car.Win();
        }

        if (UIController != null) {
            UIController.ShowWinText(winText);
        }

        if (winSoundPrefab != null)
        {
            SoundPlayer.Instance.PlaySFX(winSoundPrefab, transform.position);
        }
    }
}
