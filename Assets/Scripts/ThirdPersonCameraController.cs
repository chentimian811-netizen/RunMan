using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minVerticalAngle = -50f;
    [SerializeField] private float maxVerticalAngle = 40f;
    [SerializeField] private float obstacleRadius = 0.3f;
    [SerializeField] private LayerMask obstacleMask = -1;

    private Transform player;
    private Vector3 smoothVelocity;
    private float verticalAngle;

    private void Start()
    {
        player = transform.parent;

        if (player == null)
        {
            Debug.LogError("ThirdPersonCameraController: no player found as parent");
            enabled = false;
            return;
        }

        transform.SetParent(null);
        transform.position = player.position;
        transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        player.Rotate(Vector3.up, mouseX);

        verticalAngle -= mouseY;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        Vector3 targetPos = player.position;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelocity, 1f / smoothSpeed);

        transform.rotation = Quaternion.Euler(verticalAngle, player.eulerAngles.y, 0);

        PushCameraForward();
    }

    private void PushCameraForward()
    {
        Vector3 dir = transform.position - player.position;
        float dist = dir.magnitude;
        if (dist < 0.01f) return;

        dir /= dist;

        float startOffset = 1f;
        float castDist = dist - startOffset;
        if (castDist <= 0) return;

        if (Physics.SphereCast(player.position + dir * startOffset, obstacleRadius, dir, out RaycastHit hit, castDist, obstacleMask))
        {
            float pushDist = Mathf.Max(hit.distance + startOffset - obstacleRadius * 0.5f, 0.1f);
            transform.position = player.position + dir * Mathf.Min(pushDist, dist);
        }
    }
}
