using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    public Transform targetPosition;
    public float speed = 3.0f;

    [SerializeField] private float waitSeconds = 2.0f; // Wait time before moving (editable in Inspector)

    private bool isActivated = false;
    private bool movingToEnd = true; // Tracks direction of movement
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Coroutine activationCoroutine = null;

    void Start()
    {
        startPosition = transform.position;
        endPosition = targetPosition.position;
    }

    void Update()
    {
        if (isActivated)
        {
            float step = speed * Time.deltaTime;
            Vector3 target = movingToEnd ? endPosition : startPosition;

            transform.position = Vector3.MoveTowards(transform.position, target, step);

            // Stop moving once target is reached
            if (Vector3.Distance(transform.position, target) < 0.001f)
            {
                isActivated = false;
            }
        }
    }

    // Called by Switch script
    public void ActivatePlatform()
    {
        // If it's already moving, ignore input
        if (isActivated) return;

        // If a pending activation coroutine is already running, ignore duplicate activations
        if (activationCoroutine != null) return;

        activationCoroutine = StartCoroutine(StartAfterDelay());
    }

    private IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(waitSeconds);
        // Decide direction based on current position so we always move to the opposite end.
        float distToEnd = Vector3.Distance(transform.position, endPosition);
        float distToStart = Vector3.Distance(transform.position, startPosition);
        // If we're currently closer to the start, move to the end; otherwise move to the start.
        movingToEnd = distToEnd > distToStart;

        isActivated = true;
        Debug.Log("Platform activated! Moving to " + (movingToEnd ? "endPosition" : "startPosition"));

        // Clear the coroutine handle so future activations can start a new coroutine
        activationCoroutine = null;
    }
}
