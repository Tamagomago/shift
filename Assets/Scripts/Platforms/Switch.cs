using UnityEngine;
using TMPro; // ++ ADDED: Required namespace for TextMeshPro

public class Switch : MonoBehaviour
{
    public MovingPlatform platformToActivate;
    public RotatorSwitch rotatorToActivate; // optional rotator target

    // The popup object shown when the player can interact
    public GameObject interactPopup; 

    public Animator leverAnimator; 
    private bool isActive = false; // Tracks the current state

        // --- AUDIO ---
    [Header("Audio")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioSource audioSource;


    void Start()
    {
        // ++ ADDED: It's good practice to ensure the popup is hidden when the game starts.
        if (interactPopup != null)
        {
            interactPopup.gameObject.SetActive(false);
        }
    }

    // When the 'E' key is pressed
    public void ActivateSwitch()
    {
        // Toggle the switch every time the player activates it
        isActive = !isActive;

        // Play sound on activation
        if (activateSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(activateSound);
            else
                AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }

        // Trigger the platform movement
        if (platformToActivate != null)
        {
            platformToActivate.ActivatePlatform();
            Debug.Log("Switch activated! Platform should move.");
        }

        // Trigger a rotator target if assigned (pass this switch as the activator)
        if (rotatorToActivate != null)
        {
            rotatorToActivate.ActivateSwitch(this.gameObject);
            Debug.Log($"Switch activated rotator: {rotatorToActivate.name}");
        }

        // Tell the Animator to change state
        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isOn", isActive);
        }

        // Show or hide the popup (optional behavior)
        if (interactPopup != null)
        {
            if (isActive)
                // ++ CHANGED: Use .gameObject.SetActive(false) to hide
                interactPopup.gameObject.SetActive(false); // Hide when activated
            else
                // ++ CHANGED: Use .gameObject.SetActive(true) to show
                interactPopup.gameObject.SetActive(true); // Show again if toggled back
        }
    }

    // Called when the player enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCurrentSwitch(this);

                // Show popup when near the switch
                // ++ MODIFIED: Also check if the switch isn't already active
                if (interactPopup != null && !isActive) 
                {
                    // ++ CHANGED: Use .gameObject.SetActive(true) to show
                    interactPopup.gameObject.SetActive(true);
                }
            }
        }
    }

    // Called when the player exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ClearCurrentSwitch(this);

                // Hide the popup when the player walks away
                if (interactPopup != null)
                {
                    // ++ CHANGED: Use .gameObject.SetActive(false) to hide
                    interactPopup.gameObject.SetActive(false);
                }
            }
        }
    }
}