using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class QuizOptionComponent : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro optionTextMesh;
    [SerializeField] private Interactable interactableButton; // 改用 Interactable
    [SerializeField] private GameObject selectionFrame;
    
    private int optionIndex;
    private QuizManager quizManager;
    private bool isSelected = false;

    void Awake()
    {
        // 自動找到組件（如果沒有手動指定）
        if (optionTextMesh == null)
            optionTextMesh = GetComponentInChildren<TextMeshPro>();
        
        if (interactableButton == null)
            interactableButton = GetComponent<Interactable>(); // 獲取 Interactable 組件
        
        if (selectionFrame == null)
            selectionFrame = transform.Find("SelectionFrame")?.gameObject;
    }

    void Start()
    {
        // 設置按鈕事件 - 使用 Interactable 的 OnClick
        if (interactableButton != null)
        {
            interactableButton.OnClick.AddListener(OnButtonPressed);
        }
        else
        {
            Debug.LogError("Interactable component not found on " + gameObject.name);
        }
        
        // 初始時隱藏選擇框
        if (selectionFrame != null)
            selectionFrame.SetActive(false);
    }

    public void Initialize(int index, string optionText, QuizManager manager)
    {
        optionIndex = index;
        quizManager = manager;
        
        SetOptionText(optionText);
    }

    public void SetOptionText(string text)
    {
        if (optionTextMesh != null)
        {
            optionTextMesh.text = text;
        }
        else
        {
            Debug.LogWarning("OptionTextMesh is not assigned in " + gameObject.name);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(selected);
        }
    }
    
    // 新增：獲取當前選中狀態
    public bool IsSelected()
    {
        return isSelected;
    }
    
    // 新增：切換選中狀態
    public void ToggleSelection()
    {
        SetSelected(!isSelected);
    }

    private void OnButtonPressed()
    {
        Debug.Log($"=== BUTTON CLICKED - Option {optionIndex} ===");
        
        // 通知 QuizManager 處理選項切換，讓 QuizManager 來控制狀態
        if (quizManager != null)
        {
            quizManager.OnOptionSelected(optionIndex);
        }
    }

    void OnDestroy()
    {
        // 清理事件監聽
        if (interactableButton != null)
        {
            interactableButton.OnClick.RemoveListener(OnButtonPressed);
        }
    }
}