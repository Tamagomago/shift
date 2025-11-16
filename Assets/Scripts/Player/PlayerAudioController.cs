using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    private AudioSource _asrc;

    public AudioClip playerWalk;
    public AudioClip playerJump;

    float GenerateRandomPitch(float min = 0.9f, float max = 1.1f) => UnityEngine.Random.Range(min, max);

    void Start()
    {
        _asrc = GetComponent<AudioSource>();
        if (_asrc == null)
        {
            Debug.Log("No AudioSource component found.");
            return;
        }
        _asrc.playOnAwake = false;
    }

    public void PlayFootstep()
    {
        _asrc.pitch = GenerateRandomPitch();
        _asrc.PlayOneShot(playerWalk);
        Debug.Log("PlayerWalk sound triggered");
    }
    public void PlayJump()
    {
        _asrc.pitch = GenerateRandomPitch();
        _asrc.PlayOneShot(playerJump);
    }
}
