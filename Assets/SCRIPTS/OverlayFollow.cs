using UnityEngine;

public class OverlayFollow : MonoBehaviour
{
    public Transform cameraTransform;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(
            0f,
            cameraTransform.position.y,
            transform.position.z
        );
    }
}