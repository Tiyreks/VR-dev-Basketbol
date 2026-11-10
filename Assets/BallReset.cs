using UnityEngine;

public class BallReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
    }

    // Bounce sound, louder for harder hits. Very soft touches (rolling) stay silent.
    private void OnCollisionEnter(Collision collision)
    {
        float speed = collision.relativeVelocity.magnitude;

        if (speed < 0.8f)
            return;

        float volume = Mathf.Clamp01(speed / 6f);
        ArcadeSound.Play(ArcadeSound.Kind.Bounce, collision.GetContact(0).point, volume);
    }

    public void ResetBall()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
