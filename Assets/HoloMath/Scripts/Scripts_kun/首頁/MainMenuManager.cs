using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 按鈕")]
    public Button startLearningButton;
    public Button settingsButton;
    public Button aboutButton;
    public Button exitButton;

    [Header("文字元素")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI welcomeText;

    [Header("場景設定")]
    public string classroomSceneName = "TestScene0623";

    [Header("音效")]
    public AudioClip buttonClickSound;
    public AudioClip backgroundMusic;

    [Header("動畫設定")]
    public float titleFadeInDuration = 1.5f;
    public float buttonStaggerDelay = 0.2f;

    private AudioSource audioSource;

    void Start()
    {
        InitializeMenu();
        SetupButtonEvents();
        StartMenuAnimations(); // 只有標題和按鈕動畫
    }

    void InitializeMenu()
    {
        // 添加 AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 設置歡迎文字
        if (welcomeText != null)
        {
            string userName = PlayerPrefs.GetString("UserName", "學習者");
            welcomeText.text = $"歡迎, {userName}!";
        }

        // 播放背景音樂
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.3f;
            audioSource.Play();
        }
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

    void StartMenuAnimations()
    {
        // 啟動標題動畫
        StartCoroutine(AnimateTitle());

        // 啟動按鈕動畫
        StartCoroutine(AnimateButtons());

        // 歡迎文字動畫
        if (welcomeText != null)
            StartCoroutine(AnimateWelcomeText());
    }

    IEnumerator AnimateTitle()
    {
        if (titleText == null) yield break;

        // 標題淡入動畫
        CanvasGroup titleCanvasGroup = titleText.GetComponent<CanvasGroup>();
        if (titleCanvasGroup == null)
            titleCanvasGroup = titleText.gameObject.AddComponent<CanvasGroup>();

        titleCanvasGroup.alpha = 0;

        float time = 0;
        while (time < titleFadeInDuration)
        {
            time += Time.deltaTime;
            titleCanvasGroup.alpha = Mathf.Lerp(0, 1, time / titleFadeInDuration);

            // 輕微的縮放效果
            float scale = Mathf.Lerp(0.8f, 1f, time / titleFadeInDuration);
            titleText.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        titleCanvasGroup.alpha = 1;
        titleText.transform.localScale = Vector3.one;
    }

    IEnumerator AnimateButtons()
    {
        Button[] buttons = { startLearningButton, settingsButton, aboutButton, exitButton };

        // 初始設置所有按鈕為不可見
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                CanvasGroup buttonCanvasGroup = button.GetComponent<CanvasGroup>();
                if (buttonCanvasGroup == null)
                    buttonCanvasGroup = button.gameObject.AddComponent<CanvasGroup>();

                buttonCanvasGroup.alpha = 0;
                button.transform.localScale = Vector3.one * 0.8f;
            }
        }

        // 等待標題動畫完成一半
        yield return new WaitForSeconds(titleFadeInDuration * 0.5f);

        // 依序顯示按鈕
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                StartCoroutine(AnimateSingleButton(buttons[i]));
                yield return new WaitForSeconds(buttonStaggerDelay);
            }
        }
    }

    IEnumerator AnimateSingleButton(Button button)
    {
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        float animationDuration = 0.5f;
        float time = 0;

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float progress = time / animationDuration;

            // 彈性動畫
            float elasticProgress = ElasticEaseOut(progress);

            canvasGroup.alpha = progress;
            button.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, elasticProgress);

            yield return null;
        }

        canvasGroup.alpha = 1;
        button.transform.localScale = Vector3.one;
    }

    IEnumerator AnimateWelcomeText()
    {
        if (welcomeText == null) yield break;

        // 等待一段時間再顯示歡迎文字
        yield return new WaitForSeconds(1f);

        string fullText = welcomeText.text;
        welcomeText.text = "";

        // 打字機效果
        foreach (char c in fullText)
        {
            welcomeText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // 彈性緩動函數
    float ElasticEaseOut(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;

        float p = 0.3f;
        float a = 1f;
        float s = p / 4;

        return a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
    }

    public void StartLearning()
    {
        PlayButtonSound();

        // 按鈕點擊動畫
        if (startLearningButton != null)
            StartCoroutine(ButtonClickAnimation(startLearningButton));

        // 使用過場動畫跳轉
        SmoothSceneTransition.LoadScene(classroomSceneName);
    }

    IEnumerator ButtonClickAnimation(Button button)
    {
        Vector3 originalScale = button.transform.localScale;

        // 按下效果
        float time = 0;
        while (time < 0.1f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0.95f, time / 0.1f);
            button.transform.localScale = originalScale * scale;
            yield return null;
        }

        // 回彈效果
        time = 0;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(0.95f, 1f, ElasticEaseOut(time / 0.2f));
            button.transform.localScale = originalScale * scale;
            yield return null;
        }

        button.transform.localScale = originalScale;
    }

    void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

    void OpenSettings()
    {
        PlayButtonSound();
        if (settingsButton != null)
            StartCoroutine(ButtonClickAnimation(settingsButton));
    }

    void ShowAbout()
    {
        PlayButtonSound();
        if (aboutButton != null)
            StartCoroutine(ButtonClickAnimation(aboutButton));
    }

    void ExitApplication()
    {
        PlayButtonSound();
        if (exitButton != null)
            StartCoroutine(ButtonClickAnimation(exitButton));
        Application.Quit();
    }
}