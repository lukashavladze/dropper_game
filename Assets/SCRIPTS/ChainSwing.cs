using UnityEngine;

public class ChainSwing : MonoBehaviour
{
    public Transform ufo; // reference to your UFO
    public float swingAmount = 10f; // max rotation in degrees
    public float swingSpeed = 5f;   // how fast it tilts
    private Vector3 lastPosition;

    void Start()
    {
        if (ufo != null)
            lastPosition = ufo.position;
    }

    void Update()
    {
        if (ufo == null) return;

        // Calculate UFO movement direction
        float deltaX = ufo.position.x - lastPosition.x;

        // Rotate chain based on movement
        float targetZ = Mathf.Clamp(-deltaX * swingSpeed, -swingAmount, swingAmount);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, targetZ), Time.deltaTime * 5f);

        lastPosition = ufo.position;
    }
}
