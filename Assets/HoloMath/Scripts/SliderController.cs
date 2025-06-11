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

    void Awake()
    {
        GameObject sliderObjectA = GameObject.Find("SliderA");
        sliderA = sliderObjectA.GetComponent<PinchSlider>();
        GameObject sliderObjectB = GameObject.Find("SliderB");
        sliderB = sliderObjectB.GetComponent<PinchSlider>();
        GameObject sliderObjectC = GameObject.Find("SliderC");
        sliderC = sliderObjectC.GetComponent<PinchSlider>();
        sliderA.OnValueUpdated.AddListener(OnSliderAUpdated);
        sliderB.OnValueUpdated.AddListener(OnSliderBUpdated);
        sliderC.OnValueUpdated.AddListener(OnSliderCUpdated);
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
