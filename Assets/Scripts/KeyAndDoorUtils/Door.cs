// Door.cs
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Key Requirements")]
    [SerializeField] private int requiredLightKeys = 0;
    [SerializeField] private int requiredDarkKeys = 0;

    [Header("References")]
    [Tooltip("The trigger volume for player interaction. If empty, it will try to find one on this object.")]
    [SerializeField] private Collider interactionTrigger;

    [Header("Animation")]
    [SerializeField] private float disappearDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioSource audioSource; // optional

    // You can add locked sounds, open sounds, etc. here
    // [SerializeField] private AudioClip lockedSound;
    // [SerializeField] private AudioClip openSound;

    private bool _isOpen = false;

    private void Awake()
    {
        // If no trigger is assigned, try to get it from this GameObject
        if (interactionTrigger == null)
        {
            interactionTrigger = GetComponent<Collider>();
        }
    }

    // Called by the Player when they press 'E'
    public void TryOpen(PlayerController player)
    {
        if (_isOpen) return;

        // Check if the player has the required keys
        if (player.HasKeys(requiredLightKeys, requiredDarkKeys))
        {
            // Player has keys!
            _isOpen = true;
            
            // 1. Use the keys from player's inventory
            player.UseKeys(requiredLightKeys, requiredDarkKeys);
            
            // 2. Clear this door from the player's interaction target
            player.ClearCurrentDoor(this);
            
            // 3. Disable the interaction trigger so it can't be used again
            if (interactionTrigger != null)
            {
                interactionTrigger.enabled = false;
            }

            PlaySound(openSound);
            
            // 4. Play open sound
            // if (openSound != null) AudioSource.PlayClipAtPoint(openSound, transform.position);

            // 5. Start the smooth disappear animation
            StartCoroutine(OpenDoorSmoothly());
        }
        else
        {
            // Player does not have keys
            Debug.Log("Door is locked. Requires: " + requiredLightKeys + " Light, " + requiredDarkKeys + " Dark.");
            
            // Play locked sound
            PlaySound(lockedSound);
            // if (lockedSound != null) AudioSource.PlayClipAtPoint(lockedSound, transform.position);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    private IEnumerator OpenDoorSmoothly()
    {
        // Get the initial scale of this object itself
        Vector3 initialScale = transform.localScale;
        float timer = 0f;

        while (timer < disappearDuration)
        {
            float progress = timer / disappearDuration;
            
            // Smoothly interpolate this object's scale from initial to zero
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);
            
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure it's fully gone and disable the entire GameObject
        gameObject.SetActive(false);
    }

    // These methods are used to register this door with the player's interaction system
    private void OnTriggerEnter(Collider other)
    {
        if (_isOpen || !other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetCurrentDoor(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ClearCurrentDoor(this);
        }
    }
    
    // Return a pair of values required to access the next scene
    public int[] GetRequiredDimensionKeys()
    {
        return new int[] { requiredLightKeys, requiredDarkKeys };
    }
}