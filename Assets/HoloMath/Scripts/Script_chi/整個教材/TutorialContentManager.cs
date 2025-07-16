using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Linq;
using System;

public enum AnswerType
{
    Text,
    Number,
    Expression
}

[Serializable]
public class TutorialQuestion
{
    public string promptText;
    public string correctAnswer;
    public List<string> acceptableAnswers = new List<string>();
    public string hint;
    public AnswerType answerType = AnswerType.Text;
    public float tolerance = 0.01f;
    public bool isCaseSensitive = false;
    public bool allowPartialMatch = false;
    public Vector3 textPosition = Vector3.zero;
    public Vector3 inputFieldPosition = Vector3.zero;
    public Vector3 textRotation = Vector3.zero;
    public Vector3 inputFieldRotation = Vector3.zero;
    public Vector2 textSize = new Vector2(200, 50);
    public Vector2 inputFieldSize = new Vector2(200, 50);
    public bool useCustomPositions = false;
}

[Serializable]
public class TutorialContent
{
    public string contentName;
    public string description;
    public VideoClip videoClip;
    public Texture2D questionImage;
    public bool hasImage = false;
    public GameObject threeDObject;
    public string questionText;
    public List<TutorialQuestion> questions = new List<TutorialQuestion>();
    public bool showHints = true;
    public bool allowRetry = true;
    public bool showProgress = true;
    public int passingScore = 60;
    public bool requireAllCorrect = false;

    public bool HasInteractiveQuestions()
    {
        return questions != null && questions.Count > 0;
    }

    public int GetQuestionCount()
    {
        return questions != null ? questions.Count : 0;
    }

    public TutorialQuestion GetQuestion(int index)
    {
        if (questions != null && index >= 0 && index < questions.Count)
        {
            return questions[index];
        }
        return null;
    }

    public void AddQuestion(TutorialQuestion question)
    {
        if (questions == null)
        {
            questions = new List<TutorialQuestion>();
        }
        questions.Add(question);
    }

    public bool RemoveQuestion(int index)
    {
        if (questions != null && index >= 0 && index < questions.Count)
        {
            questions.RemoveAt(index);
            return true;
        }
        return false;
    }

    public void ClearQuestions()
    {
        if (questions != null)
        {
            questions.Clear();
        }
    }

    public bool IsValid()
    {
        return videoClip != null || threeDObject != null ||
               HasInteractiveQuestions() || !string.IsNullOrEmpty(questionText);
    }

    public string GetSummary()
    {
        string summary = $"內容: {contentName}";
        if (videoClip != null)
        {
            summary += $"\n影片: {videoClip.name}";
        }
        if (threeDObject != null)
        {
            summary += $"\n3D物件: {threeDObject.name}";
        }
        if (HasInteractiveQuestions())
        {
            summary += $"\n互動題目: {GetQuestionCount()} 題";
        }
        else if (!string.IsNullOrEmpty(questionText))
        {
            summary += $"\n文字題目: {questionText.Substring(0, Math.Min(50, questionText.Length))}...";
        }
        return summary;
    }
}

public class TutorialContentManager : MonoBehaviour
{
    [SerializeField] private TutorialContent[] tutorialContents = new TutorialContent[5];
    [SerializeField] private PressableButtonHoloLens2[] controlButtons = new PressableButtonHoloLens2[5];
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshPro questionText3D;
    [SerializeField] private Transform threeDContainer;
    [SerializeField] private GameObject imageDisplayObject;
    [SerializeField] private Renderer imageRenderer;
    [SerializeField] private GameObject questionCubeParent;
    [SerializeField] private Transform questionPanel;
    [SerializeField] private GameObject inputFieldPrefab;
    [SerializeField] private PressableButtonHoloLens2 checkAnswerButton;
    [SerializeField] private PressableButtonHoloLens2 retryButton;
    [SerializeField] private PressableButtonHoloLens2 showHintButton;
    [SerializeField] private TextMeshPro resultText;
    [SerializeField] private TextMeshPro hintText;
    [SerializeField] private int currentContentIndex = 0;
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.cyan;
    [SerializeField] private Color correctAnswerColor = Color.green;
    [SerializeField] private Color wrongAnswerColor = Color.red;

    private GameObject currentThreeDObject;
    private List<TMP_InputField> inputFields = new List<TMP_InputField>();
    private List<TextMeshPro> questionPrompts = new List<TextMeshPro>();
    private bool isAnswerChecked = false;
    private List<bool> questionResults = new List<bool>();

    void Start()
    {
        if (questionText3D == null && questionCubeParent != null)
        {
            questionText3D = questionCubeParent.GetComponentInChildren<TextMeshPro>();
        }
        InitializeThreeDObjects();
        InitializeButtons();
        InitializeInteractiveButtons();
        LoadContent(0);
    }

    private void InitializeThreeDObjects()
    {
        for (int i = 0; i < tutorialContents.Length; i++)
        {
            if (tutorialContents[i].threeDObject != null)
            {
                tutorialContents[i].threeDObject.SetActive(false);
            }
        }
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < controlButtons.Length; i++)
        {
            int index = i;
            if (controlButtons[i] != null)
            {
                controlButtons[i].ButtonPressed.AddListener(() => OnButtonPressed(index));
            }
        }
    }

    private void InitializeInteractiveButtons()
    {
        if (checkAnswerButton != null)
        {
            checkAnswerButton.ButtonPressed.AddListener(CheckAnswers);
        }
        if (retryButton != null)
        {
            retryButton.ButtonPressed.AddListener(RetryQuestions);
        }
        if (showHintButton != null)
        {
            showHintButton.ButtonPressed.AddListener(ShowHints);
        }
    }

    public void OnButtonPressed(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < tutorialContents.Length)
        {
            LoadContent(buttonIndex);
            UpdateButtonVisual(buttonIndex);
        }
    }

    private void LoadContent(int contentIndex)
    {
        if (contentIndex >= 0 && contentIndex < tutorialContents.Length)
        {
            currentContentIndex = contentIndex;
            TutorialContent content = tutorialContents[contentIndex];
            UpdateVideo(content.videoClip);
            UpdateQuestionContent(content);
            Update3DObject(content.threeDObject);
            ResetAnswerState();
            Debug.Log($"已載入內容: {content.contentName}");
        }
    }

    private void UpdateVideo(VideoClip newVideoClip)
    {
        if (videoPlayer != null && newVideoClip != null)
        {
            videoPlayer.clip = newVideoClip;
            videoPlayer.Prepare();
        }
    }

    private void UpdateQuestionContent(TutorialContent content)
    {
        UpdateQuestionImage(content.questionImage, content.hasImage);
        if (content.HasInteractiveQuestions())
        {
            GenerateQuestionFields(content.questions);
            UpdateInteractiveUI(true);
            UpdateInteractiveButtons(content);
        }
        else
        {
            UpdateQuestionText(content.questionText);
            UpdateInteractiveUI(false);
        }
    }

    private void UpdateInteractiveUI(bool showInteractive)
    {
        if (questionPanel != null) questionPanel.gameObject.SetActive(showInteractive);
        if (checkAnswerButton != null) checkAnswerButton.gameObject.SetActive(showInteractive);
        if (retryButton != null) retryButton.gameObject.SetActive(showInteractive);
        if (showHintButton != null) showHintButton.gameObject.SetActive(showInteractive);
        if (resultText != null) resultText.gameObject.SetActive(showInteractive);
        if (hintText != null) hintText.gameObject.SetActive(showInteractive);
    }

    private void UpdateInteractiveButtons(TutorialContent content)
    {
        if (showHintButton != null)
        {
            showHintButton.gameObject.SetActive(content.showHints);
        }
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(content.allowRetry);
        }
    }

    private void GenerateQuestionFields(List<TutorialQuestion> questions)
    {
        if (questionPanel == null || inputFieldPrefab == null) return;
        ClearQuestionFields();
        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var fieldObj = Instantiate(inputFieldPrefab, questionPanel);
            var promptText = fieldObj.GetComponentInChildren<TextMeshPro>();
            if (promptText != null)
            {
                promptText.text = $"{i + 1}. {question.promptText}";
                questionPrompts.Add(promptText);
                if (question.useCustomPositions)
                {
                    promptText.transform.localPosition = question.textPosition;
                    promptText.transform.localRotation = Quaternion.Euler(question.textRotation);
                    RectTransform textRect = promptText.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        textRect.sizeDelta = question.textSize;
                    }
                }
            }
            var inputField = fieldObj.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                inputField.text = "";
                inputFields.Add(inputField);
                if (question.useCustomPositions)
                {
                    inputField.transform.localPosition = question.inputFieldPosition;
                    inputField.transform.localRotation = Quaternion.Euler(question.inputFieldRotation);
                    RectTransform inputRect = inputField.GetComponent<RectTransform>();
                    if (inputRect != null)
                    {
                        inputRect.sizeDelta = question.inputFieldSize;
                    }
                }
            }
        }
        if (resultText != null) resultText.text = "";
        if (hintText != null) hintText.text = "";
    }

    private void ClearQuestionFields()
    {
        if (questionPanel == null)
        {
            Debug.LogWarning("questionPanel 未指派，無法清除題目欄位。請在 Inspector 中指派 questionPanel。");
            return;
        }
        foreach (Transform child in questionPanel)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
        inputFields.Clear();
        questionPrompts.Clear();
    }

    public void CheckAnswers()
    {
        if (currentContentIndex < 0 || currentContentIndex >= tutorialContents.Length) return;
        var content = tutorialContents[currentContentIndex];
        if (!content.HasInteractiveQuestions()) return;
        var questions = content.questions;
        int correctCount = 0;
        questionResults.Clear();
        for (int i = 0; i < questions.Count && i < inputFields.Count; i++)
        {
            string userInput = inputFields[i].text.Trim();
            bool isCorrect = CheckSingleAnswer(userInput, questions[i]);
            questionResults.Add(isCorrect);
            if (isCorrect)
            {
                correctCount++;
            }
            UpdateInputFieldColor(i, isCorrect);
        }
        if (resultText != null)
        {
            string resultMessage = $"你答對了 {correctCount} / {questions.Count} 題！";
            if (correctCount == questions.Count)
            {
                resultMessage += " 全部正確！";
            }
            else if (correctCount > questions.Count / 2)
            {
                resultMessage += " 不錯！";
            }
            else
            {
                resultMessage += " 繼續努力！";
            }
            resultText.text = resultMessage;
            resultText.color = correctCount == questions.Count ? correctAnswerColor : wrongAnswerColor;
        }
        isAnswerChecked = true;
    }

    private bool CheckSingleAnswer(string userInput, TutorialQuestion question)
    {
        if (string.IsNullOrEmpty(userInput)) return false;
        switch (question.answerType)
        {
            case AnswerType.Text:
                return CheckTextAnswer(userInput, question);
            case AnswerType.Number:
                return CheckNumberAnswer(userInput, question);
            case AnswerType.Expression:
                return CheckExpressionAnswer(userInput, question);
            default:
                return CheckTextAnswer(userInput, question);
        }
    }

    private bool CheckTextAnswer(string userInput, TutorialQuestion question)
    {
        string normalizedInput = userInput.ToLower().Trim();
        string normalizedAnswer = question.correctAnswer.ToLower().Trim();
        if (normalizedInput == normalizedAnswer) return true;
        foreach (var acceptableAnswer in question.acceptableAnswers)
        {
            if (normalizedInput == acceptableAnswer.ToLower().Trim()) return true;
        }
        return false;
    }

    private bool CheckNumberAnswer(string userInput, TutorialQuestion question)
    {
        if (float.TryParse(userInput, out float userValue) &&
            float.TryParse(question.correctAnswer, out float correctValue))
        {
            return Mathf.Abs(userValue - correctValue) <= question.tolerance;
        }
        return CheckTextAnswer(userInput, question);
    }

    private bool CheckExpressionAnswer(string userInput, TutorialQuestion question)
    {
        if (CheckTextAnswer(userInput, question)) return true;
        try
        {
            float userValue = EvaluateExpression(userInput);
            float correctValue = EvaluateExpression(question.correctAnswer);
            return Mathf.Abs(userValue - correctValue) <= question.tolerance;
        }
        catch
        {
            return false;
        }
    }

    private float EvaluateExpression(string expression)
    {
        string normalized = expression.ToLower().Trim();
        normalized = normalized.Replace("π", Mathf.PI.ToString());
        normalized = normalized.Replace("pi", Mathf.PI.ToString());
        if (normalized.Contains("/"))
        {
            string[] parts = normalized.Split('/');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], out float numerator) &&
                float.TryParse(parts[1], out float denominator))
            {
                return numerator / denominator;
            }
        }
        if (float.TryParse(normalized, out float result))
        {
            return result;
        }
        throw new System.InvalidOperationException("無法評估表達式");
    }

    private void UpdateInputFieldColor(int index, bool isCorrect)
    {
        if (index < inputFields.Count)
        {
            var image = inputFields[index].GetComponent<Image>();
            if (image != null)
            {
                image.color = isCorrect ? correctAnswerColor : wrongAnswerColor;
            }
        }
    }

    public void RetryQuestions()
    {
        foreach (var inputField in inputFields)
        {
            if (inputField != null)
            {
                inputField.text = "";
                var image = inputField.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.white;
                }
            }
        }
        ResetAnswerState();
    }

    public void ShowHints()
    {
        if (currentContentIndex < 0 || currentContentIndex >= tutorialContents.Length) return;
        var content = tutorialContents[currentContentIndex];
        if (!content.HasInteractiveQuestions() || !content.showHints) return;
        var questions = content.questions;
        string hintsText = "";
        for (int i = 0; i < questions.Count; i++)
        {
            if (!string.IsNullOrEmpty(questions[i].hint))
            {
                hintsText += $"{i + 1}. {questions[i].hint}\n";
            }
        }
        if (hintText != null)
        {
            hintText.text = string.IsNullOrEmpty(hintsText) ? "暫無提示" : hintsText;
        }
    }

    private void ResetAnswerState()
    {
        isAnswerChecked = false;
        questionResults.Clear();
        if (resultText != null)
        {
            resultText.text = "";
        }
        if (hintText != null)
        {
            hintText.text = "";
        }
    }

    private void UpdateQuestionText(string newQuestionText)
    {
        if (questionText3D != null)
        {
            questionText3D.text = newQuestionText;
        }
        else if (questionCubeParent != null)
        {
            TextMeshPro textMesh = questionCubeParent.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = newQuestionText;
                questionText3D = textMesh;
            }
        }
    }

    private void UpdateQuestionImage(Texture2D questionImage, bool hasImage)
    {
        if (imageDisplayObject != null)
        {
            if (hasImage && questionImage != null)
            {
                imageDisplayObject.SetActive(true);
                if (imageRenderer != null)
                {
                    if (imageRenderer == videoPlayer.GetComponent<Renderer>())
                    {
                        videoPlayer.Pause();
                        imageRenderer.material.mainTexture = questionImage;
                    }
                    else
                    {
                        if (imageRenderer.material != null)
                        {
                            imageRenderer.material.mainTexture = questionImage;
                        }
                    }
                }
            }
            else
            {
                imageDisplayObject.SetActive(false);
            }
        }
    }

    private void Update3DObject(GameObject newThreeDObject)
    {
        if (currentThreeDObject != null)
        {
            currentThreeDObject.SetActive(false);
        }
        if (newThreeDObject != null)
        {
            if (newThreeDObject.transform.parent != threeDContainer)
            {
                newThreeDObject.transform.SetParent(threeDContainer);
                newThreeDObject.transform.localPosition = Vector3.zero;
                newThreeDObject.transform.localRotation = Quaternion.identity;
            }
            newThreeDObject.SetActive(true);
            currentThreeDObject = newThreeDObject;
        }
    }

    private void UpdateButtonVisual(int selectedIndex)
    {
        for (int i = 0; i < controlButtons.Length; i++)
        {
            if (controlButtons[i] != null)
            {
                var buttonRenderer = controlButtons[i].GetComponent<Renderer>();
                var buttonImage = controlButtons[i].GetComponent<Image>();
                Color targetColor = (i == selectedIndex) ? selectedButtonColor : normalButtonColor;
                if (buttonRenderer != null)
                {
                    buttonRenderer.material.color = targetColor;
                }
                else if (buttonImage != null)
                {
                    buttonImage.color = targetColor;
                }
            }
        }
    }

    public void PlayCurrentVideo()
    {
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.Play();
        }
    }

    public void PauseCurrentVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }

    public void ResetToFirstContent()
    {
        LoadContent(0);
        UpdateButtonVisual(0);
    }

    public int GetCurrentContentIndex()
    {
        return currentContentIndex;
    }

    public string GetCurrentContentName()
    {
        if (currentContentIndex >= 0 && currentContentIndex < tutorialContents.Length)
        {
            return tutorialContents[currentContentIndex].contentName;
        }
        return "";
    }

    public List<bool> GetCurrentQuestionResults()
    {
        return new List<bool>(questionResults);
    }

    public int GetCorrectAnswerCount()
    {
        return questionResults.Count(result => result);
    }
}