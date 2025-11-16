using UnityEngine;

public class PlayerFallChecker : MonoBehaviour
{
    private AudioSource _asrc;
    [SerializeField] private AudioClip onTriggerSfx;
    void Awake()
    {
        _asrc = GetComponent<AudioSource>();
        if (_asrc == null)
        {
            Debug.Log("No AudioSource component found.");
            return;
        }
        _asrc.playOnAwake = false;
    }
    // Reset the player back to the level spawn
    private void OnTriggerEnter(Collider other) {
        Debug.Log($"Trigger entered by: {other.name}");
        if(other.CompareTag("Player")) {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player == null) {
                Debug.Log("Tagged Player has no PlayerController attached.");
                return;
            }

            if (_asrc != null && onTriggerSfx != null)
            {
                _asrc.PlayOneShot(onTriggerSfx);
            }
            
            Debug.Log("Respawning...");
            player.StartCoroutine(player.Respawn());
        }
    }
}
