using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleDimension : MonoBehaviour
{
    private InputSystem_Actions _inputActions;
    [Header("Assign Realm Parents")]
    public GameObject lightRealm;
    public GameObject darkRealm;

    // Track which realm is currently active
    private bool isLightRealmActive = true;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.ShiftDimension.performed += OnShiftPerformed;
    }
    private void OnDisable()
    {
        _inputActions.Player.ShiftDimension.performed -= OnShiftPerformed;
        _inputActions.Player.Disable();
    }

    private void Start()
    {
        // Toggle realms
        lightRealm.SetActive(isLightRealmActive);
        darkRealm.SetActive(!isLightRealmActive);
    }

    public void OnShiftPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        SwitchRealm();
    }

    private void SwitchRealm()
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
}
