using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public SpikeTrap[] connectedTraps;   // Multiple traps
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("PressurePlate is missing an Animator component!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("OnPress");
            Debug.Log("Pressure plate pressed down.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("OnRelease");

            // Toggle ALL connected traps
            foreach (var trap in connectedTraps)
            {
                if (trap != null)
                    trap.ToggleTrap();
            }

            Debug.Log("Pressure plate released — all traps toggled!");
        }
    }
}
