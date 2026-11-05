using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    [SerializeField, Min(1f)] private float baseForwardSpeed = 8f;
    [SerializeField, Min(1f)] private float laneWidth = 3f;
    [SerializeField, Min(1f)] private float laneChangeSpeed = 12f;
    [SerializeField, Min(1f)] private float jumpHeight = 2.2f;
    [SerializeField, Min(0.1f)] private float slideDuration = 0.85f;
    [SerializeField, Min(1f)] private float sprintMultiplier = 1.45f;
    [SerializeField, Min(0.1f)] private float staminaDrainPerSecond = 0.38f;
    [SerializeField, Min(0.1f)] private float staminaRecoveryPerSecond = 0.24f;

    private CharacterController characterController;
    private HealthSystem health;
    private int lane;
    private float verticalVelocity;
    private float originalHeight;
    private Vector3 originalCenter;
    private float slideTimer;
    private float damageCooldown;
    private GameManager gameManager;
    private Animator animator;
    private bool hasSpeedParameter;
    private bool hasGroundedParameter;
    private bool hasJumpParameter;
    private bool hasSlideParameter;
    private Transform visualRoot;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftLeg;
    private Transform rightLeg;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftLegRest;
    private Quaternion rightLegRest;
    private float stamina = 1f;

    public float Stamina => stamina;
    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        originalHeight = characterController.height;
        originalCenter = characterController.center;
        health = GetComponent<HealthSystem>();
        gameManager = GameManager.Instance;
    }

    private void Start()
    {
        CacheAnimator();
    }

    public void Configure(GameManager manager, HealthSystem healthSystem)
    {
        gameManager = manager;
        health = healthSystem;
    }

    public void ConfigureProceduralVisual(Transform root, Transform armLeft, Transform armRight,
        Transform legLeft, Transform legRight)
    {
        visualRoot = root;
        leftArm = armLeft;
        rightArm = armRight;
        leftLeg = legLeft;
        rightLeg = legRight;
        leftArmRest = leftArm.localRotation;
        rightArmRest = rightArm.localRotation;
        leftLegRest = leftLeg.localRotation;
        rightLegRest = rightLeg.localRotation;
    }

    private void Update()
    {
        if (damageCooldown > 0f)
            damageCooldown -= Time.deltaTime;

        if (gameManager == null || gameManager.State != RunState.Playing)
        {
            if (gameManager != null)
                gameManager.SetPlayerFootsteps(false);
            return;
        }

        ReadInput();
        UpdateSlide();

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0f;
        if (IsSprinting)
            stamina = Mathf.Max(0f, stamina - staminaDrainPerSecond * Time.deltaTime);
        else
            stamina = Mathf.Min(1f, stamina + staminaRecoveryPerSecond * Time.deltaTime);

        float speed = (baseForwardSpeed + Mathf.Min(5f, gameManager.Difficulty - 1f)) *
            (IsSprinting ? sprintMultiplier : 1f);
        float targetX = lane * laneWidth;
        float xVelocity = Mathf.Clamp((targetX - transform.position.x) * laneChangeSpeed, -laneChangeSpeed, laneChangeSpeed);

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        Vector3 movement = new Vector3(xVelocity, verticalVelocity, speed);
        characterController.Move(movement * Time.deltaTime);
        UpdateAnimation(speed);
        UpdateProceduralVisual();
        gameManager.SetPlayerFootsteps(characterController.isGrounded && slideTimer <= 0f);
    }

    private void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            lane = Mathf.Max(-1, lane - 1);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            lane = Mathf.Min(1, lane + 1);

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) &&
            characterController.isGrounded && slideTimer <= 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            if (animator != null && hasJumpParameter)
                animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            BeginSlide();
    }

    private void BeginSlide()
    {
        if (slideTimer > 0f || !characterController.isGrounded)
            return;

        slideTimer = slideDuration;
        characterController.height = originalHeight * 0.5f;
        characterController.center = new Vector3(originalCenter.x, originalCenter.y * 0.5f, originalCenter.z);
        if (animator != null && hasSlideParameter)
            animator.SetBool("Slide", true);
    }

    private void UpdateSlide()
    {
        if (slideTimer <= 0f)
            return;

        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            slideTimer = 0f;
            characterController.height = originalHeight;
            characterController.center = originalCenter;
            if (animator != null && hasSlideParameter)
                animator.SetBool("Slide", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.StartsWith("Coin_", StringComparison.Ordinal))
        {
            if (CoinManager.Instance != null)
                CoinManager.Instance.Collect(other.gameObject);
        }
        else if (other.gameObject.name.StartsWith("HealthKit_", StringComparison.Ordinal))
        {
            if (health != null)
                health.Heal(1);
            Destroy(other.gameObject);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.name.StartsWith("Obstacle_", StringComparison.Ordinal) && damageCooldown <= 0f)
        {
            if (health != null)
                health.Damage(1);
            if (gameManager != null)
                gameManager.PlayCollisionSound();
            damageCooldown = 1f;
        }
    }

    public void ResetRun()
    {
        lane = 0;
        verticalVelocity = 0f;
        slideTimer = 0f;
        damageCooldown = 0f;
        stamina = 1f;
        IsSprinting = false;
        characterController.height = originalHeight;
        characterController.center = originalCenter;
        characterController.enabled = false;
        transform.SetPositionAndRotation(new Vector3(0f, 0f, 0f), Quaternion.identity);
        characterController.enabled = true;
        if (animator != null)
        {
            if (hasSlideParameter)
                animator.SetBool("Slide", false);
            if (hasGroundedParameter)
                animator.SetBool("Grounded", true);
        }
        if (visualRoot != null)
            visualRoot.localPosition = Vector3.zero;
        if (leftArm != null)
            leftArm.localRotation = leftArmRest;
        if (rightArm != null)
            rightArm.localRotation = rightArmRest;
        if (leftLeg != null)
            leftLeg.localRotation = leftLegRest;
        if (rightLeg != null)
            rightLeg.localRotation = rightLegRest;
    }

    private void CacheAnimator()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            return;

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == "Speed" && parameters[i].type == AnimatorControllerParameterType.Float)
                hasSpeedParameter = true;
            else if (parameters[i].name == "Grounded" && parameters[i].type == AnimatorControllerParameterType.Bool)
                hasGroundedParameter = true;
            else if (parameters[i].name == "Jump" && parameters[i].type == AnimatorControllerParameterType.Trigger)
                hasJumpParameter = true;
            else if (parameters[i].name == "Slide" && parameters[i].type == AnimatorControllerParameterType.Bool)
                hasSlideParameter = true;
        }
    }

    private void UpdateAnimation(float speed)
    {
        if (animator == null)
            return;
        if (hasSpeedParameter)
            animator.SetFloat("Speed", speed);
        if (hasGroundedParameter)
            animator.SetBool("Grounded", characterController.isGrounded);
    }

    private void UpdateProceduralVisual()
    {
        if (visualRoot == null)
            return;

        if (slideTimer > 0f)
        {
            visualRoot.localPosition = new Vector3(0f, -0.35f, 0f);
            visualRoot.localScale = new Vector3(1f, 0.55f, 1f);
            return;
        }

        visualRoot.localScale = Vector3.one;
        float phase = Time.time * 12f;
        float stride = characterController.isGrounded ? Mathf.Sin(phase) : 0f;
        visualRoot.localPosition = new Vector3(0f, characterController.isGrounded ? Mathf.Abs(stride) * 0.035f : 0f, 0f);

        if (leftArm != null)
            leftArm.localRotation = leftArmRest * Quaternion.Euler(stride * 28f, 0f, 0f);
        if (rightArm != null)
            rightArm.localRotation = rightArmRest * Quaternion.Euler(-stride * 28f, 0f, 0f);
        if (leftLeg != null)
            leftLeg.localRotation = leftLegRest * Quaternion.Euler(-stride * 24f, 0f, 0f);
        if (rightLeg != null)
            rightLeg.localRotation = rightLegRest * Quaternion.Euler(stride * 24f, 0f, 0f);
    }
}
