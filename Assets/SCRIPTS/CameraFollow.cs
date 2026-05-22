using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    [HideInInspector]
    public Vector3 shakeOffset;

    private Vector3 basePosition; //  stable position

    void LateUpdate()
    {
        if (target == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.dropperTransform != null)
                target = GameManager.Instance.dropperTransform;
            else
                return;
        }

        //  Calculate PERFECT base position (no shake)
        Vector3 desiredPos = new Vector3(
            0f, 
            target.position.y + offset.y,
            transform.position.z
        );

        // smooth follow ONLY base position
        basePosition = Vector3.Lerp(
            basePosition == Vector3.zero ? desiredPos : basePosition,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        //  FINAL POSITION = base + shake
        transform.position = basePosition + shakeOffset;
    }
}