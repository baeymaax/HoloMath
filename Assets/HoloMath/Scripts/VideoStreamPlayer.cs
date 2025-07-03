using UnityEngine;
using UnityEngine.Video;

public class VideoStreamPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip videoClip; // 影片檔案（從 Assets 拖進來）

    void Start()
    {
        if (videoPlayer != null && videoClip != null)
        {
            videoPlayer.clip = videoClip; // 使用內嵌影片
            videoPlayer.Prepare();        // 預先載入影片
            videoPlayer.prepareCompleted += OnPrepared;
        }
    }

    void OnPrepared(VideoPlayer vp)
    {
        vp.Play(); // 預載完畢後播放
    }
}
