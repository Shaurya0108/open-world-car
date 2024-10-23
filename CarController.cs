using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This script moves the car by applying velocity and rotations to the main
/// body rigidbody. Wheel Colliders are used for ground support and friction
/// but are not used directly. Velocity is directly applied to the Rigidbody
/// to keep movement simple and predictable, as opposed to hard setting with
/// rb.MovePosition() or opting for rb.AddForce(), which creates unpredictable
/// movement controls. Not using Kinematic or CharacterController to keep code
/// simple and maintain simple Collider interactions.
/// </summary>

public class CarController : MonoBehaviour
{
    [Header("Forward")]
    [SerializeField] [Tooltip("Max forward speed car can reach")]
    [Range(1, 100)]
    private float maxForwardSpeed = 25;
    [SerializeField] [Tooltip("Time until max speed while moving forward is reached")]
    [Range(0, 3)]
    private float accelTimeToForwardMaxSpeed = 1.5f;
    [Header("Reverse")]
    [SerializeField] [Tooltip("Max reverse speed car can reach")]
    [Range(1, 100)]
    private float maxReverseSpeed = 15;
    [SerializeField] [Tooltip("Time until max speed while moving reverse is reached")]
    [Range(0, 3)]
    private float accelTimeToReverseMaxSpeed = 1;

    [Header("Momentum")]
    [SerializeField] [Tooltip("Time until we lose all speed once input is released")]
    [Range(0, 3)]
    private float decelTimeToZeroSpeed = 1;

    [Header("Turning")]
    [Range(1, 5)]
    [SerializeField] [Tooltip("How quickly we can turn")]
    private float turnSpeed = 3;
    [SerializeField] [Tooltip("Speed at which we can still turn without input. Allows" +
        " a slight drift")]
    [Range(0, 10)]
    private float turnWhileMovingThreshold = 3;

    [Header("Aerial")]
    [SerializeField] [Range(1, 10)] [Tooltip("Increasing falls faster, decreasing makes" +
        " falls slower")]
    private float gravityMultiplier = 5;

    [Header("Events")]
    [SerializeField]
    public UnityEvent OnDeath;

    [SerializeField]
    private Transform wallDetector;

    public event Action OnStartedMovement;
    public event Action OnStoppedMovement;

    public float MoveInput { get; private set; } = 0;
    public float TurnInput { get; private set; } = 0;
    public bool IsMoving { get; private set; } = false;
    public bool IsGrounded { get; private set; } = false;
    public bool IsWallColliding { get; private set; } = false;
    public float CurrentSpeed { get; private set; } = 0;

    public float ForwardAccelRatePerSecond 
        => maxForwardSpeed / accelTimeToForwardMaxSpeed;
    public float ReverseAccelRatePerSecond
        => maxReverseSpeed / accelTimeToReverseMaxSpeed;
    public float DecelRatePerSecond 
        => maxForwardSpeed / decelTimeToZeroSpeed;
    
    private Rigidbody rb = null;
    private float groundDetectorRadius = .3f;
    private float wallDetectorRadius = .15f;

    [Header("Air Time Tracking")]
    [SerializeField]
    private UIController uiController = null;
    
    private float currentAirTime = 0f;
    private bool wasGroundedLastFrame = true;
    
	private Vector3 startPosition;
	private Quaternion startRotation;

    [Header("Boost")]
    [SerializeField] [Tooltip("How much faster the car goes when boosting")]
    [Range(1.2f, 3f)]
    private float boostMultiplier = 1.5f;
    [SerializeField] [Tooltip("How long the boost lasts")]
    [Range(1f, 10f)]
    private float boostDuration = 3f;
    
    [Header("Boost Trail")]
    [SerializeField] [Tooltip("Trail Renderer components for boost effect")]
    private TrailRenderer[] boostTrails;
    [SerializeField] [Tooltip("Color of the boost trail")]
    private Color boostTrailColor = Color.cyan;
    [SerializeField] [Tooltip("Width of the boost trail")]
    private float boostTrailWidth = 0.5f;
    [SerializeField] [Tooltip("How long the trail remains visible")]
    private float trailTime = 0.5f;
    
    private bool isBoosting = false;
    private float currentBoostTime = 0f;
    private float originalMaxForwardSpeed;
    private PlayerInventory playerInventory;

	void Start()
	{
    	// Save the initial position and rotation of the car
    	startPosition = transform.position;
    	startRotation = transform.rotation;
	}

	private void OnTriggerEnter(Collider other)
	{
    	if (other.CompareTag("Ground"))
    	{
			Debug.Log("Touched ground");
	        RestartCar();
    	}
	}

	private void RestartCar()
	{
    	Debug.Log("Restarting car!");

    	// Reset position, rotation, and velocity
    	transform.position = startPosition;
    	transform.rotation = startRotation;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}


    void Awake()
    {
        // setup our car defaults
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += new Vector3(0, -1, 0);

        // assign the wall detector if it has lots its assignment
        if(wallDetector == null)
        {
            wallDetector = transform.Find("WallDetector");
            // if it's STILL empty
            if(wallDetector == null)
            {
                Debug.LogWarning("Cannot find wall detector! Make sure you did not" +
                    "delete or rename this object, and reassign on PlayerCar");
            }
        }

        // Find UI Controller if not assigned
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
            if (uiController == null)
            {
                Debug.LogWarning("UIController not found in scene!");
            }
        }

        originalMaxForwardSpeed = maxForwardSpeed;
        playerInventory = GetComponent<PlayerInventory>();

        // Setup trail renderers if not assigned
        if (boostTrails == null || boostTrails.Length == 0)
        {
            // Look for child objects with TrailRenderer components
            boostTrails = GetComponentsInChildren<TrailRenderer>();
            if (boostTrails.Length == 0)
            {
                // Create trail objects if none exist
                CreateBoostTrails();
            }
        }

        // Initialize trails
        SetupTrailRenderers();
        // Disable trails initially
        SetTrailsActive(false);
    }

    private void CreateBoostTrails()
    {
        // Create two trail objects for left and right side of the car
        boostTrails = new TrailRenderer[2];
        
        for (int i = 0; i < 2; i++)
        {
            GameObject trailObject = new GameObject($"BoostTrail_{i}");
            trailObject.transform.parent = transform;
            
            // Position trails slightly to the left and right of the car
            float xOffset = (i == 0) ? -0.5f : 0.5f;
            trailObject.transform.localPosition = new Vector3(xOffset, 0.1f, -0.5f);
            
            boostTrails[i] = trailObject.AddComponent<TrailRenderer>();
        }
    }

    private void SetupTrailRenderers()
    {
        foreach (var trail in boostTrails)
        {
            if (trail != null)
            {
                trail.time = trailTime;
                trail.startWidth = boostTrailWidth;
                trail.endWidth = 0f;
                trail.startColor = boostTrailColor;
                trail.endColor = new Color(boostTrailColor.r, boostTrailColor.g, boostTrailColor.b, 0f);
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.generateLightingData = true;
                trail.autodestruct = false;
            }
        }
    }

    private void SetTrailsActive(bool active)
    {
        foreach (var trail in boostTrails)
        {
            if (trail != null)
            {
                trail.emitting = active;
                trail.enabled = active;
            }
        }
    }

    private void HandleBoost()
    {
        // Check for boost input and if we have enough collectibles
        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting && playerInventory.CanBoost())
        {
            ActivateBoost();
        }

        // Handle active boost
        if (isBoosting)
        {
            currentBoostTime += Time.deltaTime;
            if (currentBoostTime >= boostDuration)
            {
                DeactivateBoost();
            }
        }
    }

    private void ActivateBoost()
    {
        isBoosting = true;
        currentBoostTime = 0f;
        maxForwardSpeed = originalMaxForwardSpeed * boostMultiplier;
        playerInventory.ConsumeBoost();
        SetTrailsActive(true);
    }

    private void DeactivateBoost()
    {
        isBoosting = false;
        maxForwardSpeed = originalMaxForwardSpeed;
        SetTrailsActive(false);
    }

    void Update()
    {
        IsGrounded = CheckIfGrounded();

        // calculate movement amounts
        DetectMoveInput();
        DetectTurnInput();
        DetermineIfMoving();
		HandleBoost();
    }

    private void FixedUpdate()
    {
        IsGrounded = CheckIfGrounded();
        IsWallColliding = CheckIfWallColliding();

        // if we're moving into a wall, cut max speed
        if (IsWallColliding)
            LimitSpeedFromWall();

        // Air time tracking
        if (!IsGrounded)
        {
            currentAirTime += Time.deltaTime;
            if (uiController != null)
            {
                uiController.UpdateAirTime(currentAirTime);
            }
        }
        else if (!wasGroundedLastFrame)
        {
            // We just landed
            currentAirTime = 0f;
            if (uiController != null)
            {
                uiController.UpdateAirTime(currentAirTime);
            }
        }
        wasGroundedLastFrame = IsGrounded;

        // if we're grounded, build in natural friction
        if (IsGrounded)
            CalculateSpeed();

        // only allow is to accelerate while grounded
        if (IsGrounded)
        {
            // if forward input, accelerate forward
            if (MoveInput > 0)
                MoveForward();
            // if backward input, reverse back
            else if (MoveInput < 0)
                MoveReverse();
            else if (MoveInput == 0)
                SlowDown();
            //Debug.Log("CurrentSpeed: " + maxSpeedRatio);
        }
        else
        {
            // apply extra gravity multiplier
            ApplyExtraGravity();
        }


        // only turn if we have enough speed
        if (CurrentSpeed > turnWhileMovingThreshold
            || CurrentSpeed < -turnWhileMovingThreshold)
        {
            Turn();
        }
    }

    public void Die()
    {
        //Debug.Log("Die!");
        // trigger death event so observers and FX can respond
        OnDeath.Invoke();
        // optionally, you could delay destroying this object
        // to play death animation
        Destroy(gameObject);
    }

    public bool CheckIfGrounded()
    {
        // test a small area for all colliders present, near
        // bottom of player
        Collider[] colliders = Physics.OverlapSphere
            (transform.position, groundDetectorRadius);
        foreach(Collider collider in colliders)
        {
            // if we overlap ourself (Player) ignore
            if (collider.gameObject == this.gameObject)
            {
                continue;
            }
            // otherwise we found a non Player collider,
            // we're grounded!
            return true;
        }
        // we made it to the end. no colliders found! NOT grounded
        //Debug.Log("Grounded: False!");
        return false;

    }

    private void DetermineIfMoving()
    {
        // speed value before we're considered 'stopped' or 'moving'
        int movementSpeedThreshold = 1;
        // if our speed is greater than speedThreshold (or less because of reverse)
        // AND we weren't previously moving, we have begun moving
        if ((CurrentSpeed <= -movementSpeedThreshold
            || CurrentSpeed > movementSpeedThreshold)
            && IsMoving == false)
        {
            //Debug.Log("Moving - CurrentSpeed: " + CurrentSpeed);
            IsMoving = true;
            OnStartedMovement?.Invoke();
        }
        // if our speed is close to 0 and we were previously moving
        else if ((CurrentSpeed >= -movementSpeedThreshold
            && CurrentSpeed <= movementSpeedThreshold)
            //(CurrentSpeed >= -1 && CurrentSpeed <= 0)
            //|| (CurrentSpeed <= 1 && CurrentSpeed >= 0)
            && IsMoving == true)
        {
            //Debug.Log("Stopped - CurrentSpeed: " + CurrentSpeed);
            IsMoving = false;
            OnStoppedMovement?.Invoke();

        }
        //Debug.Log("Current Speed: " + CurrentSpeed);
    }

    private void CalculateSpeed()
    {
        // if we're moving forward
        if (MoveInput > 0)
        {
            CurrentSpeed += ForwardAccelRatePerSecond * Time.deltaTime;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed, -maxReverseSpeed, maxForwardSpeed);
        }
        // if we're moving reverse
        else if (MoveInput < 0)
        {
            CurrentSpeed -= ReverseAccelRatePerSecond * Time.deltaTime;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed, -maxReverseSpeed, maxForwardSpeed);
        }
        else if (MoveInput == 0)
        {
            // if we're slowing from forward movement
            if (CurrentSpeed > 0)
            {
                CurrentSpeed -= DecelRatePerSecond * Time.deltaTime;
                CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0, maxForwardSpeed);
            }
            // if we're slowing from backwords movement
            if (CurrentSpeed < 0)
            {
                CurrentSpeed += DecelRatePerSecond * Time.deltaTime;
                CurrentSpeed = Mathf.Clamp(CurrentSpeed, -maxReverseSpeed, 0);
            }
        }

        //Debug.Log("CurrentSpeed: " + currentSpeed);
    }

    private void DetectMoveInput()
    {
        // get move amount from up/down key input
        MoveInput = Input.GetAxisRaw("Vertical");
    }

    private void DetectTurnInput()
    {
        // get turn amount from horizontal key input
        TurnInput = Input.GetAxis("Horizontal");
    }

    private void MoveForward()
    {
        // calculate new move change
        Vector3 moveDelta = transform.forward * CurrentSpeed;
        // move the rigidbody with new move change
        //rigidbody.AddForce(moveDelta, ForceMode.Acceleration);
        //rigidbody.MovePosition(rigidbody.position + moveDelta);
        rb.velocity = new Vector3
            (moveDelta.x, rb.velocity.y, moveDelta.z);
    }

    private void MoveReverse()
    {
        // calculate new move change
        Vector3 moveDelta = transform.forward * CurrentSpeed;
        // move the rigidbody with new move change

        //rigidbody.MovePosition(rigidbody.position + moveDelta);
        //rigidbody.AddForce(moveDelta, ForceMode.Acceleration);
        rb.velocity = new Vector3
            (moveDelta.x, rb.velocity.y, moveDelta.z);
    }

    private void SlowDown()
    {
        // calculate new move change (slowing)
        Vector3 moveDelta = transform.forward * CurrentSpeed;
        // move the rigidbody with new move change
        //rigidbody.AddForce(moveDelta, ForceMode.Acceleration);
        rb.velocity = new Vector3
            (moveDelta.x, rb.velocity.y, moveDelta.z);
    }

    private void Turn()
    {
        // if we're moving forward turn normally
        if(CurrentSpeed > 0)
        {
            Quaternion rotateDelta = Quaternion.Euler(0, TurnInput * turnSpeed, 0);
            rb.MoveRotation(rb.rotation * rotateDelta);
        }
        // if we're moving backwards, reverse turning
        else if(CurrentSpeed < 0)
        {
            Quaternion rotateDelta = Quaternion.Euler(0, -TurnInput * turnSpeed, 0);
            rb.MoveRotation(rb.rotation * rotateDelta);
        }
    }

    private void ApplyExtraGravity()
    {
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void LimitSpeedFromWall()
    {
        // limit speed while against wall, but allow a little bit
        // of extra room so we don't get stuck at 0 speed and can escape
        CurrentSpeed = Mathf.Clamp(CurrentSpeed, 
            -turnWhileMovingThreshold - 1, turnWhileMovingThreshold + 1);
    }

    private bool CheckIfWallColliding()
    {
        // test a small area for all colliders present in front
        Collider[] colliders = Physics.OverlapSphere
            (wallDetector.position, wallDetectorRadius);
        foreach (Collider collider in colliders)
        {
            // if we overlap ourself (Player) ignore
            if (collider.gameObject == this.gameObject)
            {
                continue;
            }
            // if we run into a trigger, it's NOT a wall
            if (collider.isTrigger)
            {
                continue;
            }


            // otherwise we found a non Player collider,
            // and it's a wall!
            return true;
        }
        // we made it to the end. no colliders found! NOT touching a wall
        //Debug.Log("Grounded: False!");
        return false;
    }

    private void OnDrawGizmos()
    {
        // draw gizmos in scene
        Gizmos.DrawWireSphere(transform.position, groundDetectorRadius);
        if(wallDetector != null)
        {
            Gizmos.DrawWireSphere(wallDetector.position,
                wallDetectorRadius);
        }
    }
}