using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class ZombieAI : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float baseSpeed = 7f;
    [SerializeField, Min(0.1f)] private float attackRange = 1.6f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.4f;

    private Transform target;
    private CharacterController controller;
    private float nextAttackTime;
    private GameManager gameManager;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftLeg;
    private Transform rightLeg;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftLegRest;
    private Quaternion rightLegRest;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.42f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        gameManager = GameManager.Instance;
    }

    public void Configure(Transform chaseTarget, GameManager manager)
    {
        target = chaseTarget;
        gameManager = manager;
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    public void ConfigureProceduralRig(Transform armLeft, Transform armRight, Transform legLeft, Transform legRight)
    {
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
        if (target == null || gameManager == null || gameManager.State != RunState.Playing)
            return;

        Vector3 difference = target.position - transform.position;
        Vector3 flatDirection = new Vector3(difference.x, 0f, difference.z);
        float distance = flatDirection.magnitude;

        if (distance <= attackRange && Mathf.Abs(difference.x) < 1.25f)
        {
            Animate(1f);
            if (Time.time >= nextAttackTime)
            {
                HealthSystem health = target.GetComponent<HealthSystem>();
                if (health != null)
                    health.Damage(1);
                gameManager.PlayZombieSound();
                nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }

        if (distance < 0.01f)
            return;

        Vector3 direction = flatDirection / distance;
        float gap = Mathf.Max(0f, target.position.z - transform.position.z);
        float speed = baseSpeed + Mathf.Clamp((gap - 8f) * 0.22f, 0f, 5f);
        Vector3 movementDirection = AvoidObstacles(direction);
        transform.rotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        Vector3 movement = movementDirection * speed;
        if (controller.isGrounded && movement.y < 0f)
            movement.y = -2f;
        movement.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(movement * Time.deltaTime);
        Animate(0f);
    }

    private Vector3 AvoidObstacles(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        if (!Physics.SphereCast(origin, 0.4f, direction, out RaycastHit hit, 1.4f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return direction;

        if (hit.transform == target || hit.transform.IsChildOf(target))
            return direction;

        Vector3 side = Vector3.Cross(Vector3.up, direction) * (hit.normal.x >= 0f ? 1f : -1f);
        Vector3 steered = direction + side * 1.2f;
        steered.y = 0f;
        return steered.normalized;
    }

    private void Animate(float attacking)
    {
        if (leftArm == null)
            return;

        float stride = Mathf.Sin(Time.time * 8f + transform.position.z);
        float armsForward = Mathf.Lerp(stride * 22f, -48f, attacking);
        float armsSide = Mathf.Lerp(0f, 16f, attacking);
        leftArm.localRotation = leftArmRest * Quaternion.Euler(armsForward, 0f, -armsSide);
        rightArm.localRotation = rightArmRest * Quaternion.Euler(-armsForward, 0f, armsSide);
        leftLeg.localRotation = leftLegRest * Quaternion.Euler(-stride * 25f, 0f, 0f);
        rightLeg.localRotation = rightLegRest * Quaternion.Euler(stride * 25f, 0f, 0f);
    }
}
