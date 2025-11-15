using UnityEngine;

public class DimensionManager : MonoBehaviour
{
    private ToggleDimension _toggleDimension ; // Reference to the toggle dimension script

    public bool CurrentSceneDimension { get; private set; }

    public ToggleDimension ToggleDimensionRef => _toggleDimension; // Give access to other scripts later on

    void Awake()
    {
        _toggleDimension = FindFirstObjectByType<ToggleDimension>();

        if (_toggleDimension == null)
        {
            Debug.Log("No `ToggleDimension` script found. ");
        }

        CurrentSceneDimension = _toggleDimension.IsLightRealmActive;
    }

}
