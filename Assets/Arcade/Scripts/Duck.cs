using UnityEngine;

// Raylarda gidip gelen ördek. Topla vurulunca geriye devrilir, biraz sonra tekrar kalkar.
// Kök obje kinematik Rigidbody ile hareket eder (çarpışma için), "pivot" devrilme, "model" yön için döner.
[RequireComponent(typeof(Rigidbody))]
public class Duck : MonoBehaviour
{
    public int points = 1;

    [Header("Hareket (ebeveyne göre yerel X)")]
    public float minX = -3f;
    public float maxX = 0f;
    public float speed = 1f;
    public float phase = 0f;

    [Header("Parçalar")]
    public Transform pivot;
    public Transform model;

    public float downTime = 1.5f;

    [HideInInspector] public DuckGame game;

    private Rigidbody rb;
    private Collider hitCollider;
    private Vector3 baseLocal;
    private float downTimer;
    private float tilt;

    public bool IsDown => downTimer > 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        hitCollider = GetComponent<Collider>();
        baseLocal = transform.localPosition;
    }

    private void FixedUpdate()
    {
        float range = Mathf.Max(0.01f, maxX - minX);
        float t = Time.time * speed + phase;
        float x = minX + Mathf.PingPong(t, range);
        bool movingRight = Mathf.Repeat(t, range * 2f) < range;

        Vector3 local = new Vector3(x, baseLocal.y, baseLocal.z);
        rb.MovePosition(transform.parent != null ? transform.parent.TransformPoint(local) : local);

        if (model != null)
            model.localRotation = Quaternion.Euler(0f, movingRight ? 90f : -90f, 0f);
    }

    private void Update()
    {
        if (downTimer > 0f)
        {
            downTimer -= Time.deltaTime;
            if (downTimer <= 0f && hitCollider != null)
                hitCollider.enabled = true;
        }

        float target = IsDown ? 80f : 0f;
        tilt = Mathf.MoveTowards(tilt, target, (IsDown ? 600f : 250f) * Time.deltaTime);

        if (pivot != null)
            pivot.localRotation = Quaternion.Euler(tilt, 0f, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsDown)
            return;

        Rigidbody other = collision.rigidbody;
        if (other == null || other.isKinematic)
            return;

        if (collision.relativeVelocity.magnitude < 0.8f)
            return;

        Hit(collision.GetContact(0).point);
    }

    public void Hit(Vector3 point)
    {
        downTimer = downTime;

        if (hitCollider != null)
            hitCollider.enabled = false;

        ArcadeSound.Play(ArcadeSound.Kind.Quack, point, 0.9f);

        if (game != null)
            game.OnDuckHit(this, point);
    }
}
