using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    void LateUpdate()
    {
        // if not yet assigned, try to find the dropper automatically
        if (target == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.dropperTransform != null)
            {
                target = GameManager.Instance.dropperTransform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: target is not assigned!");
                return;
            }
        }

        // smoothly follow the target vertically
        Vector3 desiredPos = new Vector3(transform.position.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
