using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; 

public class ToggleDimension : MonoBehaviour
{
    private InputSystem_Actions _inputActions;

    [Header("Assign Realm Parents")]
    public GameObject lightRealm;
    public GameObject darkRealm;

    [Header("Transition Settings")]
    public float transitionDuration = 0.5f; 
    
    public string dissolvePropertyName = "_Dissolve";

    // Track which realm is currently active
    private bool isLightRealmActive = true;
    
    // Prevent spamming the switch
    private bool isTransitioning = false;

    // Lists to hold all materials we need to change
    private List<Material> lightRealmMaterials = new List<Material>();
    private List<Material> darkRealmMaterials = new List<Material>();

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();

        // Get and cache all materials from all child objects
        CacheAllMaterials(lightRealm, lightRealmMaterials);
        CacheAllMaterials(darkRealm, darkRealmMaterials);
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
        // Set initial state
        // Active realm is at 0 (visible), inactive realm is at 1 (dissolved)
        SetDissolve(lightRealmMaterials, isLightRealmActive ? 0 : 1);
        SetDissolve(darkRealmMaterials, isLightRealmActive ? 1 : 0);

        lightRealm.SetActive(isLightRealmActive);
        darkRealm.SetActive(!isLightRealmActive);
    }

    // Helper to find all materials in children and cache them
    private void CacheAllMaterials(GameObject parent, List<Material> materialList)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Use .materials to get an array of all material instances on this renderer
            materialList.AddRange(renderer.materials);
        }
    }

    public void OnShiftPerformed(InputAction.CallbackContext context)
    {
        // If we're not pressing the button or already switching, do nothing
        if (!context.performed || isTransitioning) return;

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

        // Start the coroutine to handle the smooth transition
        if (isLightRealmActive)
        {
            // Fade OUT Dark Realm, Fade IN Light Realm
            StartCoroutine(TransitionRealm(darkRealm, lightRealm, darkRealmMaterials, lightRealmMaterials));
        }
        else
        {
            // Fade OUT Light Realm, Fade IN Dark Realm
            StartCoroutine(TransitionRealm(lightRealm, darkRealm, lightRealmMaterials, darkRealmMaterials));
        }

        Debug.Log("Switching to " + (isLightRealmActive ? "Light Realm" : "Dark Realm"));
    }

    private IEnumerator TransitionRealm(GameObject realmToFadeOut, GameObject realmToFadeIn, List<Material> materialsOut, List<Material> materialsIn)
    {
        isTransitioning = true;
        float elapsedTime = 0f;

        // 1. Make the "Fade In" realm active so we can see its dissolve effect
        realmToFadeIn.SetActive(true);
        
        // We assume its materials are starting at dissolve = 1 (invisible)

        while (elapsedTime < transitionDuration)
        {
            // Calculate the current transition progress (0.0 to 1.0)
            float t = elapsedTime / transitionDuration;

            // Fade Out: Animate dissolve value from 0 (visible) to 1 (invisible)
            SetDissolve(materialsOut, t);

            // Fade In: Animate dissolve value from 1 (invisible) to 0 (visible)
            SetDissolve(materialsIn, 1.0f - t);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 2. Snap to final values to ensure it's perfect
        SetDissolve(materialsOut, 1.0f); // Fully dissolved
        SetDissolve(materialsIn, 0.0f);   // Fully visible

        // 3. Deactivate the "Fade Out" realm to save performance
        realmToFadeOut.SetActive(false);

        isTransitioning = false; // Allow switching again
    }

    // Helper function to set the dissolve value on a list of materials
    private void SetDissolve(List<Material> materials, float value)
    {
        foreach (Material mat in materials)
        {
            mat.SetFloat(dissolvePropertyName, value);
        }
    }
}