using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public void TogglePlayPause() // ✅ 沒有參數
    {
        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    public void Rewind5Seconds() // ✅ 沒有參數
    {
        double newTime = Mathf.Max(0f, (float)videoPlayer.time - 5f);
        videoPlayer.time = newTime;
    }

    public void Forward5Seconds() // ✅ 沒有參數
    {
        double newTime = Mathf.Min((float)videoPlayer.length, (float)videoPlayer.time + 5f);
        videoPlayer.time = newTime;
    }
}
