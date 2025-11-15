using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Transform to copy the rotation from.")]
    [SerializeField]
    private Transform targetToCopy;

    void LateUpdate()
    {
        // Check if the target has been assigned in the Inspector
        if (targetToCopy != null)
        {
            // Apply the target's rotation to this GameObject
            transform.rotation = targetToCopy.rotation;
        }
        else
        {
            // Log a warning the first time this happens to help with setup
            Debug.LogWarning("CopyRotation: Target to copy is not set!", this);
        }
    }
}