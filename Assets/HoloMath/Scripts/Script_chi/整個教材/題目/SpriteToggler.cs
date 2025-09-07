using UnityEngine;

public class SpriteToggler : MonoBehaviour
{
    [Header("Sprite 設定")]
    [Tooltip("未選擇狀態的 Sprite (空心圓)")]
    public Sprite unselectedSprite;
    
    [Tooltip("選擇狀態的 Sprite (實心圓)")]
    public Sprite selectedSprite;
    
    [Header("目標組件")]
    [Tooltip("要切換 Sprite 的 SpriteRenderer 組件")]
    public SpriteRenderer targetSpriteRenderer;
    
    [Header("狀態設定")]
    [Tooltip("目前是否為選擇狀態")]
    public bool isSelected = false;
    
    [Tooltip("是否允許取消選擇（再次點擊會回到未選擇狀態）")]
    public bool allowDeselect = true;

    void Start()
    {
        // 如果沒有指定 targetSpriteRenderer，嘗試從子物件中找到
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // 設定初始狀態
        UpdateSprite();
    }

    /// <summary>
    /// 切換 Sprite 狀態（給按鈕的 OnClick 事件使用）
    /// </summary>
    public void ToggleSprite()
    {
        if (isSelected && allowDeselect)
        {
            // 如果已選擇且允許取消選擇，則切換為未選擇
            SetUnselected();
        }
        else if (!isSelected)
        {
            // 如果未選擇，則切換為選擇
            SetSelected();
        }
    }

    /// <summary>
    /// 設定為選擇狀態（實心圓）
    /// </summary>
    public void SetSelected()
    {
        isSelected = true;
        UpdateSprite();
        
        // 觸發選擇事件（可選）
        OnSpriteSelected();
    }

    /// <summary>
    /// 設定為未選擇狀態（空心圓）
    /// </summary>
    public void SetUnselected()
    {
        isSelected = false;
        UpdateSprite();
        
        // 觸發取消選擇事件（可選）
        OnSpriteUnselected();
    }

    /// <summary>
    /// 強制設定狀態
    /// </summary>
    /// <param name="selected">true為選擇狀態，false為未選擇狀態</param>
    public void SetState(bool selected)
    {
        if (selected)
        {
            SetSelected();
        }
        else
        {
            SetUnselected();
        }
    }

    /// <summary>
    /// 更新 Sprite 顯示
    /// </summary>
    private void UpdateSprite()
    {
        if (targetSpriteRenderer == null)
        {
            Debug.LogWarning("SpriteToggler: 未找到目標 SpriteRenderer 組件！");
            return;
        }

        if (isSelected && selectedSprite != null)
        {
            targetSpriteRenderer.sprite = selectedSprite;
            Debug.Log($"切換到選擇狀態: {selectedSprite.name}");
        }
        else if (!isSelected && unselectedSprite != null)
        {
            targetSpriteRenderer.sprite = unselectedSprite;
            Debug.Log($"切換到未選擇狀態: {unselectedSprite.name}");
        }
    }

    /// <summary>
    /// 當 Sprite 被選擇時觸發（可以在這裡添加自定義邏輯）
    /// </summary>
    protected virtual void OnSpriteSelected()
    {
        Debug.Log($"Sprite {gameObject.name} 已選擇");
        
        // 在這裡可以添加音效、動畫等
        // 例如：AudioSource.PlayClipAtPoint(selectSound, transform.position);
    }

    /// <summary>
    /// 當 Sprite 被取消選擇時觸發（可以在這裡添加自定義邏輯）
    /// </summary>
    protected virtual void OnSpriteUnselected()
    {
        Debug.Log($"Sprite {gameObject.name} 已取消選擇");
        
        // 在這裡可以添加音效、動畫等
        // 例如：AudioSource.PlayClipAtPoint(deselectSound, transform.position);
    }

    /// <summary>
    /// 在 Inspector 中驗證設定
    /// </summary>
    void OnValidate()
    {
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
}