using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class PlayIntroCutscene : MonoBehaviour
{
    private VideoPlayer _videoPlayer = null;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        _videoPlayer.Play();
        // when video is done playing, call the Load level method
        _videoPlayer.loopPointReached += LoadNextLevel;
    }

    void LoadNextLevel(VideoPlayer videoPlayer)
    {
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentBuildIndex + 1);
    }
}
