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
    private Collider platformCollider;

    [Header("Fall (transform-based)")]
    [SerializeField, Tooltip("Initial downward speed when the platform starts falling.")]
    private float fallInitialSpeed = 1.0f;

    [SerializeField, Tooltip("Downward acceleration applied while falling (positive value).")]
    private float fallAcceleration = 9.81f;

    [SerializeField, Tooltip("If true, the platform's collider will be disabled when it starts falling to avoid physics overlaps.")]
    private bool disableCollisionsOnFall = true;

    [Header("Audio")]
    [SerializeField] private AudioClip shakeSound;    
    [SerializeField] private AudioSource audioSource;  // optional AudioSource

    // --- state tracking to survive disable/enable ---
    private enum PlatformState { Idle, Shaking, Falling, Finished }
    private PlatformState state = PlatformState.Idle;

    // preserved across disable/enable
    private float shakeElapsed = 0f;
    private float fallElapsed = 0f;
    private float fallVelocity = 0f;

    // guard so we don't start multiple coroutines
    private bool sequenceCoroutineRunning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        platformCollider = GetComponent<Collider>();
    }

    void Start()
    {
        originalPosition = transform.position;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // only start sequence from Idle state
        if (other.CompareTag("Player") && state == PlatformState.Idle)
        {
            Debug.Log("Player triggered ShakeAndFallPlatform.");
            // mark that we've started the sequence
            PlayShakeSound();
            state = PlatformState.Shaking;
            // initialize preserved values
            shakeElapsed = 0f;
            fallElapsed = 0f;
            fallVelocity = fallInitialSpeed;

            
            StartSequenceIfNeeded();
        }
    }
    private void PlayShakeSound()
    {
        if (shakeSound == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(shakeSound);
        else
            AudioSource.PlayClipAtPoint(shakeSound, transform.position);
    }

    private void OnEnable()
    {
        // If we were mid-sequence when disabled, resume it
        StartSequenceIfNeeded();
    }

    private void OnDisable()
    {
        // Coroutines get stopped automatically when the MonoBehaviour is disabled.
        // Mark that our sequence coroutine is not running so OnEnable can restart it.
        sequenceCoroutineRunning = false;
    }

    private void StartSequenceIfNeeded()
    {
        if (!sequenceCoroutineRunning && (state == PlatformState.Shaking || state == PlatformState.Falling))
        {
            StartCoroutine(ShakeAndFallSequence());
        }
    }

    private IEnumerator ShakeAndFallSequence()
    {
        sequenceCoroutineRunning = true;

        // --- Shaking phase (may resume mid-shake using shakeElapsed) ---
        while (shakeElapsed < shakeDuration)
        {
            // if the object was moved externally, keep originalPosition as the intended center.
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            float zOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = originalPosition + new Vector3(xOffset, 0f, zOffset);

            shakeElapsed += Time.deltaTime;
            // if object gets disabled here, coroutine will stop and OnDisable will set sequenceCoroutineRunning = false
            yield return null;
        }

        // ensure center before falling
        transform.position = originalPosition;

        // transition to falling
        state = PlatformState.Falling;

        // Optionally disable the collider once when falling starts
        if (disableCollisionsOnFall && platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        // Ensure fallVelocity has sensible value if resumed directly in Falling state
        if (fallVelocity <= 0f)
            fallVelocity = fallInitialSpeed;

        // --- Falling phase (may resume mid-fall using fallElapsed and fallVelocity) ---
        while (fallElapsed < destroyDelay)
        {
            transform.position += Vector3.down * fallVelocity * Time.deltaTime;
            fallVelocity += fallAcceleration * Time.deltaTime;
            fallElapsed += Time.deltaTime;
            yield return null;
        }

        // done
        state = PlatformState.Finished;
        sequenceCoroutineRunning = false;
        gameObject.SetActive(false);
    }

    public void ResetPlatform()
    {
        // Stop any running sequence
        StopAllCoroutines();
        sequenceCoroutineRunning = false;

        // Reset transform
        transform.position = originalPosition;

        // Reset collider
        if (platformCollider != null)
            platformCollider.enabled = true;

        // Reset Rigidbody (if any)
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset state machine
        state = PlatformState.Idle;
        shakeElapsed = 0f;
        fallElapsed = 0f;
        fallVelocity = fallInitialSpeed;
    }

}
