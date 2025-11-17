using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

/// <summary>
/// 單元選擇選單 - 使用 HandMenu 和 GridObjectCollection 顯示所有可用的單元
/// </summary>
public class UnitSelectionMenu : MonoBehaviour
{
    [Header("引用設定")]
    [SerializeField] private TutorialContentManager_Test tutorialManager;
    [SerializeField] private GameObject unitButtonPrefab;
    [SerializeField] private GridObjectCollection gridCollection;
    [SerializeField] private Transform buttonContainer; // 按鈕容器，如果沒有則使用 gridCollection

    [Header("中文字體設定")]
    [SerializeField] private TMP_FontAsset chineseFont;
    [SerializeField] private Material chineseFontMaterial;

    [Header("顯示設定")]
    [SerializeField] private bool autoPopulateOnStart = true;
    [SerializeField] private Color normalButtonColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color selectedButtonColor = new Color(0.1f, 0.6f, 0.9f, 1f);
    [SerializeField] private float buttonSpacing = 0.04f;

    [Header("選單標題")]
    [SerializeField] private TextMeshPro menuTitleText;
    [SerializeField] private string menuTitle = "選擇單元";

    private List<GameObject> createdButtons = new List<GameObject>();
    private int currentSelectedUnitIndex = -1;

    void Start()
    {
        if (autoPopulateOnStart)
        {
            StartCoroutine(PopulateMenuAfterDelay(0.5f));
        }

        // 設定標題
        if (menuTitleText != null)
        {
            menuTitleText.text = menuTitle;
            if (chineseFont != null)
            {
                menuTitleText.font = chineseFont;
            }
            if (chineseFontMaterial != null)
            {
                menuTitleText.fontMaterial = chineseFontMaterial;
            }
        }
    }

    private IEnumerator PopulateMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (tutorialManager != null)
        {
            var units = tutorialManager.GetAllUnits();
            PopulateMenu(units);
        }
    }

    /// <summary>
    /// 填充選單按鈕
    /// </summary>
    public void PopulateMenu(List<JsonTutorialUnit> units)
    {
        if (units == null || units.Count == 0)
        {
            Debug.LogWarning("UnitSelectionMenu: 沒有可用的單元資料");
            return;
        }

        // 清空現有按鈕
        ClearAllButtons();

        Transform container = buttonContainer != null ? buttonContainer : (gridCollection != null ? gridCollection.transform : transform);

        // 動態生成按鈕
        for (int i = 0; i < units.Count; i++)
        {
            CreateUnitButton(units[i], i, container);
        }

        // 更新網格佈局
        if (gridCollection != null)
        {
            gridCollection.CellWidth = buttonSpacing;
            gridCollection.CellHeight = buttonSpacing;
            gridCollection.UpdateCollection();
        }

        Debug.Log($"UnitSelectionMenu: 成功建立 {units.Count} 個單元按鈕");
    }

    /// <summary>
    /// 建立單個單元按鈕
    /// </summary>
    private void CreateUnitButton(JsonTutorialUnit unit, int unitIndex, Transform container)
    {
        GameObject buttonObj;

        if (unitButtonPrefab != null)
        {
            // 使用 Prefab
            buttonObj = Instantiate(unitButtonPrefab, container);
            buttonObj.name = $"UnitButton_{unitIndex}_{unit.unitName}";
        }
        else
        {
            // 如果沒有 Prefab，建立基本按鈕
            buttonObj = CreateDefaultButton(unit.unitName, container);
            buttonObj.name = $"UnitButton_{unitIndex}_{unit.unitName}";
        }

        // 設定按鈕文字
        SetupButtonText(buttonObj, unit);

        // 綁定點擊事件
        SetupButtonInteraction(buttonObj, unitIndex);

        createdButtons.Add(buttonObj);
    }

    /// <summary>
    /// 建立預設按鈕（當沒有 Prefab 時）
    /// </summary>
    private GameObject CreateDefaultButton(string buttonName, Transform container)
    {
        GameObject buttonObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buttonObj.transform.SetParent(container);
        buttonObj.transform.localScale = new Vector3(0.15f, 0.08f, 0.02f);
        buttonObj.transform.localPosition = Vector3.zero;
        buttonObj.transform.localRotation = Quaternion.identity;

        // 添加 MRTK Interactable 組件
        Interactable interactable = buttonObj.AddComponent<Interactable>();

        // 添加 Collider (Cube 自帶 BoxCollider)
        BoxCollider collider = buttonObj.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }

        // 設定材質顏色
        Renderer renderer = buttonObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = normalButtonColor;
        }

        // 創建文字子物件
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.015f);
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 3;
        textMesh.enableAutoSizing = true;
        textMesh.fontSizeMin = 1;
        textMesh.fontSizeMax = 5;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(0.14f, 0.07f);
        }

        return buttonObj;
    }

    /// <summary>
    /// 設定按鈕文字
    /// </summary>
    private void SetupButtonText(GameObject buttonObj, JsonTutorialUnit unit)
    {
        TextMeshPro textMesh = buttonObj.GetComponentInChildren<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogWarning($"UnitSelectionMenu: 按鈕 {buttonObj.name} 沒有 TextMeshPro 組件");
            return;
        }

        // 設定文字內容
        string buttonText = $"{unit.unitName}\n({unit.unitDescription})";
        textMesh.text = buttonText;

        // 設定中文字體
        if (chineseFont != null)
        {
            textMesh.font = chineseFont;
        }
        if (chineseFontMaterial != null)
        {
            textMesh.fontMaterial = chineseFontMaterial;
        }
        else if (chineseFont != null && chineseFont.material != null)
        {
            textMesh.fontMaterial = chineseFont.material;
        }

        // 確保文字可見
        textMesh.color = Color.white;
    }

    /// <summary>
    /// 設定按鈕互動
    /// </summary>
    private void SetupButtonInteraction(GameObject buttonObj, int unitIndex)
    {
        Interactable interactable = buttonObj.GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = buttonObj.AddComponent<Interactable>();
        }

        // 移除舊的監聽器
        interactable.OnClick.RemoveAllListeners();

        // 添加新的監聽器
        int capturedIndex = unitIndex;
        interactable.OnClick.AddListener(() => OnUnitButtonClicked(capturedIndex));
    }

    /// <summary>
    /// 單元按鈕被點擊
    /// </summary>
    private void OnUnitButtonClicked(int unitIndex)
    {
        Debug.Log($"UnitSelectionMenu: 選擇單元 {unitIndex}");

        if (tutorialManager == null)
        {
            Debug.LogError("UnitSelectionMenu: TutorialManager 參考遺失");
            return;
        }

        // 更新選擇狀態
        currentSelectedUnitIndex = unitIndex;
        UpdateButtonVisuals();

        // 載入選擇的單元（從該單元的第一個內容開始）
        tutorialManager.LoadUnitFromMenu(unitIndex, 0);

        // 可選：關閉選單
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新按鈕視覺效果
    /// </summary>
    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < createdButtons.Count; i++)
        {
            GameObject buttonObj = createdButtons[i];
            if (buttonObj == null) continue;

            Renderer renderer = buttonObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = (i == currentSelectedUnitIndex) ? selectedButtonColor : normalButtonColor;
            }
        }
    }

    /// <summary>
    /// 清空所有按鈕
    /// </summary>
    private void ClearAllButtons()
    {
        foreach (GameObject button in createdButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        createdButtons.Clear();
    }

    /// <summary>
    /// 切換選單顯示/隱藏
    /// </summary>
    public void ToggleMenu()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    /// <summary>
    /// 顯示選單
    /// </summary>
    public void ShowMenu()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隱藏選單
    /// </summary>
    public void HideMenu()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新選單（重新載入單元資料）
    /// </summary>
    public void RefreshMenu()
    {
        if (tutorialManager != null)
        {
            var units = tutorialManager.GetAllUnits();
            PopulateMenu(units);
        }
    }

    /// <summary>
    /// 設定 TutorialManager 參考
    /// </summary>
    public void SetTutorialManager(TutorialContentManager_Test manager)
    {
        tutorialManager = manager;
    }

    // Gizmos 用於編輯器視覺化
    private void OnDrawGizmos()
    {
        if (gridCollection != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(gridCollection.transform.position, new Vector3(0.5f, 0.5f, 0.05f));
        }
    }
}
