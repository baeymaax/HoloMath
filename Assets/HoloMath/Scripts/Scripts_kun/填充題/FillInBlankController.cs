using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FillInBlankController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public TextMeshProUGUI feedbackText;

    [Header("Question Data")]
    public string question = "5 + 3 = ?";
    public string correctAnswer = "8";

    [Header("Animation")]
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float animDuration = 0.3f;

    void Start()
    {
        if (questionText != null)
            questionText.text = question;

        if (answerInput != null)
        {
            answerInput.text = "";
            // 自動聚焦
            answerInput.Select();
            answerInput.ActivateInputField();
        }

        if (submitButton != null)
            submitButton.onClick.AddListener(CheckAnswer);

        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.alpha = 0;
        }
    }

    public void CheckAnswer()
    {
        if (answerInput == null || feedbackText == null) return;

        string userAnswer = answerInput.text.Trim();

        if (userAnswer == correctAnswer)
        {
            ShowFeedback("恭喜答對了！", new Color(0.2f, 0.8f, 0.4f));
            // 震動效果
            StartCoroutine(ShakeCard(true));
        }
        else
        {
            ShowFeedback("錯了 6這都不會", new Color(0.9f, 0.3f, 0.3f));
            StartCoroutine(ShakeCard(false));
        }

        // 清空輸入框但保持焦點
        answerInput.text = "";
        answerInput.Select();
        answerInput.ActivateInputField();
    }

    void ShowFeedback(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        StartCoroutine(FadeInFeedback());
    }

    IEnumerator FadeInFeedback()
    {
        float elapsed = 0;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeInCurve.Evaluate(elapsed / animDuration);
            feedbackText.alpha = t;

            // 輕微縮放效果
            feedbackText.transform.localScale = Vector3.one * (0.8f + 0.2f * t);

            yield return null;
        }

        // 3秒後淡出
        yield return new WaitForSeconds(3f);
        yield return FadeOutFeedback();
    }

    IEnumerator FadeOutFeedback()
    {
        float elapsed = 0;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1 - (elapsed / animDuration);
            feedbackText.alpha = t;

            yield return null;
        }
    }

    IEnumerator ShakeCard(bool isCorrect)
    {
        Transform card = GameObject.Find("QuestionCard").transform;
        Vector3 originalPos = card.localPosition;
        float shakeAmount = isCorrect ? 5f : 10f;
        float duration = 0.2f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-shakeAmount, shakeAmount);
            card.localPosition = originalPos + new Vector3(x, 0, 0);
            yield return null;
        }

        card.localPosition = originalPos;
    }
}