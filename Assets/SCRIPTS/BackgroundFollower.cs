using UnityEngine;

public class BackgroundFollower : MonoBehaviour
{
    public Transform cameraTransform;
    public float followSpeed = 2f;

    //void LateUpdate()
    //{
    //    if (cameraTransform == null) return;
    //    Vector3 newPos = transform.position;
    //    newPos.y = Mathf.Lerp(transform.position.y, cameraTransform.position.y, followSpeed * Time.deltaTime);
    //    transform.position = newPos;
    //}
}
