using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if it's the player
        if (other.CompareTag("Player"))
        {
            // 2. That's it. Load the scene.
            // The door already checked for keys.
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("NextSceneTrigger has no 'Next Scene Name' set!");
            }
        }
    }
}
