using UnityEngine;
using System.Collections.Generic;

public class DimensionManager : MonoBehaviour
{
    private ToggleDimension _toggleDimension ; // Reference to the toggle dimension script
    // Store positions of the realm platforms
    public GameObject[] lightRealmPlatformPos;
    public GameObject[] darkRealmPlatformPos;
    public bool CurrentSceneDimension { get; private set; }

    public ToggleDimension ToggleDimensionRef => _toggleDimension; // Give access to other scripts later on

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip switchSound;
    [Range(0f, 1f)]
    [SerializeField] private float switchVolume = 1f;

    void Awake()
    {
        // Get the ToggleDimension script for access later
        _toggleDimension = FindFirstObjectByType<ToggleDimension>();

        if (_toggleDimension == null)
        {
            Debug.Log("No `ToggleDimension` script found. ");
            return;
        }

        CurrentSceneDimension = _toggleDimension.IsLightRealmActive;
    }

    public void PlaySwitchSound()
    {
        if (audioSource == null)
        {
            // Create a default AudioSource on the same GameObject
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (switchSound != null)
            audioSource.PlayOneShot(switchSound, switchVolume);
    }

    void Start()
    {
        if (_toggleDimension == null) return;

        lightRealmPlatformPos = GetRealmPlatformPositions(true);
        darkRealmPlatformPos = GetRealmPlatformPositions(false);

        // Debug.Log($"Light platforms count: {lightRealmPlatformPos.Length}");
        // foreach (var platform in lightRealmPlatformPos) { Debug.Log($"Light Platform: {platform.name}, World: {platform.transform.position}, Local: {platform.transform.localPosition}"); }

        // Debug.Log($"Dark platforms count: {darkRealmPlatformPos.Length}");
        // foreach (var platform in darkRealmPlatformPos) { Debug.Log($"Light Platform: {platform.name}, World: {platform.transform.position}, Local: {platform.transform.localPosition}"); }

    }

    public GameObject[] GetOppositeRealmPlatforms()
    {
        return _toggleDimension.IsLightRealmActive ? darkRealmPlatformPos : lightRealmPlatformPos;
    }
    
    public GameObject[] GetRealmPlatformPositions(bool isLightRealm)
    {
        GameObject parent = isLightRealm ? _toggleDimension.lightRealm : _toggleDimension.darkRealm;
        // Parent GameObject not found
        if (parent == null) return new GameObject[0];
        // Loop through and create an array of transforms for their positions in the game world
        List<GameObject> platformPositions = new List<GameObject>();
        string tagToCompare = isLightRealm ? "LightRealm" : "DarkRealm";
        
        for(int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            RecursivePlatformFetch(child, tagToCompare, platformPositions);
        }

        return platformPositions.ToArray();
    }

    public void RecursivePlatformFetch(Transform current, string tagToCompare, List<GameObject> platformPos)
    {
        // Base case - child game object of a realm is marked as a realm platform
        if (current.CompareTag(tagToCompare))
        {
            platformPos.Add(current.gameObject);
        }

        // Recusively fetch platforms with realm labels from nested game objects 
        for(int i = 0; i < current.childCount; i++)
        {
            RecursivePlatformFetch(current.GetChild(i), tagToCompare, platformPos);
        }
    }
}
