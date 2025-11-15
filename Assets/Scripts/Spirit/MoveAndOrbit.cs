using UnityEngine;

/// <summary>Move to a target on player collision, then spiral out and orbit.</summary>
public class MoveAndOrbit : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The transform of the target location to move to and orbit around.")]
    [SerializeField]
    private Transform targetLocation;

    [Header("Movement Settings")]
    [Tooltip("Movement speed towards the target.")]
    [SerializeField]
    private float movementSpeed = 5f;

    [Tooltip("Distance threshold to consider arrived.")]
    [SerializeField]
    private float arrivalThreshold = 0.1f;

    [Header("Orbit Settings")]
    [Tooltip("Orbit speed (deg/s).")]
    [SerializeField]
    private float orbitSpeed = 30f;

    [Tooltip("Orbit radius around the target.")]
    [SerializeField]
    private float orbitRadius = 3f;

    [Tooltip("Radius expansion speed; uses movementSpeed if 0.")]
    [SerializeField]
    private float expandSpeed = 0f;

    // State
    private bool isActivated = false;
    private bool isAtTarget = false; // arrived at center
    private float currentOrbitAngle = 0f;
    private float currentOrbitRadius = 0f;
    private Vector3 lastMoveDirection = Vector3.forward;

    /// <summary>Trigger enter handler.</summary>
    /// <param name="other">Collider that entered.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Activate on player collision
        if (!isActivated && other.CompareTag("Player"))
        {
            if (targetLocation == null)
            {
                Debug.LogError("MoveAndOrbit: Target Location is not set!", this);
                return;
            }
            Debug.Log("Player collision detected. Activating movement.");
            isActivated = true;
        }
    }

    /// <summary>Frame update.</summary>
    void Update()
    {
        // Early out if not active or missing target
        if (!isActivated || targetLocation == null)
        {
            return;
        }
        // STAGE 1: Move to center
        if (!isAtTarget)
        {
            // Direction and distance to target
            Vector3 direction = (targetLocation.position - transform.position);
            float distance = direction.magnitude;
            // Store approach direction
            if (distance > 0.001f)
            {
                lastMoveDirection = direction.normalized;
            }
            // Check arrival
            if (distance <= arrivalThreshold)
            {
                Debug.Log("Arrived at target center. Starting spiral-out orbit.");
                isAtTarget = true;
                transform.position = targetLocation.position;
                currentOrbitAngle = Mathf.Atan2(lastMoveDirection.z, lastMoveDirection.x) * Mathf.Rad2Deg;
                currentOrbitRadius = 0f;
            }
            else
            {
                // Move towards target
                transform.position += lastMoveDirection * movementSpeed * Time.deltaTime;
            }
        }
        // STAGE 2: Spiral out and orbit
        else
        {
            // Determine expansion speed
            float effectiveExpandSpeed = (expandSpeed > 0f) ? expandSpeed : movementSpeed;
            // Expand radius smoothly
            if (currentOrbitRadius < orbitRadius)
            {
                currentOrbitRadius = Mathf.MoveTowards(currentOrbitRadius, orbitRadius, effectiveExpandSpeed * Time.deltaTime);
            }
            else
            {
                // Clamp it just in case
                currentOrbitRadius = orbitRadius; 
            }
            // Update angle
            currentOrbitAngle += orbitSpeed * Time.deltaTime;
            // Calculate new position on XZ plane
            float xOffset = Mathf.Cos(currentOrbitAngle * Mathf.Deg2Rad) * currentOrbitRadius;
            float zOffset = Mathf.Sin(currentOrbitAngle * Mathf.Deg2Rad) * currentOrbitRadius;
            // Apply position relative to target
            transform.position = targetLocation.position + new Vector3(xOffset, 0, zOffset);
        }
    }
}