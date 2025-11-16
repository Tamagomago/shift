using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public SpikeTrap[] connectedTraps;   // Multiple traps
    private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("PressurePlate is missing an Animator component!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        animator.SetTrigger("OnPress");

        // --- PLAY PRESS SOUND ---
        if (pressSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(pressSound);
            else
                AudioSource.PlayClipAtPoint(pressSound, transform.position);
        }

        Debug.Log("Pressure plate pressed down.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        animator.SetTrigger("OnRelease");

        // --- PLAY RELEASE SOUND ---
        if (releaseSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(releaseSound);
            else
                AudioSource.PlayClipAtPoint(releaseSound, transform.position);
        }

        // Toggle ALL connected traps
        foreach (var trap in connectedTraps)
        {
            if (trap != null)
                trap.ToggleTrap();
        }

        Debug.Log("Pressure plate released — all traps toggled!");
    }
}
