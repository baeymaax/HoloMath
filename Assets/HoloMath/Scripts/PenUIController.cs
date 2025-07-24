using UnityEngine;
using UnityEngine.UI;

public class PenUIController : MonoBehaviour
{
    [Header("UI 元件")]
    public Slider thicknessSlider;
    public Button[] colorButtons;

    private void Start()
    {
        // 綁定粗細滑桿
        if (thicknessSlider != null)
        {
            thicknessSlider.minValue = 0.001f;
            thicknessSlider.maxValue = 0.05f;
            thicknessSlider.value = 0.01f;

            thicknessSlider.onValueChanged.AddListener(value =>
            {
                VirtualPen.Instance.SetThickness(value);
            });
        }

        // 綁定顏色按鈕（依照順序對應：紅、綠、藍、黑）
        if (colorButtons.Length >= 4)
        {
            colorButtons[0].onClick.AddListener(() => VirtualPen.Instance.SetColor(Color.red));
            colorButtons[1].onClick.AddListener(() => VirtualPen.Instance.SetColor(Color.green));
            colorButtons[2].onClick.AddListener(() => VirtualPen.Instance.SetColor(Color.blue));
            colorButtons[3].onClick.AddListener(() => VirtualPen.Instance.SetColor(Color.black));
        }
    }
}
