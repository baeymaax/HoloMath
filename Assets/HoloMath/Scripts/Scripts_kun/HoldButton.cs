using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Calculator calculator;   // 在 Inspector 指向 Calculator 物件
    public float holdTime = 0.5f;   // 長按判定秒數

    private float timer = 0f;
    private bool holding = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;
        timer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!holding) return;

        if (timer < holdTime)
            calculator.OnButtonClick("BACK"); // 短按：倒退
        else
            calculator.OnButtonClick("C");    // 長按：清空

        holding = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        holding = false;  // 滑出按鈕視為放開
    }

    private void Update()
    {
        if (holding)
            timer += Time.unscaledDeltaTime;
    }
}
