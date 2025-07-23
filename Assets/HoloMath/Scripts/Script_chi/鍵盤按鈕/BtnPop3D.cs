using UnityEngine;

public class BtnPop3D : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string popAnimationName = "Pop";
    [SerializeField] private bool useAnimator = true;
    
    [Header("3D Transform Animation (如果不使用Animator)")]
    [SerializeField] private bool useDirectTransform = false;
    [SerializeField] private Vector3 popScale = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio Feedback (可選)")]
    [SerializeField] private AudioClip popSound;
    [SerializeField] private float volume = 1.0f;
    
    private Animator anim;
    private Vector3 originalScale;
    private bool isAnimating = false;
    private AudioSource audioSource;

    private void Awake()
    {
        // 獲取Animator組件
        anim = GetComponent<Animator>();
        
        // 記錄原始縮放
        originalScale = transform.localScale;
        
        // 設置AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && popSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // 驗證設置
        if (useAnimator && anim == null)
        {
            Debug.LogWarning($"{gameObject.name}: 啟用了Animator模式但沒有找到Animator組件，切換到Transform動畫模式");
            useAnimator = false;
            useDirectTransform = true;
        }
    }

    /// <summary>
    /// 播放Pop動畫 - 主要方法
    /// </summary>
    public void PlayPop()
    {
        // 播放音效
        PlayPopSound();
        
        if (useAnimator && anim != null)
        {
            // 使用Animator動畫
            PlayAnimatorPop();
        }
        else if (useDirectTransform)
        {
            // 使用程式化Transform動畫
            PlayTransformPop();
        }
    }
    
    /// <summary>
    /// 使用Animator播放動畫
    /// </summary>
    private void PlayAnimatorPop()
    {
        if (anim != null && !string.IsNullOrEmpty(popAnimationName))
        {
            anim.Play(popAnimationName, 0, 0f);
        }
    }
    
    /// <summary>
    /// 使用程式化Transform動畫
    /// </summary>
    private void PlayTransformPop()
    {
        if (isAnimating) return;
        
        StartCoroutine(TransformPopCoroutine());
    }
    
    /// <summary>
    /// Transform動畫協程
    /// </summary>
    private System.Collections.IEnumerator TransformPopCoroutine()
    {
        isAnimating = true;
        float elapsedTime = 0f;
        
        // 放大階段
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / (animationDuration / 2);
            float curveValue = scaleCurve.Evaluate(progress);
            
            Vector3 currentScale = Vector3.Lerp(originalScale, popScale, curveValue);
            transform.localScale = currentScale;
            
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // 縮小階段
        while (elapsedTime < animationDuration / 2)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / (animationDuration / 2);
            float curveValue = scaleCurve.Evaluate(1 - progress);
            
            Vector3 currentScale = Vector3.Lerp(originalScale, popScale, curveValue);
            transform.localScale = currentScale;
            
            yield return null;
        }
        
        // 確保回到原始大小
        transform.localScale = originalScale;
        isAnimating = false;
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlayPopSound()
    {
        if (audioSource != null && popSound != null)
        {
            audioSource.clip = popSound;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 立即停止動畫並重置
    /// </summary>
    public void StopPopAnimation()
    {
        if (isAnimating)
        {
            StopAllCoroutines();
            transform.localScale = originalScale;
            isAnimating = false;
        }
    }
    
    /// <summary>
    /// 設置新的原始縮放值
    /// </summary>
    public void SetOriginalScale(Vector3 newScale)
    {
        originalScale = newScale;
        if (!isAnimating)
        {
            transform.localScale = originalScale;
        }
    }
    
    // 在Inspector中測試用
    [ContextMenu("Test Pop Animation")]
    private void TestPopAnimation()
    {
        PlayPop();
    }
}