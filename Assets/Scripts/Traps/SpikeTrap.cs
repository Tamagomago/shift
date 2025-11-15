using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public bool spikesUp = true; // Start with spikes up

    [Header("References")]
    public Collider spikeCollider;   // The collider that should kill the player
    private Animator animator;       // Reference to the Animator

    [Header("Player Tag")]
    public string playerTag = "Player";

    private void Awake()
    {
        // Get references in Awake, as it's always called first
        animator = GetComponent<Animator>();
        if (spikeCollider == null)
        {
            spikeCollider = GetComponent<Collider>();
        }
    }

    // OnEnable() is called every time the object is set to active
    private void OnEnable()
    {
        // --- STATE SYNC ---
        // Always sync the state when this object is enabled.
        // This ensures that if the game is saved, or the object
        // is disabled and re-enabled, it's in the correct state.

        // Sync the collider's state
        if (spikeCollider != null)
        {
            spikeCollider.enabled = spikesUp;
        }

        // Sync the Animator's state
        if (animator != null)
        {
            animator.SetBool("spikesUp", spikesUp);
        }
    }

    // We no longer need the Start() function,
    // as OnEnable() handles the initial setup.

    public void ToggleTrap()
    {
        spikesUp = !spikesUp;

        // --- ANIMATION SYNC ---
        // Update the Animator with the new state
        if (animator != null)
        {
            animator.SetBool("spikesUp", spikesUp);
        }

        // Toggle deadly collider
        if (spikeCollider != null)
        {
            spikeCollider.enabled = spikesUp;
        }
        
        Debug.Log("Spike state changed. Spikes up? " + spikesUp);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If collider is disabled, this will never trigger (safe state)
        if (!spikesUp) return;

        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player hit by spike trap — triggering respawn");

            PlayerController player = other.GetComponent<PlayerController>();

            if (player == null)
            {
                Debug.LogWarning("SpikeTrap: Player hit but has no PlayerController");
                return;
            }

            player.StartCoroutine(player.Respawn());
        }
    }
}