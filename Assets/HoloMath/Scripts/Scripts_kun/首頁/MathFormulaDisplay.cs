using UnityEngine;
using TMPro;
using System.Collections;

public class MathFormulaDisplay : MonoBehaviour
{
    [Header("Math Formulas")]
    public string[] mathFormulas = {
        "f(x) = x^2",
        "y = sin(x)",
        "Integration f(x)dx",
        "sqrt(a^2 + b^2)",
        "e^(i*pi) + 1 = 0"
    };

    public TextMeshPro formulaText;
    public float changeInterval = 5f;

    void Start()
    {
        if (formulaText == null)
            formulaText = GetComponent<TextMeshPro>();

        StartCoroutine(CycleFormulas());
    }

    System.Collections.IEnumerator CycleFormulas()
    {
        while (true)
        {
            foreach (string formula in mathFormulas)
            {
                // 簡單的淡出淡入效果
                if (formulaText != null)
                {
                    Color originalColor = formulaText.color;

                    // 淡出
                    for (float t = 1f; t >= 0; t -= Time.deltaTime * 2f)
                    {
                        formulaText.color = new Color(originalColor.r, originalColor.g, originalColor.b, t);
                        yield return null;
                    }

                    // 更換文字
                    formulaText.text = formula;

                    // 淡入
                    for (float t = 0; t <= 1f; t += Time.deltaTime * 2f)
                    {
                        formulaText.color = new Color(originalColor.r, originalColor.g, originalColor.b, t);
                        yield return null;
                    }

                    formulaText.color = originalColor;
                }

                yield return new WaitForSeconds(changeInterval);
            }
        }
    }
}