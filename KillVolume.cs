using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillVolume : MonoBehaviour
{
    private UIController UIController;
    [SerializeField] private string dieText = "you die";
    [SerializeField] private AudioSource dieSoundPrefab;

    private void Awake()
    {
        UIController = FindObjectOfType<UIController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.attachedRigidbody.gameObject.GetComponent<CarController>();

        if (car != null)
        {
            car.Die();
        }

        if (dieText != null)
        {
            UIController.ShowWinText(dieText);
        }

        if (dieSoundPrefab != null)
        {
            SoundPlayer.Instance.PlaySFX(dieSoundPrefab, transform.position);
        }
    }
}
