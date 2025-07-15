using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Linq;


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 答案類型枚舉
/// </summary>
public enum AnswerType
{
    Text,        // 文字答案
    Number,      // 數值答案
    Expression   // 表達式答案
}

/// <summary>
/// 教學題目類別
/// </summary>
[Serializable]
public class TutorialQuestion
{
    [Header("題目設定")]
    public string promptText;                    // 題目文字
    public string correctAnswer;                 // 正確答案
    public List<string> acceptableAnswers = new List<string>(); // 可接受的答案
    public string hint;                          // 提示文字

    [Header("答案類型")]
    public AnswerType answerType = AnswerType.Text; // 答案類型
    public float tolerance = 0.01f;              // 數值容差（用於數值答案）

    [Header("額外設定")]
    public bool isCaseSensitive = false;         // 是否區分大小寫
    public bool allowPartialMatch = false;       // 是否允許部分匹配
}

/// <summary>
/// 教學內容類別
/// </summary>
[Serializable]
public class TutorialContent
{
    [Header("基本資訊")]
    public string contentName;                   // 內容名稱
    public string description;                   // 內容描述

    [Header("媒體內容")]
    public VideoClip videoClip;                  // 影片片段
    public Texture2D questionImage;              // 例題圖片
    public bool hasImage = false;                // 是否有圖片

    [Header("3D物件")]
    public GameObject threeDObject;              // 3D物件

    [Header("舊版文字題目")]
    public string questionText;                  // 例題文字（舊版）

    [Header("新版互動式題目")]
    public List<TutorialQuestion> questions = new List<TutorialQuestion>(); // 題目列表

    [Header("互動設定")]
    public bool showHints = true;                // 是否顯示提示
    public bool allowRetry = true;               // 是否允許重試
    public bool showProgress = true;             // 是否顯示進度

    [Header("評分設定")]
    public int passingScore = 60;                // 及格分數（百分比）
    public bool requireAllCorrect = false;       // 是否需要全部正確

    /// <summary>
    /// 檢查是否有互動式題目
    /// </summary>
    /// <returns>是否有互動式題目</returns>
    public bool HasInteractiveQuestions()
    {
        return questions != null && questions.Count > 0;
    }

    /// <summary>
    /// 獲取題目數量
    /// </summary>
    /// <returns>題目數量</returns>
    public int GetQuestionCount()
    {
        return questions != null ? questions.Count : 0;
    }

    /// <summary>
    /// 獲取指定索引的題目
    /// </summary>
    /// <param name="index">題目索引</param>
    /// <returns>題目物件，如果索引無效則返回null</returns>
    public TutorialQuestion GetQuestion(int index)
    {
        if (questions != null && index >= 0 && index < questions.Count)
        {
            return questions[index];
        }
        return null;
    }

    /// <summary>
    /// 添加題目
    /// </summary>
    /// <param name="question">要添加的題目</param>
    public void AddQuestion(TutorialQuestion question)
    {
        if (questions == null)
        {
            questions = new List<TutorialQuestion>();
        }
        questions.Add(question);
    }

    /// <summary>
    /// 移除指定索引的題目
    /// </summary>
    /// <param name="index">題目索引</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveQuestion(int index)
    {
        if (questions != null && index >= 0 && index < questions.Count)
        {
            questions.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 清空所有題目
    /// </summary>
    public void ClearQuestions()
    {
        if (questions != null)
        {
            questions.Clear();
        }
    }

    /// <summary>
    /// 檢查內容是否有效
    /// </summary>
    /// <returns>內容是否有效</returns>
    public bool IsValid()
    {
        // 至少要有影片或3D物件或題目
        return videoClip != null || threeDObject != null ||
               HasInteractiveQuestions() || !string.IsNullOrEmpty(questionText);
    }

    /// <summary>
    /// 獲取內容摘要
    /// </summary>
    /// <returns>內容摘要字串</returns>
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
    [Header("內容數據")]
    [SerializeField] private TutorialContent[] tutorialContents = new TutorialContent[5];

    [Header("控制按鈕")]
    [SerializeField] private PressableButtonHoloLens2[] controlButtons = new PressableButtonHoloLens2[5];

    [Header("目標物件")]
    [SerializeField] private VideoPlayer videoPlayer; // 影片播放器
    [SerializeField] private TextMeshPro questionText3D; // 3D例題文字 (TextMesh Pro 3D)
    [SerializeField] private Transform threeDContainer; // 3D物件容器

    [Header("例題圖片顯示")]
    [SerializeField] private GameObject imageDisplayObject; // 顯示圖片的物件（如Quad或Plane）
    [SerializeField] private Renderer imageRenderer; // 圖片渲染器

    [Header("或者使用父物件自動搜尋")]
    [SerializeField] private GameObject questionCubeParent; // 例題Cube父物件（如果不直接指定TextMeshPro）

    [Header("互動式題目介面")]
    [SerializeField] private Transform questionPanel; // 題目面板容器
    [SerializeField] private GameObject inputFieldPrefab; // 輸入欄位預製件
    [SerializeField] private PressableButtonHoloLens2 checkAnswerButton; // 檢查答案按鈕
    [SerializeField] private PressableButtonHoloLens2 retryButton; // 重試按鈕
    [SerializeField] private PressableButtonHoloLens2 showHintButton; // 顯示提示按鈕
    [SerializeField] private TextMeshPro resultText; // 結果顯示文字
    [SerializeField] private TextMeshPro hintText; // 提示文字

    [Header("設定")]
    [SerializeField] private int currentContentIndex = 0; // 當前內容索引
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.cyan;
    [SerializeField] private Color correctAnswerColor = Color.green;
    [SerializeField] private Color wrongAnswerColor = Color.red;

    // 私有變數
    private GameObject currentThreeDObject; // 當前顯示的3D物件
    private List<TMP_InputField> inputFields = new List<TMP_InputField>(); // 輸入欄位列表
    private List<TextMeshPro> questionPrompts = new List<TextMeshPro>(); // 題目提示文字列表
    private bool isAnswerChecked = false; // 是否已檢查答案
    private List<bool> questionResults = new List<bool>(); // 每題的答題結果

    void Start()
    {
        // 如果沒有直接指定TextMeshPro，嘗試從父物件中找到
        if (questionText3D == null && questionCubeParent != null)
        {
            questionText3D = questionCubeParent.GetComponentInChildren<TextMeshPro>();
        }

        // 初始化所有3D物件狀態（先全部隱藏）
        InitializeThreeDObjects();

        InitializeButtons();
        InitializeInteractiveButtons();
        LoadContent(0); // 載入第一個內容
    }

    /// <summary>
    /// 初始化所有3D物件狀態（在開始時隱藏所有物件）
    /// </summary>
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

    /// <summary>
    /// 初始化按鈕事件
    /// </summary>
    private void InitializeButtons()
    {
        for (int i = 0; i < controlButtons.Length; i++)
        {
            int index = i; // 避免閉包問題
            if (controlButtons[i] != null)
            {
                controlButtons[i].ButtonPressed.AddListener(() => OnButtonPressed(index));
            }
        }
    }

    /// <summary>
    /// 初始化互動式按鈕事件
    /// </summary>
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

    /// <summary>
    /// 按鈕點擊事件
    /// </summary>
    /// <param name="buttonIndex">按鈕索引</param>
    public void OnButtonPressed(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < tutorialContents.Length)
        {
            LoadContent(buttonIndex);
            UpdateButtonVisual(buttonIndex);
        }
    }

    /// <summary>
    /// 載入指定索引的內容
    /// </summary>
    /// <param name="contentIndex">內容索引</param>
    private void LoadContent(int contentIndex)
    {
        if (contentIndex >= 0 && contentIndex < tutorialContents.Length)
        {
            currentContentIndex = contentIndex;
            TutorialContent content = tutorialContents[contentIndex];

            // 更新影片
            UpdateVideo(content.videoClip);

            // 更新例題內容
            UpdateQuestionContent(content);

            // 更新3D物件
            Update3DObject(content.threeDObject);

            // 重置答題狀態
            ResetAnswerState();

            Debug.Log($"已載入內容: {content.contentName}");
        }
    }

    /// <summary>
    /// 更新影片內容
    /// </summary>
    /// <param name="newVideoClip">新的影片片段</param>
    private void UpdateVideo(VideoClip newVideoClip)
    {
        if (videoPlayer != null && newVideoClip != null)
        {
            videoPlayer.clip = newVideoClip;
            videoPlayer.Prepare(); // 準備播放
        }
    }

    /// <summary>
    /// 更新例題內容（支援新舊版本）
    /// </summary>
    /// <param name="content">教學內容</param>
    private void UpdateQuestionContent(TutorialContent content)
    {
        // 更新圖片
        UpdateQuestionImage(content.questionImage, content.hasImage);

        // 根據內容類型更新題目
        if (content.HasInteractiveQuestions())
        {
            // 使用新版互動式題目
            GenerateQuestionFields(content.questions);
            UpdateInteractiveUI(true);

            // 更新按鈕狀態
            UpdateInteractiveButtons(content);
        }
        else
        {
            // 使用舊版純文字題目
            UpdateQuestionText(content.questionText);
            UpdateInteractiveUI(false);
        }
    }

    /// <summary>
    /// 更新互動式介面的顯示狀態
    /// </summary>
    /// <param name="showInteractive">是否顯示互動式介面</param>
    private void UpdateInteractiveUI(bool showInteractive)
    {
        if (questionPanel != null)
        {
            questionPanel.gameObject.SetActive(showInteractive);
        }

        if (checkAnswerButton != null)
        {
            checkAnswerButton.gameObject.SetActive(showInteractive);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(showInteractive);
        }

        if (showHintButton != null)
        {
            showHintButton.gameObject.SetActive(showInteractive);
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(showInteractive);
        }

        if (hintText != null)
        {
            hintText.gameObject.SetActive(showInteractive);
        }
    }

    /// <summary>
    /// 更新互動式按鈕狀態
    /// </summary>
    /// <param name="content">教學內容</param>
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

    /// <summary>
    /// 動態生成題目輸入欄位
    /// </summary>
    /// <param name="questions">題目列表</param>
    // 在你的 TutorialContentManager 類別中，確保只有一個 GenerateQuestionFields 方法

    /// <summary>
    /// 動態生成題目輸入欄位 - 唯一版本
    /// </summary>
    /// <param name="questions">題目列表</param>
    private void GenerateQuestionFields(List<TutorialQuestion> questions)
    {
        // 加入 null 檢查
        if (questionPanel == null)
        {
            Debug.LogWarning("questionPanel 未指派，無法生成題目欄位。請在 Inspector 中指派 questionPanel。");
            return;
        }

        if (inputFieldPrefab == null)
        {
            Debug.LogWarning("inputFieldPrefab 未指派，無法生成題目欄位。請在 Inspector 中指派 inputFieldPrefab。");
            return;
        }

        // 清除舊題目
        ClearQuestionFields();

        // 建立新題目
        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var fieldObj = Instantiate(inputFieldPrefab, questionPanel);

            // 設定題目文字
            var promptText = fieldObj.GetComponentInChildren<TextMeshPro>();
            if (promptText != null)
            {
                promptText.text = $"{i + 1}. {question.promptText}";
                questionPrompts.Add(promptText);
            }

            // 設定輸入欄位
            var inputField = fieldObj.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                inputField.text = ""; // 清空輸入
                inputFields.Add(inputField);
            }
        }

        // 清空結果和提示
        if (resultText != null)
        {
            resultText.text = "";
        }

        if (hintText != null)
        {
            hintText.text = "";
        }
    }

    /// <summary>
    /// 清除題目欄位 - 唯一版本
    /// </summary>
    private void ClearQuestionFields()
    {
        // 加入 null 檢查
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

    /// <summary>
    /// 檢查答案
    /// </summary>
    public void CheckAnswers()
    {
        if (currentContentIndex < 0 || currentContentIndex >= tutorialContents.Length)
            return;

        var content = tutorialContents[currentContentIndex];
        if (!content.HasInteractiveQuestions())
            return;

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

            // 更新輸入欄位顏色
            UpdateInputFieldColor(i, isCorrect);
        }

        // 顯示結果
        if (resultText != null)
        {
            string resultMessage = $"你答對了 {correctCount} / {questions.Count} 題！";
            if (correctCount == questions.Count)
            {
                resultMessage += " 🎉 全部正確！";
            }
            else if (correctCount > questions.Count / 2)
            {
                resultMessage += " 👍 不錯！";
            }
            else
            {
                resultMessage += " 💪 繼續努力！";
            }

            resultText.text = resultMessage;
            resultText.color = correctCount == questions.Count ? correctAnswerColor : wrongAnswerColor;
        }

        isAnswerChecked = true;
    }

    /// <summary>
    /// 檢查單一答案
    /// </summary>
    /// <param name="userInput">使用者輸入</param>
    /// <param name="question">題目</param>
    /// <returns>是否正確</returns>
    private bool CheckSingleAnswer(string userInput, TutorialQuestion question)
    {
        if (string.IsNullOrEmpty(userInput))
            return false;

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

    /// <summary>
    /// 檢查文字答案
    /// </summary>
    private bool CheckTextAnswer(string userInput, TutorialQuestion question)
    {
        string normalizedInput = userInput.ToLower().Trim();
        string normalizedAnswer = question.correctAnswer.ToLower().Trim();

        // 檢查主要答案
        if (normalizedInput == normalizedAnswer)
            return true;

        // 檢查可接受的答案
        foreach (var acceptableAnswer in question.acceptableAnswers)
        {
            if (normalizedInput == acceptableAnswer.ToLower().Trim())
                return true;
        }

        return false;
    }

    /// <summary>
    /// 檢查數值答案
    /// </summary>
    private bool CheckNumberAnswer(string userInput, TutorialQuestion question)
    {
        if (float.TryParse(userInput, out float userValue) &&
            float.TryParse(question.correctAnswer, out float correctValue))
        {
            return Mathf.Abs(userValue - correctValue) <= question.tolerance;
        }

        return CheckTextAnswer(userInput, question); // 如果無法解析為數值，回到文字比對
    }

    /// <summary>
    /// 檢查表達式答案（支援多種格式）
    /// </summary>
    private bool CheckExpressionAnswer(string userInput, TutorialQuestion question)
    {
        // 首先嘗試文字比對
        if (CheckTextAnswer(userInput, question))
            return true;

        // 嘗試數值評估（如果可能）
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

    /// <summary>
    /// 簡單的表達式評估（支援基本數學運算和π）
    /// </summary>
    private float EvaluateExpression(string expression)
    {
        // 簡化版本，支援基本運算
        string normalized = expression.ToLower().Trim();

        // 替換π符號
        normalized = normalized.Replace("π", Mathf.PI.ToString());
        normalized = normalized.Replace("pi", Mathf.PI.ToString());

        // 這裡可以擴展更複雜的表達式解析
        // 目前只支援簡單的除法運算
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

        // 嘗試直接解析為數值
        if (float.TryParse(normalized, out float result))
        {
            return result;
        }

        throw new System.InvalidOperationException("無法評估表達式");
    }

    /// <summary>
    /// 更新輸入欄位顏色
    /// </summary>
    private void UpdateInputFieldColor(int index, bool isCorrect)
    {
        if (index < inputFields.Count)
        {
            var inputField = inputFields[index];
            var image = inputField.GetComponent<Image>();

            if (image != null)
            {
                image.color = isCorrect ? correctAnswerColor : wrongAnswerColor;
            }
        }
    }

    /// <summary>
    /// 重試題目
    /// </summary>
    public void RetryQuestions()
    {
        // 清空所有輸入
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

        // 重置狀態
        ResetAnswerState();
    }

    /// <summary>
    /// 顯示提示
    /// </summary>
    public void ShowHints()
    {
        if (currentContentIndex < 0 || currentContentIndex >= tutorialContents.Length)
            return;

        var content = tutorialContents[currentContentIndex];
        if (!content.HasInteractiveQuestions() || !content.showHints)
            return;

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

    /// <summary>
    /// 重置答題狀態
    /// </summary>
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

    // 以下為原有方法，保持不變

    /// <summary>
    /// 更新例題文字 (3D TextMesh Pro)
    /// </summary>
    /// <param name="newQuestionText">新的例題文字</param>
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

    /// <summary>
    /// 更新例題圖片
    /// </summary>
    /// <param name="questionImage">例題圖片</param>
    /// <param name="hasImage">是否有圖片</param>
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

    /// <summary>
    /// 更新3D物件
    /// </summary>
    /// <param name="newThreeDObject">新的3D物件</param>
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

    /// <summary>
    /// 更新按鈕視覺效果
    /// </summary>
    /// <param name="selectedIndex">選中的按鈕索引</param>
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

    /// <summary>
    /// 播放當前影片
    /// </summary>
    public void PlayCurrentVideo()
    {
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// 暫停當前影片
    /// </summary>
    public void PauseCurrentVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }

    /// <summary>
    /// 重置到第一個內容
    /// </summary>
    public void ResetToFirstContent()
    {
        LoadContent(0);
        UpdateButtonVisual(0);
    }

    /// <summary>
    /// 獲取當前內容索引
    /// </summary>
    /// <returns>當前內容索引</returns>
    public int GetCurrentContentIndex()
    {
        return currentContentIndex;
    }

    /// <summary>
    /// 獲取當前內容名稱
    /// </summary>
    /// <returns>當前內容名稱</returns>
    public string GetCurrentContentName()
    {
        if (currentContentIndex >= 0 && currentContentIndex < tutorialContents.Length)
        {
            return tutorialContents[currentContentIndex].contentName;
        }
        return "";
    }

    /// <summary>
    /// 獲取當前答題結果
    /// </summary>
    /// <returns>答題結果列表</returns>
    public List<bool> GetCurrentQuestionResults()
    {
        return new List<bool>(questionResults);
    }

    /// <summary>
    /// 獲取當前正確答案數量
    /// </summary>
    /// <returns>正確答案數量</returns>
    public int GetCorrectAnswerCount()
    {
        return questionResults.Count(result => result);
    }
}