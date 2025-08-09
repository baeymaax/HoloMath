using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 元素")]
    public Button startLearningButton;
    public Button settingsButton;
    public Button aboutButton;
    public Button exitButton;

    [Header("文字元素")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI welcomeText;

    [Header("特效")]
    public GameObject[] floatingObjects;

    [Header("場景設定")]
    public string classroomSceneName = "TestScene0623";

    [Header("音效")]
    public AudioClip buttonClickSound;
    public AudioClip backgroundMusic;

    private AudioSource audioSource;

    void Start()
    {
        InitializeMenu();
        SetupButtonEvents();
    }

    void InitializeMenu()
    {
        // 添加 AudioSource 組件
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 設置歡迎文字
        if (welcomeText != null)
        {
            string userName = PlayerPrefs.GetString("UserName", "");
            welcomeText.text = $"歡迎, {userName}!";
        }

        // 播放背景音樂
        if (backgroundMusic != null && audioSource != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.3f;
            audioSource.Play();
        }

        StartFloatingAnimations();
    }

    void SetupButtonEvents()
    {
        if (startLearningButton != null)
            startLearningButton.onClick.AddListener(StartLearning);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (aboutButton != null)
            aboutButton.onClick.AddListener(ShowAbout);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitApplication);
    }

    public void StartLearning()
    {
        Debug.Log("=== 開始學習按鈕被點擊！ ===");
        PlayButtonSound();

        // 檢查 SmoothSceneTransition 是否存在
        if (FindObjectOfType<SmoothSceneTransition>() != null)
        {
            Debug.Log("✅ 找到 SmoothSceneTransition，調用過場動畫");
            SmoothSceneTransition.LoadScene("TestScene0623");
        }
        else
        {
            Debug.LogError("❌ 找不到 SmoothSceneTransition！使用普通跳轉");
            SceneManager.LoadScene("TestScene0623");
        }
    }

    void StartFloatingAnimations()
    {
        foreach (GameObject obj in floatingObjects)
        {
            if (obj != null)
            {
                // 使用簡單的 Coroutine 代替 LeanTween（避免依賴問題）
                StartCoroutine(FloatAnimation(obj));
            }
        }
    }

    System.Collections.IEnumerator FloatAnimation(GameObject obj)
    {
        Vector3 originalPos = obj.transform.position;
        float time = 0;

        while (true)
        {
            time += Time.deltaTime;
            float yOffset = Mathf.Sin(time) * 0.5f;
            obj.transform.position = originalPos + Vector3.up * yOffset;
            yield return null;
        }
    }

    void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    void OpenSettings()
    {
        Debug.Log("設定功能 - 待實現");
        PlayButtonSound();
    }

    void ShowAbout()
    {
        Debug.Log("關於功能 - 待實現");
        PlayButtonSound();
    }

    void ExitApplication()
    {
        Debug.Log("退出應用程式");
        PlayButtonSound();
        Application.Quit();
    }
}