using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// 單元按鈕控制器 - 用於單個單元按鈕的視覺效果和互動
/// </summary>
[RequireComponent(typeof(Interactable))]
public class UnitButtonController : MonoBehaviour
{
    [Header("視覺組件")]
    [SerializeField] private TextMeshPro buttonText;
    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private GameObject selectionFrame; // 選擇框
    [SerializeField] private GameObject hoverEffect; // 懸停效果

    [Header("顏色設定")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.1f, 0.6f, 0.9f, 1f);
    [SerializeField] private Color textColor = Color.white;

    [Header("動畫設定")]
    [SerializeField] private bool enablePressAnimation = true;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationDuration = 0.1f;

    private Interactable interactable;
    private bool isSelected = false;
    private bool isHovered = false;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    // 單元資料
    private int unitIndex = -1;
    private JsonTutorialUnit unitData;

    void Awake()
    {
        // 自動尋找組件
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshPro>();
        }
        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<Renderer>();
        }

        interactable = GetComponent<Interactable>();
        originalScale = transform.localScale;

        // 設定初始狀態
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(false);
        }
        if (hoverEffect != null)
        {
            hoverEffect.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (interactable != null)
        {
            // 註冊 MRTK 事件
            interactable.OnClick.AddListener(OnButtonClicked);
        }
    }

    void OnDisable()
    {
        if (interactable != null)
        {
            interactable.OnClick.RemoveListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// 初始化按鈕
    /// </summary>
    public void Initialize(int index, JsonTutorialUnit unit, TMP_FontAsset font = null, Material fontMaterial = null)
    {
        unitIndex = index;
        unitData = unit;

        // 設定文字
        if (buttonText != null && unit != null)
        {
            buttonText.text = $"{unit.unitName}\n<size=60%>{unit.unitDescription}</size>";
            buttonText.color = textColor;

            // 設定字體
            if (font != null)
            {
                buttonText.font = font;
            }
            if (fontMaterial != null)
            {
                buttonText.fontMaterial = fontMaterial;
            }
            else if (font != null && font.material != null)
            {
                buttonText.fontMaterial = font.material;
            }
        }

        // 設定初始顏色
        UpdateVisualState();
    }

    /// <summary>
    /// 按鈕被點擊
    /// </summary>
    private void OnButtonClicked()
    {
        Debug.Log($"UnitButton: 點擊單元 {unitIndex} - {unitData?.unitName}");

        // 播放按壓動畫
        if (enablePressAnimation)
        {
            PlayPressAnimation();
        }

        // 這裡的實際載入邏輯由 UnitSelectionMenu 處理
    }

    /// <summary>
    /// 設定選擇狀態
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();

        if (selectionFrame != null)
        {
            selectionFrame.SetActive(selected);
        }
    }

    /// <summary>
    /// 設定懸停狀態
    /// </summary>
    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        UpdateVisualState();

        if (hoverEffect != null)
        {
            hoverEffect.SetActive(hovered);
        }
    }

    /// <summary>
    /// 更新視覺狀態
    /// </summary>
    private void UpdateVisualState()
    {
        if (buttonRenderer == null) return;

        Color targetColor = normalColor;

        if (isSelected)
        {
            targetColor = selectedColor;
        }
        else if (isHovered)
        {
            targetColor = hoverColor;
        }

        // 平滑過渡顏色
        if (buttonRenderer.material != null)
        {
            buttonRenderer.material.color = targetColor;
        }
    }

    /// <summary>
    /// 播放按壓動畫
    /// </summary>
    private void PlayPressAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(PressAnimationCoroutine());
    }

    private IEnumerator PressAnimationCoroutine()
    {
        // 縮小
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * pressScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // 恢復
        elapsed = 0f;
        startScale = transform.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        animationCoroutine = null;
    }

    /// <summary>
    /// 設定按鈕文字
    /// </summary>
    public void SetButtonText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    /// <summary>
    /// 設定按鈕顏色
    /// </summary>
    public void SetButtonColor(Color color)
    {
        normalColor = color;
        UpdateVisualState();
    }

    /// <summary>
    /// 獲取單元索引
    /// </summary>
    public int GetUnitIndex()
    {
        return unitIndex;
    }

    /// <summary>
    /// 獲取單元資料
    /// </summary>
    public JsonTutorialUnit GetUnitData()
    {
        return unitData;
    }

    /// <summary>
    /// MRTK 懸停進入事件（需要實作 IMixedRealityPointerHandler）
    /// </summary>
    public void OnPointerEnter()
    {
        SetHovered(true);
    }

    /// <summary>
    /// MRTK 懸停離開事件
    /// </summary>
    public void OnPointerExit()
    {
        SetHovered(false);
    }

    // 編輯器 Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
