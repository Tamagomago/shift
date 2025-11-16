using UnityEngine;
using UnityEngine.Video;

public class StageCutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string stageKey; // e.g. "Stage_0_cs"

    private bool isDone = false; // Prevents skip/end from running twice

    void Start()
    {
        videoPlayer.playOnAwake = false;

        if (PlayerPrefs.GetInt(stageKey, 0) == 1)
        {
            // We've seen this cutscene. Destroy this object immediately.
            Destroy(gameObject);
        }
        else
        {
            // First time. Pause the game and play the cutscene.
            Time.timeScale = 0f; // Pauses all "Game Time" physics and Update loops
            videoPlayer.loopPointReached += HandleCutsceneEnd;
            videoPlayer.Play();
        }
    }

    void Update()
    {
        // Check if the video is playing (to avoid skipping after it's done)
        if (videoPlayer.isPlaying && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)))
        {
            HandleCutsceneEnd(videoPlayer);
        }
    }

    void HandleCutsceneEnd(VideoPlayer vp)
    {
        // --- FIX: Prevent this from running more than once ---
        if (isDone) return;
        isDone = true;

        // Unsubscribe from the event
        videoPlayer.loopPointReached -= HandleCutsceneEnd;

        // Mark as watched
        PlayerPrefs.SetInt(stageKey, 1);

        // --- THIS IS THE MAIN FIX ---
        // Unpause the game and destroy the cutscene object.
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}