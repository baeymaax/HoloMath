using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SmoothSceneTransition : MonoBehaviour
{
    [Header("過場 UI 元素")]
    public CanvasGroup transitionCanvasGroup;
    public Image backgroundOverlay;
    public Image logoImage;
    public TextMeshProUGUI logoText;
    public TextMeshProUGUI loadingText;
    public Slider progressBar;
    public Image progressFill;

    [Header("動畫設定")]
    public float fadeOutDuration = 0.8f;
    public float logoAnimationDuration = 1.2f;
    public float fadeInDuration = 0.6f;
    public AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("視覺效果")]
    public Color overlayColor = new Color(0.1f, 0.1f, 0.2f, 1f);
    public string[] loadingTexts = {
        "正在啟動數學世界...",
        "載入虛擬教室...",
        "準備學習環境...",
        "即將開始學習！"
    };

    [Header("音效")]
    public AudioClip transitionSound;
    public AudioClip loadingSound;

    private AudioSource audioSource;
    private static SmoothSceneTransition instance;

    void Awake()
    {
        // 單例模式，確保場景切換時不被銷毀
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 初始設定 - 隱藏過場界面
        if (transitionCanvasGroup != null)
            transitionCanvasGroup.alpha = 0;
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(SmoothTransitionCoroutine(sceneName));
    }

    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(SmoothTransitionCoroutine(sceneIndex));
    }

    IEnumerator SmoothTransitionCoroutine(object sceneIdentifier)
    {
        // 第一階段：優雅淡出當前場景
        yield return StartCoroutine(FadeOutCurrentScene());

        // 第二階段：顯示過場動畫
        yield return StartCoroutine(ShowTransitionAnimation());

        // 第三階段：異步加載新場景
        yield return StartCoroutine(LoadSceneAsync(sceneIdentifier));

        // 第四階段：淡入新場景
        yield return StartCoroutine(FadeInNewScene());
    }

    IEnumerator FadeOutCurrentScene()
    {
        // 播放過場音效
        if (transitionSound != null && audioSource != null)
            audioSource.PlayOneShot(transitionSound);

        // 顯示過場界面
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.gameObject.SetActive(true);
            transitionCanvasGroup.blocksRaycasts = true;
        }

        // 背景覆蓋層從透明到不透明
        float time = 0;
        Color startColor = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float progress = time / fadeOutDuration;
            float smoothProgress = smoothCurve.Evaluate(progress);

            // 過場界面淡入
            if (transitionCanvasGroup != null)
                transitionCanvasGroup.alpha = smoothProgress;

            // 背景覆蓋
            if (backgroundOverlay != null)
                backgroundOverlay.color = Color.Lerp(startColor, overlayColor, smoothProgress);

            yield return null;
        }

        // 確保完全覆蓋
        if (transitionCanvasGroup != null)
            transitionCanvasGroup.alpha = 1;
        if (backgroundOverlay != null)
            backgroundOverlay.color = overlayColor;
    }

    IEnumerator ShowTransitionAnimation()
    {
        // Logo Image 縮放動畫 (iOS 風格)
        if (logoImage != null)
        {
            logoImage.transform.localScale = Vector3.zero;

            float time = 0;
            while (time < logoAnimationDuration)
            {
                time += Time.deltaTime;
                float progress = time / logoAnimationDuration;

                // iOS 風格的彈性動畫
                float elasticProgress = ElasticEaseOut(progress);
                logoImage.transform.localScale = Vector3.one * elasticProgress;

                // 旋轉效果
                logoImage.transform.rotation = Quaternion.Euler(0, 0, (1 - progress) * 180);

                yield return null;
            }

            logoImage.transform.localScale = Vector3.one;
            logoImage.transform.rotation = Quaternion.identity;
        }

        // Logo Text 縮放動畫 (iOS 風格)
        if (logoText != null)
        {
            logoText.transform.localScale = Vector3.zero;

            float time = 0;
            while (time < logoAnimationDuration)
            {
                time += Time.deltaTime;
                float progress = time / logoAnimationDuration;

                // iOS 風格的彈性動畫
                float elasticProgress = ElasticEaseOut(progress);
                logoText.transform.localScale = Vector3.one * elasticProgress;

                // 旋轉效果
                logoText.transform.rotation = Quaternion.Euler(0, 0, (1 - progress) * 180);

                yield return null;
            }

            logoText.transform.localScale = Vector3.one;
            logoText.transform.rotation = Quaternion.identity;
        }

        // 初始化進度條
        if (progressBar != null)
        {
            progressBar.value = 0;
            progressBar.gameObject.SetActive(true);
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = loadingTexts[0];
        }
    }

    IEnumerator LoadSceneAsync(object sceneIdentifier)
    {
        // 開始異步加載
        AsyncOperation asyncLoad;

        if (sceneIdentifier is string)
            asyncLoad = SceneManager.LoadSceneAsync((string)sceneIdentifier);
        else
            asyncLoad = SceneManager.LoadSceneAsync((int)sceneIdentifier);

        asyncLoad.allowSceneActivation = false;

        // 播放加載音效
        if (loadingSound != null && audioSource != null)
        {
            audioSource.clip = loadingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 模擬平滑的進度更新
        float displayProgress = 0;
        int currentTextIndex = 0;

        while (!asyncLoad.isDone)
        {
            // 實際進度 (Unity 的 0.9 表示加載完成)
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // 平滑追趕實際進度
            displayProgress = Mathf.Lerp(displayProgress, realProgress, Time.deltaTime * 2f);

            // 更新進度條
            if (progressBar != null)
            {
                progressBar.value = displayProgress;

                // 進度條顏色變化
                if (progressFill != null)
                {
                    progressFill.color = Color.Lerp(
                        new Color(0.3f, 0.6f, 1f),  // 藍色
                        new Color(0.3f, 1f, 0.4f),  // 綠色
                        displayProgress
                    );
                }
            }

            // 更新加載文字
            if (loadingText != null)
            {
                int newTextIndex = Mathf.FloorToInt(displayProgress * (loadingTexts.Length - 1));
                if (newTextIndex != currentTextIndex && newTextIndex < loadingTexts.Length)
                {
                    currentTextIndex = newTextIndex;
                    StartCoroutine(TypewriterText(loadingTexts[currentTextIndex]));
                }
            }

            // 當加載完成且進度接近100%時，激活場景
            if (realProgress >= 0.95f && displayProgress >= 0.95f)
            {
                // 最後的文字
                if (loadingText != null)
                    StartCoroutine(TypewriterText(loadingTexts[loadingTexts.Length - 1]));

                yield return new WaitForSeconds(0.5f); // 讓用戶看到100%
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 停止加載音效
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    IEnumerator FadeInNewScene()
    {
        yield return new WaitForSeconds(0.3f); // 短暫停留顯示完成

        float time = 0;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float progress = time / fadeInDuration;
            float smoothProgress = smoothCurve.Evaluate(progress);

            // 過場界面淡出
            if (transitionCanvasGroup != null)
                transitionCanvasGroup.alpha = 1 - smoothProgress;

            yield return null;
        }

        // 完全隱藏過場界面
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0;
            transitionCanvasGroup.gameObject.SetActive(false);
        }
    }

    IEnumerator TypewriterText(string text)
    {
        if (loadingText == null) yield break;

        loadingText.text = "";
        foreach (char c in text)
        {
            loadingText.text += c;
            yield return new WaitForSeconds(0.03f);
        }
    }

    // iOS 風格的彈性緩動函數
    float ElasticEaseOut(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;

        float p = 0.3f;
        float a = 1f;
        float s = p / 4;

        return a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
    }

    public static void LoadScene(string sceneName)
    {
        if (instance != null)
            instance.TransitionToScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int sceneIndex)
    {
        if (instance != null)
            instance.TransitionToScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }
}