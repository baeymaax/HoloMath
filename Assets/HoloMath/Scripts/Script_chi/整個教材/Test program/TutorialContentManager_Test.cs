using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Linq;
using System;

public enum QuestionType_Test
{
    FillInBlank,
    MultipleChoice
}

public enum AnswerType_Test
{
    Text,
    Number,
    Expression
}

[Serializable]
public class QuizOption_Test
{
    public string optionText;
    public bool isCorrect;
}

[Serializable]
public class TutorialQuestion_Test
{
    [Header("題目類型")]
    public QuestionType_Test questionType = QuestionType_Test.FillInBlank;

    [Header("題目內容")]
    public string promptText;

    [Header("填空題設定")]
    public string correctAnswer;
    public List<string> acceptableAnswers = new List<string>();
    public string hint;
    public AnswerType answerType = AnswerType.Text;
    public float tolerance = 0.01f;
    public bool isCaseSensitive = false;
    public bool allowPartialMatch = false;

    [Header("選擇題設定")]
    public List<QuizOption_Test> options = new List<QuizOption_Test>();

    [Header("位置設定")]
    public Vector3 textPosition = Vector3.zero;
    public Vector3 inputFieldPosition = Vector3.zero;
    public Vector3 textRotation = Vector3.zero;
    public Vector3 inputFieldRotation = Vector3.zero;
    public Vector2 textSize = new Vector2(200, 50);
    public Vector2 inputFieldSize = new Vector2(200, 50);
    public bool useCustomPositions = false;

    [Header("選擇題排版設定")]
    public float optionSpacing = 0.3f;
    public Vector3 optionScale = Vector3.one;
    
    [Header("選擇題位置設定")]
    public bool useCustomQuestionTextPosition = false;
    public Vector3 questionTextPosition = Vector3.zero;
    public bool useCustomOptionPositions = false;
    public Vector3 optionStartPosition = Vector3.zero;
    
    [Header("選擇題文字設定")]
    public float questionTextFontSize = 4f;

    public bool IsMultipleChoice()
    {
        return questionType == QuestionType_Test.MultipleChoice;
    }

    public bool IsFillInBlank()
    {
        return questionType == QuestionType_Test.FillInBlank;
    }

    public HashSet<int> GetCorrectAnswerIndices()
    {
        HashSet<int> correctAnswers = new HashSet<int>();
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].isCorrect)
            {
                correctAnswers.Add(i);
            }
        }
        return correctAnswers;
    }
}

[Serializable]
public class TutorialContent_Test
{
    public string contentName;
    public string description;
    public VideoClip videoClip;
    public Texture2D questionImage;
    public bool hasImage = false;
    public GameObject threeDObject;
    public string questionText;
    public List<TutorialQuestion_Test> questions = new List<TutorialQuestion_Test>();
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
    public TutorialQuestion_Test GetQuestion(int index)
    {
        if (questions != null && index >= 0 && index < questions.Count)
        {
            return questions[index];
        }
        return null;
    }
    public void AddQuestion(TutorialQuestion_Test question)
    {
        if (questions == null)
        {
            questions = new List<TutorialQuestion_Test>();
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
            int fillInBlankCount = questions.Count(q => q.questionType == QuestionType_Test.FillInBlank);
            int multipleChoiceCount = questions.Count(q => q.questionType == QuestionType_Test.MultipleChoice);
            summary += $"\n題目總數: {GetQuestionCount()} 題 (填空題: {fillInBlankCount}, 選擇題: {multipleChoiceCount})";
        }
        else if (!string.IsNullOrEmpty(questionText))
        {
            summary += $"\n文字題目: {questionText.Substring(0, Math.Min(50, questionText.Length))}...";
        }
        return summary;
    }
}

public class TutorialContentManager_Test : MonoBehaviour
{
    [SerializeField] private TutorialContent_Test[] tutorialContents = new TutorialContent_Test[5];
    [SerializeField] private PressableButtonHoloLens2[] controlButtons = new PressableButtonHoloLens2[5];
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshPro questionText3D;
    [SerializeField] private Transform threeDContainer;
    [SerializeField] private GameObject imageDisplayObject;
    [SerializeField] private Renderer imageRenderer;
    [SerializeField] private GameObject questionCubeParent;
    [SerializeField] private Transform questionPanel;
    [SerializeField] private GameObject inputFieldPrefab;
    [SerializeField] private GameObject optionPrefab;
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

    [Header("中文字體設定")]
    [SerializeField] private TMP_FontAsset chineseFont; // 拖拉你的中文字體
    [SerializeField] private Material chineseFontMaterial; // 拖拉 MSJH_CHT_SDF_4096Material

    private GameObject currentThreeDObject;
    private List<TMP_InputField> inputFields = new List<TMP_InputField>();
    private List<TextMeshPro> questionPrompts = new List<TextMeshPro>();
    private List<GameObject> questionContainers = new List<GameObject>();
    private List<HashSet<int>> multipleChoiceSelections = new List<HashSet<int>>();
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
            TutorialContent_Test content = tutorialContents[contentIndex];
            UpdateVideo(content.videoClip);
            UpdateQuestionContent(content);
            Update3DObject(content.threeDObject);
            ResetAnswerState();
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

    private void UpdateQuestionContent(TutorialContent_Test content)
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

    private void UpdateInteractiveButtons(TutorialContent_Test content)
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

    private void GenerateQuestionFields(List<TutorialQuestion_Test> questions)
    {
        if (questionPanel == null) return;
        ClearQuestionFields();
        float currentYOffset = 0f;
        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            GameObject questionContainer = new GameObject($"Question_{i}_Container");
            questionContainer.transform.SetParent(questionPanel);
            questionContainer.transform.localPosition = Vector3.zero;
            questionContainer.transform.localRotation = Quaternion.identity;
            questionContainer.transform.localScale = Vector3.one;
            questionContainers.Add(questionContainer);
            if (question.IsFillInBlank())
            {
                CreateFillInBlankQuestion(question, questionContainer, i, ref currentYOffset);
            }
            else if (question.IsMultipleChoice())
            {
                CreateMultipleChoiceQuestion(question, questionContainer, i, ref currentYOffset);
            }
        }
        if (resultText != null) resultText.text = "";
        if (hintText != null) hintText.text = "";
    }

    private void CreateFillInBlankQuestion(TutorialQuestion_Test question, GameObject container, int questionIndex, ref float yOffset)
    {
        if (inputFieldPrefab == null) return;
        var fieldObj = Instantiate(inputFieldPrefab, container.transform);
        var promptText = fieldObj.GetComponentInChildren<TextMeshPro>();
        if (promptText != null)
        {
            promptText.text = $"{questionIndex + 1}. {question.promptText}";

            if (chineseFont != null)
            {
                promptText.font = chineseFont;
            }
            if (chineseFontMaterial != null)
            {
                promptText.fontMaterial = chineseFontMaterial;
            }
            else if (chineseFont != null && chineseFont.material != null)
            {
                promptText.fontMaterial = chineseFont.material;
            }

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
            else
            {
                promptText.transform.localPosition = new Vector3(0, yOffset, 0);
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
            else
            {
                inputField.transform.localPosition = new Vector3(0, yOffset - 0.15f, 0);
            }
        }
        multipleChoiceSelections.Add(new HashSet<int>());
        yOffset -= 0.5f;
    }

    private void CreateMultipleChoiceQuestion(TutorialQuestion_Test question, GameObject container, int questionIndex, ref float yOffset)
{
    if (optionPrefab == null) return;

    // 創建問題文字
    GameObject questionTextObj = new GameObject($"QuestionText_{questionIndex}");
    questionTextObj.transform.SetParent(container.transform);
    
    // 設定問題文字位置
    Vector3 questionTextPos;
    if (question.useCustomQuestionTextPosition)
    {
        questionTextPos = question.questionTextPosition;
    }
    else
    {
        questionTextPos = new Vector3(0, yOffset, 0);
    }
    questionTextObj.transform.localPosition = questionTextPos;
    questionTextObj.transform.localRotation = Quaternion.identity;
    questionTextObj.transform.localScale = Vector3.one;

    TextMeshPro questionTextMesh = questionTextObj.AddComponent<TextMeshPro>();
    questionTextMesh.text = $"{questionIndex + 1}. {question.promptText}";
    questionTextMesh.fontSize = question.questionTextFontSize;
    questionTextMesh.alignment = TextAlignmentOptions.Left;

    // 設定中文字體和材質
    if (chineseFont != null)
    {
        questionTextMesh.font = chineseFont;
    }
    if (chineseFontMaterial != null)
    {
        questionTextMesh.fontMaterial = chineseFontMaterial;
    }
    else if (chineseFont != null && chineseFont.material != null)
    {
        questionTextMesh.fontMaterial = chineseFont.material;
    }

    questionPrompts.Add(questionTextMesh);

    // 如果沒有使用自訂位置，更新 yOffset
    if (!question.useCustomQuestionTextPosition)
    {
        yOffset -= 0.2f;
    }

    HashSet<int> selections = new HashSet<int>();
    multipleChoiceSelections.Add(selections);

    // 創建選項
    for (int optionIndex = 0; optionIndex < question.options.Count; optionIndex++)
    {
        GameObject optionObj = Instantiate(optionPrefab, container.transform);
        optionObj.name = $"Option_{questionIndex}_{optionIndex}";

        // 設定選項位置
        Vector3 optionPosition;
        if (question.useCustomOptionPositions)
        {
            // 使用自訂起始位置，然後根據 optionSpacing 計算每個選項的位置
            optionPosition = question.optionStartPosition + new Vector3(0, -(optionIndex * question.optionSpacing), 0);
        }
        else
        {
            // 使用預設位置計算
            optionPosition = new Vector3(0, yOffset - (optionIndex * question.optionSpacing), 0);
        }

        optionObj.transform.localPosition = optionPosition;
        optionObj.transform.localScale = question.optionScale;

        QuizOptionComponent_Test optionComponent = optionObj.GetComponent<QuizOptionComponent_Test>();
        if (optionComponent == null)
        {
            optionComponent = optionObj.AddComponent<QuizOptionComponent_Test>();
        }

        // 設定選項組件的中文字體
        if (chineseFont != null || chineseFontMaterial != null)
        {
            optionComponent.SetChineseFont(chineseFont, chineseFontMaterial);
        }

        int capturedQuestionIndex = questionIndex;
        int capturedOptionIndex = optionIndex;

        optionComponent.Initialize(optionIndex, question.options[optionIndex].optionText,
            (selectedOptionIndex) => OnMultipleChoiceOptionSelected(capturedQuestionIndex, selectedOptionIndex));
    }

    inputFields.Add(null);
    
    // 如果沒有使用自訂位置，更新 yOffset 為下一個問題做準備
    if (!question.useCustomOptionPositions)
    {
        yOffset -= (question.options.Count * question.optionSpacing + 0.3f);
    }
}

    private void ClearQuestionFields()
    {
        if (questionPanel == null)
        {
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
        questionContainers.Clear();
        multipleChoiceSelections.Clear();
    }

    public void OnMultipleChoiceOptionSelected(int questionIndex, int optionIndex)
    {
        if (questionIndex < multipleChoiceSelections.Count)
        {
            var selections = multipleChoiceSelections[questionIndex];
            bool wasSelected = selections.Contains(optionIndex);
            if (selections.Contains(optionIndex))
            {
                selections.Remove(optionIndex);
            }
            else
            {
                selections.Add(optionIndex);
            }
            if (questionIndex < questionContainers.Count)
            {
                var container = questionContainers[questionIndex];
                var optionComponents = container.GetComponentsInChildren<QuizOptionComponent>();
                if (optionIndex < optionComponents.Length)
                {
                    optionComponents[optionIndex].SetSelected(!wasSelected);
                }
            }
        }
    }

    public void CheckAnswers()
    {
        if (currentContentIndex < 0 || currentContentIndex >= tutorialContents.Length) return;
        var content = tutorialContents[currentContentIndex];
        if (!content.HasInteractiveQuestions()) return;
        var questions = content.questions;
        int correctCount = 0;
        questionResults.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            bool isCorrect = false;
            if (questions[i].IsFillInBlank())
            {
                if (i < inputFields.Count && inputFields[i] != null)
                {
                    string userInput = inputFields[i].text.Trim();
                    isCorrect = CheckSingleAnswer(userInput, questions[i]);
                    UpdateInputFieldColor(i, isCorrect);
                }
            }
            else if (questions[i].IsMultipleChoice())
            {
                if (i < multipleChoiceSelections.Count)
                {
                    var userSelections = multipleChoiceSelections[i];
                    var correctAnswers = questions[i].GetCorrectAnswerIndices();
                    isCorrect = userSelections.SetEquals(correctAnswers);
                    UpdateMultipleChoiceVisuals(i, isCorrect, correctAnswers);
                }
            }
            questionResults.Add(isCorrect);
            if (isCorrect)
            {
                correctCount++;
            }
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

    private bool CheckSingleAnswer(string userInput, TutorialQuestion_Test question)
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

    private bool CheckTextAnswer(string userInput, TutorialQuestion_Test question)
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

    private bool CheckNumberAnswer(string userInput, TutorialQuestion_Test question)
    {
        if (float.TryParse(userInput, out float userValue) &&
            float.TryParse(question.correctAnswer, out float correctValue))
        {
            return Mathf.Abs(userValue - correctValue) <= question.tolerance;
        }
        return CheckTextAnswer(userInput, question);
    }

    private bool CheckExpressionAnswer(string userInput, TutorialQuestion_Test question)
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
        if (index < inputFields.Count && inputFields[index] != null)
        {
            var image = inputFields[index].GetComponent<Image>();
            if (image != null)
            {
                image.color = isCorrect ? correctAnswerColor : wrongAnswerColor;
            }
        }
    }

    private void UpdateMultipleChoiceVisuals(int questionIndex, bool isCorrect, HashSet<int> correctAnswers)
    {
        if (questionIndex < questionContainers.Count)
        {
            var container = questionContainers[questionIndex];
            var optionComponents = container.GetComponentsInChildren<QuizOptionComponent_Test>();
            for (int i = 0; i < optionComponents.Length; i++)
            {
                var optionComponent = optionComponents[i];
                bool shouldBeSelected = correctAnswers.Contains(i);
                bool wasSelected = multipleChoiceSelections[questionIndex].Contains(i);
                if (shouldBeSelected)
                {
                    optionComponent.SetResultColor(correctAnswerColor);
                }
                else if (wasSelected)
                {
                    optionComponent.SetResultColor(wrongAnswerColor);
                }
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
        for (int i = 0; i < multipleChoiceSelections.Count; i++)
        {
            multipleChoiceSelections[i].Clear();
            if (i < questionContainers.Count)
            {
                var optionComponents = questionContainers[i].GetComponentsInChildren<QuizOptionComponent_Test>();
                foreach (var optionComponent in optionComponents)
                {
                    optionComponent.SetSelected(false);
                    optionComponent.ResetColor();
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
            if (questions[i].IsFillInBlank() && !string.IsNullOrEmpty(questions[i].hint))
            {
                hintsText += $"{i + 1}. {questions[i].hint}\n";
            }
            else if (questions[i].IsMultipleChoice())
            {
                var correctAnswers = questions[i].GetCorrectAnswerIndices();
                if (correctAnswers.Count > 0)
                {
                    hintsText += $"{i + 1}. 正確答案數量: {correctAnswers.Count}\n";
                }
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
