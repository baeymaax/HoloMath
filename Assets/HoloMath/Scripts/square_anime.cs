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
        // 加入 CanvasGroup 來控制透明度
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
            $"面積 = {baseLength} × {height} ÷ 2 = {area:F2}" :
            $"面積 = {baseLength} × {height} = {area:F2}";

        textDisplay.text = formula;

        // 播放淡入動畫
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
