using System.Collections;
using UnityEngine;

public class ShakeAndFallPlatform : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField, Tooltip("Time (in seconds) the platform will shake before falling.")]
    private float shakeDuration = 1.0f;

    [SerializeField, Tooltip("Time (in seconds) after falling to destroy the object.")]
    private float destroyDelay = 3.0f;

    [Header("Shake")]
    [SerializeField, Tooltip("How intense the shake is. A value of 0.1 is a good start.")]
    private float shakeMagnitude = 0.1f;
    private Rigidbody rb;
    private Vector3 originalPosition;
    private bool hasBeenTriggered = false;
    private Collider platformCollider;

    [Header("Fall (transform-based)")]
    [SerializeField, Tooltip("Initial downward speed when the platform starts falling.")]
    private float fallInitialSpeed = 1.0f;

    [SerializeField, Tooltip("Downward acceleration applied while falling (positive value).")]
    private float fallAcceleration = 9.81f;

    [SerializeField, Tooltip("If true, the platform's collider will be disabled when it starts falling to avoid physics overlaps.")]
    private bool disableCollisionsOnFall = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        platformCollider = GetComponent<Collider>();
    }

    void Start()
    {
        originalPosition = transform.position;
        // Keep any Rigidbody present in kinematic mode so physics doesn't try to resolve overlaps.
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            Debug.Log("Player triggered ShakeAndFallPlatform.");
            hasBeenTriggered = true;
            StartCoroutine(ShakeAndFallSequence());
        }
    }

    private IEnumerator ShakeAndFallSequence()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            float zOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = originalPosition + new Vector3(xOffset, 0f, zOffset);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to the original position before falling
        transform.position = originalPosition;

        // Optionally disable the collider so the falling platform won't awkwardly overlap or push other colliders
        if (disableCollisionsOnFall && platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        // Transform-driven fall: move down for the duration of destroyDelay, with acceleration
        float elapsedFall = 0f;
        float velocity = fallInitialSpeed;

        while (elapsedFall < destroyDelay)
        {
            // Move down by velocity (units/s)
            transform.position += Vector3.down * velocity * Time.deltaTime;

            // Apply acceleration to velocity
            velocity += fallAcceleration * Time.deltaTime;

            elapsedFall += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
