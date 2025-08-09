using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MenuAnimations : MonoBehaviour
{
    [Header("動畫元素")]
    public CanvasGroup titlePanel;
    public CanvasGroup buttonPanel;
    public TextMeshProUGUI welcomeText;
    public TextMeshProUGUI[] formulaTexts;

    void Start()
    {
        StartCoroutine(PlayIntroAnimation());
    }

    IEnumerator PlayIntroAnimation()
    {
        // 初始設置 - 所有元素透明
        SetAlpha(titlePanel, 0);
        SetAlpha(buttonPanel, 0);
        if (welcomeText != null) welcomeText.color = Color.clear;

        foreach (var formula in formulaTexts)
        {
            if (formula != null) formula.color = Color.clear;
        }

        // 動畫序列
        yield return new WaitForSeconds(0.5f);

        // 1. 標題淡入
        yield return StartCoroutine(FadeIn(titlePanel, 1.5f));

        // 2. 歡迎文字打字機效果
        if (welcomeText != null)
        {
            yield return StartCoroutine(TypewriterEffect(welcomeText, "歡迎回來，學習者！"));
        }

        // 3. 按鈕面板淡入
        yield return StartCoroutine(FadeIn(buttonPanel, 1f));

        // 4. 數學公式隨機出現
        StartCoroutine(AnimateFormulas());
    }

    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null) yield break;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI text, string fullText)
    {
        text.text = "";
        text.color = Color.white;

        foreach (char c in fullText)
        {
            text.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator AnimateFormulas()
    {
        foreach (var formula in formulaTexts)
        {
            if (formula != null)
            {
                StartCoroutine(FormulaFadeIn(formula));
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    IEnumerator FormulaFadeIn(TextMeshProUGUI formula)
    {
        Color targetColor = new Color(0.45f, 0.73f, 1f, 0.6f); // 淺藍色半透明
        float time = 0;
        float duration = 1f;

        while (time < duration)
        {
            time += Time.deltaTime;
            formula.color = Color.Lerp(Color.clear, targetColor, time / duration);
            yield return null;
        }

        // 開始浮動動畫
        StartCoroutine(FloatFormula(formula));
    }

    IEnumerator FloatFormula(TextMeshProUGUI formula)
    {
        Vector3 originalPos = formula.transform.position;
        float time = 0;

        while (true)
        {
            time += Time.deltaTime;
            float yOffset = Mathf.Sin(time * 0.5f) * 20f; // 緩慢浮動
            formula.transform.position = originalPos + Vector3.up * yOffset;
            yield return null;
        }
    }

    void SetAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }
}