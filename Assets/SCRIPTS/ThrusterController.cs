using UnityEngine;

public class ThrusterController : MonoBehaviour
{
    public ParticleSystem leftThruster;
    public ParticleSystem rightThruster;

    private Vector3 lastPos;
    private float xVel;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        // compute velocity manually
        xVel = (transform.position.x - lastPos.x) / Time.deltaTime;
        lastPos = transform.position;

        // Reverse behavior:
        // Moving right -> LEFT thruster
        // Moving left  -> RIGHT thruster
        if (xVel > 0.05f)
        {
            // moving right
            PlayIfStopped(leftThruster);
            StopIfPlaying(rightThruster);
        }
        else if (xVel < -0.05f)
        {
            // moving left
            PlayIfStopped(rightThruster);
            StopIfPlaying(leftThruster);
        }
        else
        {
            // not moving
            StopIfPlaying(leftThruster);
            StopIfPlaying(rightThruster);
        }
    }

    void PlayIfStopped(ParticleSystem ps)
    {
        if (!ps.isPlaying) ps.Play();
    }

    void StopIfPlaying(ParticleSystem ps)
    {
        if (ps.isPlaying) ps.Stop();
    }
}
