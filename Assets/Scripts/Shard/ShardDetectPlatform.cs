using UnityEngine;

public class ShardDetectPlatform : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float shardRadius = 2f; // radius to detect nearby platforms
    private DimensionManager _dimensionManager;
    private ShardPlayerFollow _shardPlayerFollow;

    [Header("Shard Glow")]
    [SerializeField] private GameObject playerShardObject;
    [SerializeField] private float baseEmissionIntensity = 10f;
    [SerializeField] private float glowingEmissionIntensity = 50f;
    [SerializeField] private Color emissionColor = new Color(0.749f, 0.749f, 0.749f);
    private Material _shardMaterial;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _dimensionManager = FindFirstObjectByType<DimensionManager>();
        _shardPlayerFollow = GetComponent<ShardPlayerFollow>();
        Renderer shardRenderer = playerShardObject.GetComponent<Renderer>();
        _shardMaterial = shardRenderer.material;
        _shardMaterial.EnableKeyword("_EMISSION");

        if (_dimensionManager == null || _shardPlayerFollow == null)
        {
            Debug.LogWarning("DimensionManager or ShardPlayerFollow is not found in the Shard GameObject!");
            return;
        }

        SetEmission(baseEmissionIntensity);
    }

    private void Update()
    {
        // Check the opposite realm according to the active realm
        GameObject[] realmToCheck = _dimensionManager.GetOppositeRealmPlatforms();
        bool hasNearbyPlatforms = CheckNearbyPlatforms(realmToCheck);

        // TODO ADD GLOW EFFECT HERE:
        if (hasNearbyPlatforms)
        {
            SetEmission(glowingEmissionIntensity);
        }
        else
        {
            SetEmission(baseEmissionIntensity);
        }
    }

    private void SetEmission(float intensity)
    {
        Color finalColor = emissionColor * intensity;
        _shardMaterial.SetColor(EmissionColorID, finalColor);
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
