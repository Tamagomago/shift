using UnityEngine;

public class Switch : MonoBehaviour
{
    public MovingPlatform platformToActivate;
    public SmoothPopup interactPopup; 

    // Materials to indicate switch state
    public Material inactiveMaterial;
    public Material activeMaterial;

    private Renderer switchRenderer;
    private bool isActive = false; // Tracks the current visual state

    void Start()
    {
        // Get the Renderer component to change material
        switchRenderer = GetComponent<Renderer>();
        if (switchRenderer != null && inactiveMaterial != null)
        {
            switchRenderer.material = inactiveMaterial;
        }
    }

    // When the 'E' key is pressed
    public void ActivateSwitch()
    {
        // Toggle the switch every time the player activates it
        isActive = !isActive;

        // Trigger the platform movement
        if (platformToActivate != null)
        {
            platformToActivate.ActivatePlatform();
            Debug.Log("Switch activated! Platform should move.");
        }

        // Change switch material based on state

        // Show or hide the popup (optional behavior)
        if (interactPopup != null)
        {
            if (isActive)
                interactPopup.Hide(); // Hide when activated
            else
                interactPopup.Show(); // Show again if toggled back
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
                if (interactPopup != null)
                {
                    interactPopup.Show();
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
                    interactPopup.Hide();
                }
            }
        }
    }
}
