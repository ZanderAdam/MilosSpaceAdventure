using UnityEngine;

/// <summary>
/// Smooth camera follow for the player ship using SmoothDamp.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 _velocity;

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            smoothTime
        );
    }
}
