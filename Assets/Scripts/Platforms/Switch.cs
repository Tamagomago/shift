using UnityEngine;

public class Switch : MonoBehaviour
{
    public MovingPlatform platformToActivate;
    private bool hasBeenActivated = false;
    
    // Optional: Materials to show the switch state
    public Material inactiveMaterial;
    public Material activeMaterial;
    
    private Renderer switchRenderer;

    void Start()
    {
        // Get the Renderer component to change material
        switchRenderer = GetComponent<Renderer>();
        if (switchRenderer != null && inactiveMaterial != null)
        {
            switchRenderer.material = inactiveMaterial;
        }
    }

    // when the 'E' key is pressed.
    public void ActivateSwitch()
    {
        // Check if the switch has already been used
        if (hasBeenActivated)
        {
            return; // Do nothing if already activated
        }
        
        // Set the flag to true so this only runs once
        hasBeenActivated = true;

        if (platformToActivate != null)
        {
            platformToActivate.ActivatePlatform();
        }

        if (switchRenderer != null && activeMaterial != null)
        {
            switchRenderer.material = activeMaterial;
        }
    }

    // This function is called by Unity when another collider enters this trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the Player
        if (other.CompareTag("Player"))
        {
            // Get the PlayerController script from the player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCurrentSwitch(this);
            }
        }
    }

    // This function is called by Unity when another collider exits this trigger
    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited is the Player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ClearCurrentSwitch(this);
            }
        }
    }
}

