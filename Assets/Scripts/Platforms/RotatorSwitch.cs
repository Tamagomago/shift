using UnityEngine;
using TMPro; // Required namespace for TextMeshPro

/// <summary>
/// This switch, when activated, rotates a target object by a specified amount.
/// It's designed to be activated by a PlayerController.
/// Each instance of this switch can only be activated ONCE.
/// </summary>
public class RotatorSwitch : MonoBehaviour
{
    [Header("Switch Configuration")]
    public Transform objectToRotate;
    public Vector3 rotationAmount = new Vector3(0, 90, 0);
    public float rotationSpeed = 2f;

    [Header("UI & Animation")]
    public GameObject interactPopup;
    public Animator leverAnimator;

    private bool isActive = false;
    private bool isRotating = false;
    // Once set, this rotator will ignore further activations (per-instance guard)
    private bool hasActivatedOnce = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    void Start()
    {
        if (interactPopup != null)
            interactPopup.gameObject.SetActive(false);
    }

    /// <summary>
    /// Parameterless activation (e.g. direct player interaction).
    /// This instance only activates once.
    /// </summary>
    public void ActivateSwitch()
    {
        // --- MODIFIED ---
        // Check if this instance has already been activated.
        if (hasActivatedOnce)
        {
            Debug.Log($"RotatorSwitch '{name}' already activated once; ignoring direct interaction.");
            return;
        }
        // --- END MODIFICATION ---

        ActivateInternal(null);
    }

    /// <summary>
    /// Activation by an external switch GameObject.
    /// This instance only activates once.
    /// </summary>
    public void ActivateSwitch(GameObject activator)
    {
        // Check if this instance has already been activated.
        if (hasActivatedOnce)
        {
            Debug.Log($"RotatorSwitch '{name}' already activated once; ignoring activation from {(activator != null ? activator.name : "unknown")}.");
            return;
        }

        ActivateInternal(activator);
    }

    /// <summary>
    /// Internal logic to perform the activation.
    /// </summary>
    private void ActivateInternal(GameObject activator)
    {
        if (objectToRotate != null)
        {
            // Use current rotation as the new base
            initialRotation = objectToRotate.rotation;
            targetRotation = initialRotation * Quaternion.Euler(rotationAmount);
        }

        isActive = true;
        isRotating = true;
        hasActivatedOnce = true;

        Debug.Log($"RotatorSwitch '{name}' activated by {(activator != null ? activator.name : "direct interaction")} - instance locked: {hasActivatedOnce}");

        if (leverAnimator != null)
            leverAnimator.SetBool("isOn", isActive);

        if (interactPopup != null)
            interactPopup.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isRotating || objectToRotate == null) return;

        // Note: This script doesn't support de-activation. It rotates to the target and stops.
        Quaternion goalRotation = isActive ? targetRotation : initialRotation;
        objectToRotate.rotation = Quaternion.Slerp(objectToRotate.rotation, goalRotation, Time.deltaTime * rotationSpeed);

        // Stop rotating when close enough
        if (Quaternion.Angle(objectToRotate.rotation, goalRotation) < 0.1f)
        {
            objectToRotate.rotation = goalRotation;
            isRotating = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.SetCurrentSwitch(this);
        
        // Only show the popup if it hasn't been activated yet
        if (interactPopup != null && !hasActivatedOnce)
            interactPopup.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.ClearCurrentSwitch(this);
        if (interactPopup != null)
            interactPopup.gameObject.SetActive(false);
    }
}