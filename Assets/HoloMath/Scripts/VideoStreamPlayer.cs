using UnityEngine;
using UnityEngine.Video;

public class VideoStreamPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Prepare(); // 預先載入影片
            videoPlayer.prepareCompleted += OnPrepared;
        }
    }

    void OnPrepared(VideoPlayer vp)
    {
        vp.Play(); // 預載完畢後播放
    }
}
