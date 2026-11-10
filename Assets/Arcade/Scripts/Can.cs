using UnityEngine;

// Kuledeki tek bir kutu. Başlangıç yerini hatırlar ki kule yeniden dizilebilsin.
[RequireComponent(typeof(Rigidbody))]
public class Can : MonoBehaviour
{
    [HideInInspector] public bool counted;

    private Rigidbody rb;
    private Vector3 homePosition;
    private Quaternion homeRotation;

    public Rigidbody Body => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    // Yerinden oynamış mı? (devrildi, kaydı ya da düştü)
    public bool IsDisplaced =>
        (transform.position - homePosition).sqrMagnitude > 0.05f * 0.05f ||
        Vector3.Angle(transform.up, homeRotation * Vector3.up) > 20f;

    public bool IsResting => rb.IsSleeping() || rb.linearVelocity.sqrMagnitude < 0.01f;

    public void ResetCan()
    {
        counted = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = homePosition;
        rb.rotation = homeRotation;
        transform.SetPositionAndRotation(homePosition, homeRotation);
        rb.Sleep();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float speed = collision.relativeVelocity.magnitude;
        if (speed < 1f)
            return;

        ArcadeSound.Play(ArcadeSound.Kind.Clang, transform.position, Mathf.Clamp01(speed / 6f) * 0.7f);
    }
}
