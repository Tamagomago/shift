using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    // Get the platform's transform (our parent)
    private Transform platformTransform;

    void Start()
    {
        platformTransform = transform.parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the player they are on this platform
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCurrentPlatform(platformTransform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the player they have left this platform
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ClearCurrentPlatform(platformTransform);
            }
        }
    }
}