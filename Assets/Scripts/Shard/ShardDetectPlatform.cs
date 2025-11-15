using UnityEngine;

public class ShardDetectPlatform : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float shardRadius = 2f; // radius to detect nearby platforms
    private DimensionManager _dimensionManager;
    private ShardPlayerFollow _shardPlayerFollow;
    private void Awake()
    {
        _dimensionManager = FindFirstObjectByType<DimensionManager>();
        _shardPlayerFollow = GetComponent<ShardPlayerFollow>();
        if (_dimensionManager == null || _shardPlayerFollow == null)
        {
            Debug.LogWarning("DimensionManager or ShardPlayerFollow is not found in the Shard GameObject!");
            return;
        }
    }

    private void Update()
    {
        // Check the opposite realm according to the active realm
        GameObject[] realmToCheck = _dimensionManager.GetOppositeRealmPlatforms();
        bool hasNearbyPlatforms = CheckNearbyPlatforms(realmToCheck);

        // TODO ADD GLOW EFFECT HERE:
        string result = hasNearbyPlatforms ? "Player is near a platform" : "No platform nearby player.";
        Debug.Log(result);
    }
    private bool CheckNearbyPlatforms(GameObject[] realmPlatforms)
    {
        foreach (GameObject platform in realmPlatforms) {
            float distance = Vector3.Distance(_shardPlayerFollow.PlayerRef.position, platform.transform.position);
            if (distance <= shardRadius) return true;
        }
        return false;
    }
}
