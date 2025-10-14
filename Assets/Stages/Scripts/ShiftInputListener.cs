using UnityEngine;
using UnityEngine.Events;

public class ShiftInputListener : MonoBehaviour
{
    [Header("Key to trigger dimension shift")]
    public KeyCode shiftKey = KeyCode.LeftShift;

    [Header("Event triggered when shift key is pressed")]
    public UnityEvent OnShiftPressed;

    private void Update()
    {
        if (Input.GetKeyDown(shiftKey))
        {
            // Trigger the event when the shift key is pressed
            OnShiftPressed?.Invoke();
        }
    }
}

/*
 * NOTE:
 * This script is temporary and serves as a placeholder for testing.
 * It listens for the shift key input before a player controller is added.
 * Later, this will be integrated into the player's ability system or input manager.
 */
