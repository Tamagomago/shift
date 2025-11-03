using UnityEngine;

public class PlayerFallChecker : MonoBehaviour
{
    // Reset the player back to the level spawn
    private void OnTriggerEnter(Collider other) {
        Debug.Log($"Trigger entered by: {other.name}");
        if(other.CompareTag("Player")) {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player == null) {
                Debug.Log("Tagged Player has no PlayerController attached.");
                return;
            }

            Debug.Log("Respawning...");
            player.StartCoroutine(player.Respawn());
        }
    }
}
