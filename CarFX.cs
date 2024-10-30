using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Animator))]
public class CarFX : MonoBehaviour
{
    [Header("Auidio/Visual")]
    [SerializeField]
    TrailRenderer tireTrailLeft = null;
    [SerializeField]
    TrailRenderer tireTrailRight = null;
    [SerializeField]
    private ParticleSystem deathParticlePrefab = null;

    [SerializeField]
    private ParticleSystem winParticlePrefab = null;

    [Header("Audio")]
    [SerializeField]
    private AudioSource deathSoundPrefab = null;
    private AudioSource winSoundPrefab = null;

    private Animator animator;

    private const string moveParameterName = "isMoving";
    private CarController carController;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        animator = GetComponent<Animator>();
        // add FX to the Death event
        carController.OnDeath.AddListener(PlayDeathVFX);
        carController.OnDeath.AddListener(PlayDeathSFX);

        carController.OnWin.AddListener(PlayWinVFX);
        carController.OnWin.AddListener(PlayWinSFX);
    }

    private void OnEnable()
    {
        // start watching events
        carController.OnStartedMovement += OnStartedMoving;
        carController.OnStoppedMovement += OnStoppedMoving;
    }

    private void OnDisable()
    {
        // stop watching events
        carController.OnStartedMovement -= OnStartedMoving;
        carController.OnStoppedMovement -= OnStoppedMoving;
    }

    private void Update()
    {
        HandleTireTrails();
    }

    private void HandleTireTrails()
    {
        // if there's no tire trail defined, exit early
        if (tireTrailLeft == null || tireTrailRight == null)
            return;

        // if max speed, and trail isn't already active, turn it on
        if (carController.IsGrounded
            && tireTrailLeft.emitting == false
            && tireTrailRight.emitting == false)
        {
            Debug.Log("Enable trails!");
            tireTrailLeft.emitting = true;
            tireTrailRight.emitting = true;
        }
        // if not max speed, and trail IS active, turn it off
        if (carController.IsGrounded == false
            && tireTrailLeft.emitting == true
            && tireTrailRight.emitting == true)
        {
            Debug.Log("Disable trails...");
            tireTrailLeft.emitting = false;
            tireTrailRight.emitting = false;
        }
    }

    private void OnStartedMoving()
    {
        // if we don't have a controller in the animator, just exit this method
        if (animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetBool(moveParameterName, true);
    }

    private void OnStoppedMoving()
    {
        // if we don't have a controller in the animator, just exit this method
        if (animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetBool(moveParameterName, false);
    }

    private void PlayDeathVFX()
    {
        // spawn death particle
        if (deathParticlePrefab != null)
        {
            // spawn particle object
            ParticleSystem deathParticle = Instantiate(deathParticlePrefab,
                transform.position, Quaternion.identity);
            deathParticle.Play();
        }
    }

    private void PlayWinVFX()
    {
        // spawn win particle
        if (winParticlePrefab != null)
        {
            // spawn particle object
            ParticleSystem winParticle = Instantiate(winParticlePrefab,
                transform.position, Quaternion.identity);
            winParticle.Play();
        }
    }

    private void PlayDeathSFX()
    {
        SoundPlayer.Instance.PlaySFX(deathSoundPrefab, transform.position);
    }

    private void PlayWinSFX()
    {
        SoundPlayer.Instance.PlaySFX(winSoundPrefab, transform.position);
    }
}
