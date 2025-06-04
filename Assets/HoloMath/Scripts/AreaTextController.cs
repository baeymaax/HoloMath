using UnityEngine;
using TMPro;

public class AreaTextController : MonoBehaviour
{
    public TextMeshPro textDisplay;
    private CanvasGroup canvasGroup;

    void Awake()
    {
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