using UnityEngine;

/// <summary>
/// Attach this script to a World Space Canvas or UI element
/// to make it always face the main camera.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;
    }

    // Using LateUpdate ensures all camera movement for the frame has finished
    void LateUpdate()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("LookAtCamera: Main Camera is not found!");
            return;
        }

        // Make this object's rotation match the camera's rotation
        transform.rotation = mainCamera.transform.rotation;
        
        // --- Alternative (if it faces the wrong way) ---
        // This will point the object's "forward" (Z) axis at the camera
        // transform.LookAt(mainCamera.transform);
    }
}