using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIRaycastSolution : MonoBehaviour
{
    [Header("設定")]
    public Camera uiCamera; // 如果是World Space Canvas，指定攝影機
    public float maxDistance = 100f;
    
    [Header("視覺回饋")]
    public LineRenderer laserLine;
    public GameObject crosshair; // 準星物件
    
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    private Button currentHoveredButton;
    
    void Start()
    {
        // 自動找到EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("場景中沒有EventSystem！請添加EventSystem");
            return;
        }
        
        // 找到Canvas的GraphicRaycaster
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                Debug.LogError("Canvas沒有GraphicRaycaster組件！");
            }
        }
        
        // 如果沒有指定攝影機，使用主攝影機
        if (uiCamera == null)
            uiCamera = Camera.main;
            
        // 設置準星
        if (crosshair != null)
        {
            crosshair.SetActive(true);
        }
    }
    
    void Update()
    {
        // 方法1：使用螢幕中央點進行UI射線檢測
        PerformUIRaycast();
        
        // 檢測點擊輸入
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            PerformUIClick();
        }
        
        // 診斷資訊 (按D鍵)
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugUIRaycast();
        }
    }
    
    void PerformUIRaycast()
    {
        if (graphicRaycaster == null || eventSystem == null) return;
        
        // 創建PointerEventData (使用螢幕中央)
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = new Vector2(Screen.width / 2, Screen.height / 2);
        
        // 執行UI射線檢測
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerData, results);
        
        // 更新視覺效果
        UpdateVisualFeedback(results);
        
        // 更新當前指向的按鈕
        UpdateHoveredButton(results);
    }
    
    void UpdateVisualFeedback(List<RaycastResult> results)
    {
        // 更新雷射線
        if (laserLine != null)
        {
            Vector3 startPos = uiCamera.transform.position;
            Vector3 direction = uiCamera.transform.forward;
            
            if (results.Count > 0)
            {
                // 有碰撞，線到碰撞點
                Vector3 worldPos = results[0].worldPosition;
                laserLine.SetPosition(0, startPos);
                laserLine.SetPosition(1, worldPos);
                laserLine.material.color = Color.green; // 綠色表示碰撞
            }
            else
            {
                // 沒碰撞，延伸到最大距離
                laserLine.SetPosition(0, startPos);
                laserLine.SetPosition(1, startPos + direction * maxDistance);
                laserLine.material.color = Color.red; // 紅色表示沒碰撞
            }
        }
    }
    
    void UpdateHoveredButton(List<RaycastResult> results)
    {
        Button newButton = null;
        
        // 找到第一個Button
        foreach (var result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                newButton = button;
                break;
            }
        }
        
        // 如果按鈕改變了
        if (newButton != currentHoveredButton)
        {
            // 離開舊按鈕
            if (currentHoveredButton != null)
            {
                Debug.Log($"離開按鈕: {currentHoveredButton.name}");
                // 可以添加視覺效果移除
            }
            
            // 進入新按鈕
            currentHoveredButton = newButton;
            if (currentHoveredButton != null)
            {
                Debug.Log($"指向按鈕: {currentHoveredButton.name}");
                // 可以添加視覺高亮效果
            }
        }
    }
    
    void PerformUIClick()
    {
        if (currentHoveredButton != null && currentHoveredButton.interactable)
        {
            Debug.Log($"雷射點擊按鈕: {currentHoveredButton.name}");
            
            // 使用完整的UI事件系統，確保不干擾原本的UI功能
            PointerEventData eventData = new PointerEventData(eventSystem);
            eventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
            eventData.button = PointerEventData.InputButton.Left;
            
            // 模擬完整的點擊流程
            ExecuteEvents.Execute(currentHoveredButton.gameObject, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(currentHoveredButton.gameObject, eventData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(currentHoveredButton.gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }
        else
        {
            Debug.Log("沒有指向任何可點擊的按鈕");
        }
    }
    
    void DebugUIRaycast()
    {
        Debug.Log("=== UI射線檢測診斷 ===");
        Debug.Log($"EventSystem: {(eventSystem != null ? "存在" : "缺失")}");
        Debug.Log($"GraphicRaycaster: {(graphicRaycaster != null ? "存在" : "缺失")}");
        Debug.Log($"UI攝影機: {(uiCamera != null ? uiCamera.name : "缺失")}");
        
        if (graphicRaycaster != null && eventSystem != null)
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = new Vector2(Screen.width / 2, Screen.height / 2);
            
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerData, results);
            
            Debug.Log($"檢測到 {results.Count} 個UI物件");
            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log($"  {i}: {results[i].gameObject.name} (距離: {results[i].distance})");
            }
        }
    }
    
    // 在螢幕上顯示準星
    void OnGUI()
    {
        if (crosshair == null) // 如果沒有3D準星物件，用GUI繪製
        {
            float size = 20;
            GUI.color = currentHoveredButton != null ? Color.green : Color.red;
            GUI.Box(new Rect(Screen.width/2 - size/2, Screen.height/2 - size/2, size, size), "+");
            
            // 顯示當前指向的物件名稱
            if (currentHoveredButton != null)
            {
                GUI.Label(new Rect(Screen.width/2 + 30, Screen.height/2 - 10, 200, 20), 
                         $"指向: {currentHoveredButton.name}");
            }
        }
    }
}