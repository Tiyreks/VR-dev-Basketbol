using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Atılan top elde değilse birkaç saniye sonra kendi rafına geri döner.
// Böylece yerdeki topları toplamaya gerek kalmaz.
[RequireComponent(typeof(Rigidbody))]
public class AutoReturnBall : MonoBehaviour
{
    public float returnDelay = 4f;
    public float killY = -2f;
    public bool playBounceSound = true;

    private Vector3 home;
    private Quaternion homeRotation;
    private Rigidbody rb;
    private XRGrabInteractable grab;
    private float awayTime;

    private void Start()
    {
        home = transform.position;
        homeRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
    }

    private void Update()
    {
        if (grab != null && grab.isSelected)
        {
            awayTime = 0f;
            return;
        }

        if (transform.position.y < killY)
        {
            ReturnHome();
            return;
        }

        bool away = (transform.position - home).sqrMagnitude > 0.3f * 0.3f;
        awayTime = away ? awayTime + Time.deltaTime : 0f;

        if (awayTime > returnDelay)
            ReturnHome();
    }

    public void ReturnHome()
    {
        awayTime = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = home;
        rb.rotation = homeRotation;
        transform.SetPositionAndRotation(home, homeRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!playBounceSound)
            return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < 1f)
            return;

        ArcadeSound.Play(ArcadeSound.Kind.Bounce, collision.GetContact(0).point, Mathf.Clamp01(speed / 8f) * 0.6f);
    }
}
