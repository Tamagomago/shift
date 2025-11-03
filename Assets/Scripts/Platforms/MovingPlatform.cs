using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform targetPosition;
    public float speed = 3.0f;
    private bool isActivated = false;
    private Vector3 startPosition;
    private Vector3 endPosition;

    void Start()
    {
        startPosition = transform.position;
        endPosition = targetPosition.position;
    }

    void Update()
    {
        // Only move if the switch has activated the platform
        if (isActivated)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, endPosition, step);
        }
    }

    // Public function that our Switch script can call
    public void ActivatePlatform()
    {
        isActivated = true;
    }
}