using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class StageCutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string stageKey; // e.g. "Stage1_CutscenePlayed"
    public string nextSceneName; // e.g. "Stage1_Main"

    void Start()
    {
        if (PlayerPrefs.GetInt(stageKey, 0) == 0)
        {
            // Play the cutscene for the first time
            videoPlayer.loopPointReached += OnCutsceneEnd;
            videoPlayer.Play();
        }
        else
        {
            // Skip directly to stage
            LoadStage();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            videoPlayer.Stop();
            OnCutsceneEnd(videoPlayer);
        }
    }


    void OnCutsceneEnd(VideoPlayer vp)
    {
        // Mark as watched
        PlayerPrefs.SetInt(stageKey, 1);
        PlayerPrefs.Save();

        LoadStage();
    }

    void LoadStage()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
