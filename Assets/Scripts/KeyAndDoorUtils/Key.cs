// Key.cs
using System.Collections;
using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private KeyType keyType;
    
    [Header("Visuals")]
    [SerializeField] private ParticleSystem collectParticles;

    [Header("Animation")]
    [SerializeField] private float disappearDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;


    private bool _isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger once and only if the collider is the Player
        if (_isCollected || !other.CompareTag("Player"))
        {
            return;
        }

        // Try to get the PlayerController
        PlayerController player = other.GetComponent<PlayerController>();
        
        if (player != null)
        {
            // Mark as collected immediately to prevent re-triggering
            _isCollected = true;

            // Disable the key's collider so it can't be hit again
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // 1. Tell the player to add this key
            player.AddKey(keyType);

            if (collectSound != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(collectSound);
                else
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            // 2. Start the collection coroutine (particles + animation)
            StartCoroutine(CollectAndDisappear());
        }
    }

    private IEnumerator CollectAndDisappear()
    {
        // 1. Determine how long to wait before destroying the object
        float destroyDelay = disappearDuration;
        if (collectParticles != null)
        {
            collectParticles.Play();
            // Wait for whichever is longer: the animation or the particles
            destroyDelay = Mathf.Max(disappearDuration, collectParticles.main.duration);
        }

        // 2. Schedule the object to be destroyed
        Destroy(gameObject, destroyDelay);

        // 3. Play the smooth disappear animation
        Vector3 initialScale = transform.localScale;
        float timer = 0f;

        while (timer < disappearDuration)
        {
            // Calculate progress (0 to 1)
            float progress = timer / disappearDuration;
            
            // Smoothly interpolate the scale from initial to zero
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);
            
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 4. Ensure it's fully gone
        // The object will be destroyed by the timer set earlier
        transform.localScale = Vector3.zero;
    }
}