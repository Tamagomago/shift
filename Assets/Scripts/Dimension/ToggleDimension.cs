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
    private DimensionManager _dimensionManager;

    // Track which realm is currently active
    public bool IsLightRealmActive { get; private set; } = true;

    // Prevent spamming the switch
    private bool isTransitioning = false;

    [Header("Dark Realm Timer Settings")]
    [SerializeField] private float timerDuration = 5f; // change later
    // [SerializeField] private TimerUI timerUI;
    private Coroutine darkRealmTimerCoroutine;
    private PlayerController _playerController;
    private bool _isTimerRunning = false;
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
        _dimensionManager = FindFirstObjectByType<DimensionManager>();

        // Find PlayerController to toggle respawn later
        _playerController = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogWarning("PlayerController not found! Make sure your player has the correct tag and script.");
            return;
        }
        // Set initial state
        // Active realm is at 0 (visible), inactive realm is at 1 (dissolved)
        SetDissolve(lightRealmMaterials, IsLightRealmActive ? 0 : 1);
        SetDissolve(darkRealmMaterials, IsLightRealmActive ? 1 : 0);

        lightRealm.SetActive(IsLightRealmActive);
        darkRealm.SetActive(!IsLightRealmActive);
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

    public void SwitchRealm(bool? targetRealm = null)
    {
        if (lightRealm == null || darkRealm == null)
        {
            Debug.LogWarning("ToggleDimension: Realms are not assigned!");
            return;
        }
            
        _dimensionManager?.PlaySwitchSound();

        // Handle external-defined realm switch (player or enemy)
        IsLightRealmActive = targetRealm ?? !IsLightRealmActive;

        // Start the coroutine to handle the smooth transition
        if (IsLightRealmActive)
        {
            // Fade OUT Dark Realm, Fade IN Light Realm
            StartCoroutine(TransitionRealm(darkRealm, lightRealm, darkRealmMaterials, lightRealmMaterials));
            // Stop any dark realm timers from running and clear flags
            if (darkRealmTimerCoroutine != null)
            {
                StopCoroutine(darkRealmTimerCoroutine);
                darkRealmTimerCoroutine = null;
            }
            _isTimerRunning = false;
        }
        else
        {
            // Fade OUT Light Realm, Fade IN Dark Realm
            StartCoroutine(TransitionRealm(lightRealm, darkRealm, lightRealmMaterials, darkRealmMaterials));
            // Start dark realm timer
            if (darkRealmTimerCoroutine != null)
            {
                StopCoroutine(darkRealmTimerCoroutine);
                darkRealmTimerCoroutine = null;
                _isTimerRunning = false;
            }
            // Start the dark realm timer only after the transition completes
            StartCoroutine(StartTimerAfterTransition());
        }

        Debug.Log("Switching to " + (IsLightRealmActive ? "Light Realm" : "Dark Realm"));
    }

    // Waits for the current transition to finish before starting the dark realm timer.
    private IEnumerator StartTimerAfterTransition()
    {
        // Wait until transition finishes (or no transition in progress)
        yield return new WaitUntil(() => !isTransitioning);

        // If the player left the dark realm before transition finished, don't start the timer
        if (IsLightRealmActive) yield break;

        // Guard again just in case
        if (_isTimerRunning) yield break;

        // darkRealmTimerCoroutine = StartCoroutine(DarkRealmTimer());
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

    private IEnumerator DarkRealmTimer()
    {
        if (_isTimerRunning) yield break;
        
        _isTimerRunning = true;
        Debug.Log("Timer started");

        // timerUI.progressSlider.maxValue = timerDuration;
        // timerUI.SetProgress(timerDuration);

        float elapsedTime = 0f;
        while (elapsedTime < timerDuration)
        {
            // If player left the dark realm (shifted back) stop early
            if (IsLightRealmActive)
            {
                Debug.Log("Timer stopped early — returned to light realm.");
                _isTimerRunning = false;
                darkRealmTimerCoroutine = null;
                yield break;
            }
            elapsedTime += Time.deltaTime;

            float remaining = timerDuration - elapsedTime;
            // timerUI.SetProgress(remaining);
            // Optional: remove or reduce frame spam logs
            Debug.Log($"elapsed: {elapsedTime}");
            yield return null;
        }

        Debug.Log("Time is up. Returning to spawn point");
        // timerUI.SetProgress(0f);
        _isTimerRunning = false;
        darkRealmTimerCoroutine = null;

        // Ensure the state correctly reflects switching back to light realm
        IsLightRealmActive = true;
        StartCoroutine(TransitionRealm(darkRealm, lightRealm, darkRealmMaterials, lightRealmMaterials));

        if (_playerController != null) _playerController.Respawn();
    }
}