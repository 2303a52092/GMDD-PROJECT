using UnityEngine;

public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 4.5f, -8f);
    [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.2f, 3f);

    private Transform target;
    private Vector3 velocity;

    public void Configure(Transform followTarget)
    {
        target = followTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target.position + lookOffset);
    }
}
