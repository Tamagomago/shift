using UnityEngine;

public class ToggleDimension : MonoBehaviour
{
    [Header("Assign Realm Parents")]
    public GameObject lightRealm;
    public GameObject darkRealm;

    // Track which realm is currently active
    private bool isLightRealmActive = true;

    // Called by ShiftInputListener or other triggers
    public void SwitchRealm()
    {
        if (lightRealm == null || darkRealm == null)
        {
            Debug.LogWarning("ToggleDimension: Realms are not assigned!");
            return;
        }

        isLightRealmActive = !isLightRealmActive;

        // Toggle realms
        lightRealm.SetActive(isLightRealmActive);
        darkRealm.SetActive(!isLightRealmActive);

        Debug.Log("Switched to " + (isLightRealmActive ? "Light Realm" : "Dark Realm"));
    }

    private void Start()
    {
        // Ensure only one realm is active at start
        lightRealm.SetActive(isLightRealmActive);
        darkRealm.SetActive(!isLightRealmActive);
    }
}
