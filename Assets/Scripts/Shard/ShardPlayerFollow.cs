using UnityEngine;

public class ShardPlayerFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(-0.6f, 3f, -1.5f); // Offset the shard to the top left of the player's shoulder
    [SerializeField] private float followSpeed = 3f;

    // LateUpdate ensures parent (player) updates position first before updating the shard's position
    void LateUpdate()
    {
        if (player == null) return;
        // Calculate target position based on player position
        Vector3 targetPos = player.position + player.rotation * offset;
        // Move to target position
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
