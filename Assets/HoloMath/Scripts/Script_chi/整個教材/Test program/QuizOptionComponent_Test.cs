using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UI;
using System;

public class QuizOptionComponent_Test : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro optionTextMesh;
    [SerializeField] private Interactable interactableButton;
    [SerializeField] private GameObject selectionFrame;
    [SerializeField] private Renderer backgroundRenderer;
    [SerializeField] private Image backgroundImage;

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset chineseFont; // 繁體中文字體
    [SerializeField] private Material chineseFontMaterial; // MSJH_CHT_SDF_4096Material

    private int optionIndex;
    private Action<int> onOptionSelected;
    private bool isSelected = false;
    private Color originalColor;
    private Color normalColor = Color.white;
    private Color selectedColor = Color.cyan;

    void Awake()
    {
        if (optionTextMesh == null)
            optionTextMesh = GetComponentInChildren<TextMeshPro>();
        if (interactableButton == null)
        {
            interactableButton = GetComponent<Interactable>();
            if (interactableButton == null)
                interactableButton = GetComponentInChildren<Interactable>();
            if (interactableButton == null)
                interactableButton = GetComponentInParent<Interactable>();
        }
        if (selectionFrame == null)
            selectionFrame = transform.Find("SelectionFrame")?.gameObject;
        if (backgroundRenderer == null)
            backgroundRenderer = GetComponent<Renderer>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (backgroundRenderer != null)
        {
            originalColor = backgroundRenderer.material.color;
        }
        else if (backgroundImage != null)
        {
            originalColor = backgroundImage.color;
        }
        else
        {
            originalColor = normalColor;
        }

        // 設定繁體中文字體和材質
        SetupChineseFont();
    }

    void Start()
    {
        if (interactableButton != null)
        {
            interactableButton.OnClick.AddListener(OnButtonPressed);
        }
        if (selectionFrame != null)
            selectionFrame.SetActive(false);
        SetBackgroundColor(normalColor);
    }

    private void SetupChineseFont()
    {
        if (optionTextMesh != null)
        {
            // 如果有指定中文字體，就使用它
            if (chineseFont != null)
            {
                optionTextMesh.font = chineseFont;
            }

            // 如果有指定中文字體材質，就設定它
            if (chineseFontMaterial != null)
            {
                optionTextMesh.fontMaterial = chineseFontMaterial;
            }
            else
            {
                // 如果沒有指定材質，嘗試使用字體的預設材質
                if (chineseFont != null && chineseFont.material != null)
                {
                    optionTextMesh.fontMaterial = chineseFont.material;
                }
            }
        }
    }

    public void Initialize(int index, string optionText, Action<int> onSelected)
    {
        optionIndex = index;
        onOptionSelected = onSelected;
        SetOptionText(optionText);
    }

    public void SetOptionText(string text)
    {
        if (optionTextMesh != null)
        {
            optionTextMesh.text = text;
        }
    }

    // 新增方法：動態設定中文字體
    public void SetChineseFont(TMP_FontAsset font, Material fontMaterial = null)
    {
        chineseFont = font;
        chineseFontMaterial = fontMaterial;
        SetupChineseFont();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(selected);
        }
        SetBackgroundColor(selected ? selectedColor : normalColor);
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void ToggleSelection()
    {
        SetSelected(!isSelected);
    }

    public void SetResultColor(Color color)
    {
        SetBackgroundColor(color);
    }

    public void ResetColor()
    {
        SetBackgroundColor(isSelected ? selectedColor : normalColor);
    }

    private void SetBackgroundColor(Color color)
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.material.color = color;
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }

    private void OnButtonPressed()
    {
        ToggleSelection();
        onOptionSelected?.Invoke(optionIndex);
    }

    void OnDestroy()
    {
        if (interactableButton != null)
        {
            interactableButton.OnClick.RemoveListener(OnButtonPressed);
        }
    }

    public void SetColorTheme(Color normal, Color selected)
    {
        normalColor = normal;
        selectedColor = selected;
        if (!isSelected)
        {
            SetBackgroundColor(normalColor);
        }
    }
}