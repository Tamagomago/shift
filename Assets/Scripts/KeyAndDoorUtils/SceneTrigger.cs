using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName; // Set this in the Inspector
    private Door _door;
    private int[] _requiredKeys;
    void Start()
    {
        _door = FindFirstObjectByType<Door>();
        if (_door == null)
        {
            Debug.Log("No Door component found.");
            return;
        }

        _requiredKeys = _door.GetRequiredDimensionKeys();
        if (_requiredKeys.Length < 2) return;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController _player = other.GetComponent<PlayerController>();
            if (_player == null) return;
    
            bool playerHasCompleteKeys = _player.HasKeys(_requiredKeys[0], _requiredKeys[1]);
            if (playerHasCompleteKeys)
            {
                _player.UseKeys(_requiredKeys[0], _requiredKeys[1]);
                SceneManager.LoadScene(nextSceneName);
            } 
            else
            {
                Debug.Log("Insufficient Keys.");
            }
        }
    }
}
