using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Calculator calculator;   // �b Inspector ���V Calculator ����
    public float holdTime = 0.5f;   // �����P�w���

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
            calculator.OnButtonClick("BACK"); // �u���G�˰h
        else
            calculator.OnButtonClick("C");    // �����G�M��

        holding = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        holding = false;  // �ƥX���s������}
    }

    private void Update()
    {
        if (holding)
            timer += Time.unscaledDeltaTime;
    }
}
