using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class square_anime : MonoBehaviour
{
    public TextMeshPro textDisplay;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // �[�J CanvasGroup �ӱ���z����
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void UpdateArea(float baseLength, float height, bool isTriangle)
    {
        float area = isTriangle ? (baseLength * height / 2f) : (baseLength * height);
        string formula = isTriangle ?
            $"���n = {baseLength} �� {height} �� 2 = {area:F2}" :
            $"���n = {baseLength} �� {height} = {area:F2}";

        textDisplay.text = formula;

        // ����H�J�ʵe
        StopAllCoroutines();
        StartCoroutine(FadeInText());
    }

    System.Collections.IEnumerator FadeInText()
    {
        canvasGroup.alpha = 0;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
    }
}
