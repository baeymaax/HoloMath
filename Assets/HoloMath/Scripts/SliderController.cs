using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class SliderController : MonoBehaviour
{
    [Header("Parabola Drawer")]
    public ParabolaDrawer parabola;

    [Header("Pinch Sliders")]
    public PinchSlider sliderA;
    public PinchSlider sliderB;
    public PinchSlider sliderC;

    private void OnEnable()
    {
        if (sliderA != null) sliderA.OnValueUpdated.AddListener(OnSliderAUpdated);
        if (sliderB != null) sliderB.OnValueUpdated.AddListener(OnSliderBUpdated);
        if (sliderC != null) sliderC.OnValueUpdated.AddListener(OnSliderCUpdated);
    }

    private void OnDisable()
    {
        if (sliderA != null) sliderA.OnValueUpdated.RemoveListener(OnSliderAUpdated);
        if (sliderB != null) sliderB.OnValueUpdated.RemoveListener(OnSliderBUpdated);
        if (sliderC != null) sliderC.OnValueUpdated.RemoveListener(OnSliderCUpdated);
    }

    private void OnSliderAUpdated(SliderEventData data)
    {
        parabola.a = data.NewValue;
        parabola.DrawParabola();
    }

    private void OnSliderBUpdated(SliderEventData data)
    {
        parabola.b = data.NewValue;
        parabola.DrawParabola();
    }

    private void OnSliderCUpdated(SliderEventData data)
    {
        parabola.c = data.NewValue;
        parabola.DrawParabola();
    }
}
