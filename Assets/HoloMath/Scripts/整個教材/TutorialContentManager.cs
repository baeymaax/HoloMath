using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

[System.Serializable]
public class TutorialContent
{
    [Header("教學內容組")]
    public string contentName; // 內容名稱（用於識別）

    [Header("影片內容")]
    public VideoClip videoClip; // 教學影片

    [Header("例題內容")]
    [TextArea(3, 5)]
    public string questionText; // 例題文字

    [Header("例題圖片（可選）")]
    public Texture2D questionImage; // 例題圖片
    public bool hasImage = false; // 是否包含圖片

    [Header("3D物件")]
    public GameObject threeDObject; // 對應的3D圖形物件
}

public class TutorialContentManager : MonoBehaviour
{
    [Header("內容數據")]
    [SerializeField] private TutorialContent[] tutorialContents = new TutorialContent[5];

    [Header("控制按鈕")]
    [SerializeField] private PressableButtonHoloLens2[] controlButtons = new PressableButtonHoloLens2[5];

    [Header("目標物件")]
    [SerializeField] private VideoPlayer videoPlayer; // 影片播放器
    [SerializeField] private TextMeshPro questionText3D; // 3D例題文字 (TextMesh Pro 3D)
    [SerializeField] private Transform threeDContainer; // 3D物件容器

    [Header("例題圖片顯示")]
    [SerializeField] private GameObject imageDisplayObject; // 顯示圖片的物件（如Quad或Plane）
    [SerializeField] private Renderer imageRenderer; // 圖片渲染器

    [Header("或者使用父物件自動搜尋")]
    [SerializeField] private GameObject questionCubeParent; // 例題Cube父物件（如果不直接指定TextMeshPro）

    [Header("設定")]
    [SerializeField] private int currentContentIndex = 0; // 當前內容索引
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.cyan;

    private GameObject currentThreeDObject; // 當前顯示的3D物件

    void Start()
    {
        // 如果沒有直接指定TextMeshPro，嘗試從父物件中找到
        if (questionText3D == null && questionCubeParent != null)
        {
            questionText3D = questionCubeParent.GetComponentInChildren<TextMeshPro>();
        }

        // 初始化所有3D物件狀態（先全部隱藏）
        InitializeThreeDObjects();

        InitializeButtons();
        LoadContent(0); // 載入第一個內容
    }

    /// <summary>
    /// 初始化所有3D物件狀態（在開始時隱藏所有物件）
    /// </summary>
    private void InitializeThreeDObjects()
    {
        for (int i = 0; i < tutorialContents.Length; i++)
        {
            if (tutorialContents[i].threeDObject != null)
            {
                tutorialContents[i].threeDObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 初始化按鈕事件
    /// </summary>
    private void InitializeButtons()
    {
        for (int i = 0; i < controlButtons.Length; i++)
        {
            int index = i; // 避免閉包問題
            if (controlButtons[i] != null)
            {
                controlButtons[i].ButtonPressed.AddListener(() => OnButtonPressed(index));
            }
        }
    }

    /// <summary>
    /// 按鈕點擊事件
    /// </summary>
    /// <param name="buttonIndex">按鈕索引</param>
    public void OnButtonPressed(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < tutorialContents.Length)
        {
            LoadContent(buttonIndex);
            UpdateButtonVisual(buttonIndex);
        }
    }

    /// <summary>
    /// 載入指定索引的內容
    /// </summary>
    /// <param name="contentIndex">內容索引</param>
    private void LoadContent(int contentIndex)
    {
        if (contentIndex >= 0 && contentIndex < tutorialContents.Length)
        {
            currentContentIndex = contentIndex;
            TutorialContent content = tutorialContents[contentIndex];

            // 更新影片
            UpdateVideo(content.videoClip);

            // 更新例題文字和圖片
            UpdateQuestionContent(content.questionText, content.questionImage, content.hasImage);

            // 更新3D物件
            Update3DObject(content.threeDObject);

            Debug.Log($"已載入內容: {content.contentName}");
        }
    }

    /// <summary>
    /// 更新影片內容
    /// </summary>
    /// <param name="newVideoClip">新的影片片段</param>
    private void UpdateVideo(VideoClip newVideoClip)
    {
        if (videoPlayer != null && newVideoClip != null)
        {
            videoPlayer.clip = newVideoClip;
            videoPlayer.Prepare(); // 準備播放
        }
    }

    /// <summary>
    /// 更新例題內容（文字和圖片）
    /// </summary>
    /// <param name="newQuestionText">新的例題文字</param>
    /// <param name="questionImage">例題圖片</param>
    /// <param name="hasImage">是否有圖片</param>
    private void UpdateQuestionContent(string newQuestionText, Texture2D questionImage, bool hasImage)
    {
        // 更新文字
        UpdateQuestionText(newQuestionText);

        // 更新圖片
        UpdateQuestionImage(questionImage, hasImage);
    }

    /// <summary>
    /// 更新例題文字 (3D TextMesh Pro)
    /// </summary>
    /// <param name="newQuestionText">新的例題文字</param>
    private void UpdateQuestionText(string newQuestionText)
    {
        if (questionText3D != null)
        {
            questionText3D.text = newQuestionText;
        }
        else if (questionCubeParent != null)
        {
            // 如果直接指定的TextMeshPro為空，嘗試從父物件中找
            TextMeshPro textMesh = questionCubeParent.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = newQuestionText;
                questionText3D = textMesh; // 保存引用，下次直接使用
            }
        }
    }

    /// <summary>
    /// 更新例題圖片
    /// </summary>
    /// <param name="questionImage">例題圖片</param>
    /// <param name="hasImage">是否有圖片</param>
    private void UpdateQuestionImage(Texture2D questionImage, bool hasImage)
    {
        if (imageDisplayObject != null)
        {
            if (hasImage && questionImage != null)
            {
                // 顯示圖片
                imageDisplayObject.SetActive(true);

                if (imageRenderer != null)
                {
                    // 檢查是否與影片播放器共用同一個 Renderer
                    if (imageRenderer == videoPlayer.GetComponent<Renderer>())
                    {
                        // 共用 Renderer，暫停影片並切換到圖片
                        videoPlayer.Pause();
                        imageRenderer.material.mainTexture = questionImage;
                    }
                    else
                    {
                        // 保持原有材質，只更改貼圖
                        // 移除強制創建新材質的部分
                        if (imageRenderer.material != null)
                        {
                            imageRenderer.material.mainTexture = questionImage;
                        }
                    }
                }
            }
            else
            {
                // 隱藏圖片物件
                imageDisplayObject.SetActive(false);
            }
        }
    }


    /// <summary>
    /// 更新3D物件
    /// </summary>
    /// <param name="newThreeDObject">新的3D物件</param>
    private void Update3DObject(GameObject newThreeDObject)
    {
        // 隱藏當前3D物件
        if (currentThreeDObject != null)
        {
            currentThreeDObject.SetActive(false);
        }

        // 顯示新的3D物件
        if (newThreeDObject != null)
        {
            // 如果物件不在容器中，將其移動到容器
            if (newThreeDObject.transform.parent != threeDContainer)
            {
                newThreeDObject.transform.SetParent(threeDContainer);
                newThreeDObject.transform.localPosition = Vector3.zero;
                newThreeDObject.transform.localRotation = Quaternion.identity;
            }

            newThreeDObject.SetActive(true);
            currentThreeDObject = newThreeDObject;
        }
    }

    /// <summary>
    /// 更新按鈕視覺效果
    /// </summary>
    /// <param name="selectedIndex">選中的按鈕索引</param>
    private void UpdateButtonVisual(int selectedIndex)
    {
        for (int i = 0; i < controlButtons.Length; i++)
        {
            if (controlButtons[i] != null)
            {
                // 獲取按鈕的Renderer或Image組件來改變顏色
                var buttonRenderer = controlButtons[i].GetComponent<Renderer>();
                var buttonImage = controlButtons[i].GetComponent<Image>();

                Color targetColor = (i == selectedIndex) ? selectedButtonColor : normalButtonColor;

                if (buttonRenderer != null)
                {
                    buttonRenderer.material.color = targetColor;
                }
                else if (buttonImage != null)
                {
                    buttonImage.color = targetColor;
                }
            }
        }
    }

    /// <summary>
    /// 播放當前影片
    /// </summary>
    public void PlayCurrentVideo()
    {
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// 暫停當前影片
    /// </summary>
    public void PauseCurrentVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }

    /// <summary>
    /// 重置到第一個內容
    /// </summary>
    public void ResetToFirstContent()
    {
        LoadContent(0);
        UpdateButtonVisual(0);
    }

    /// <summary>
    /// 獲取當前內容索引
    /// </summary>
    /// <returns>當前內容索引</returns>
    public int GetCurrentContentIndex()
    {
        return currentContentIndex;
    }

    /// <summary>
    /// 獲取當前內容名稱
    /// </summary>
    /// <returns>當前內容名稱</returns>
    public string GetCurrentContentName()
    {
        if (currentContentIndex >= 0 && currentContentIndex < tutorialContents.Length)
        {
            return tutorialContents[currentContentIndex].contentName;
        }
        return "";
    }
}