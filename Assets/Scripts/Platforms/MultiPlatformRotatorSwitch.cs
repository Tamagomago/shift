using UnityEngine;
using System.Collections.Generic;

public class MultiPlatformRotatorSwitch : MonoBehaviour
{
    [Header("Switch Configuration")]
    [Tooltip("All objects that will rotate when this switch is activated.")]
    public List<Transform> platformsToRotate = new List<Transform>();

    [Tooltip("Rotation added each activation.")]
    public Vector3 rotationIncrement = new Vector3(0, 90, 0);

    [Tooltip("How fast each platform rotates toward its target.")]
    public float rotationSpeed = 2f;

    [Header("UI & Animation")]
    public GameObject interactPopup;
    public Animator leverAnimator;

    [Header("Audio")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioSource audioSource;

    private bool isActive = false;
    private bool isRotating = false;
    private List<Quaternion> targetRotations = new List<Quaternion>();

    void Start()
    {
        if (interactPopup != null)
            interactPopup.gameObject.SetActive(false);

        // Initialize targetRotations for each platform
        foreach (var platform in platformsToRotate)
        {
            if (platform != null)
                targetRotations.Add(platform.rotation);
        }
    }

    public void ActivateSwitch()
    {
        ActivateInternal(null);
    }

    public void ActivateSwitch(GameObject activator)
    {
        ActivateInternal(activator);
    }

    private void ActivateInternal(GameObject activator)
    {
        // ✅ Prevent activation while rotation is still ongoing
        if (isRotating)
        {
            Debug.Log($"MultiPlatformRotatorSwitch '{name}' is currently rotating — activation ignored.");
            return;
        }

        if (platformsToRotate.Count == 0)
        {
            Debug.LogWarning($"MultiPlatformRotatorSwitch '{name}' has no platforms assigned!");
            return;
        }

        isActive = !isActive;
        isRotating = true;

        // Update each platform’s next target rotation
        for (int i = 0; i < platformsToRotate.Count; i++)
        {
            if (platformsToRotate[i] == null) continue;
            targetRotations[i] = platformsToRotate[i].rotation * Quaternion.Euler(rotationIncrement);
        }

        // --- PLAY SOUND ---
        if (activateSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(activateSound);
            else
                AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }

        Debug.Log($"MultiPlatformRotatorSwitch '{name}' activated by {(activator != null ? activator.name : "direct interaction")}.");

        if (leverAnimator != null)
            leverAnimator.SetBool("isOn", isActive);

        if (interactPopup != null)
            interactPopup.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isRotating) return;

        bool allDone = true;

        for (int i = 0; i < platformsToRotate.Count; i++)
        {
            Transform platform = platformsToRotate[i];
            if (platform == null) continue;

            platform.rotation = Quaternion.Slerp(platform.rotation, targetRotations[i], Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(platform.rotation, targetRotations[i]) > 0.1f)
                allDone = false;
            else
                platform.rotation = targetRotations[i];
        }

        if (allDone)
        {
            isRotating = false;
            Debug.Log($"MultiPlatformRotatorSwitch '{name}' finished rotating; ready for next activation.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.SetCurrentSwitch(this);

        // Only show popup if not currently rotating
        if (interactPopup != null && !isRotating)
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
