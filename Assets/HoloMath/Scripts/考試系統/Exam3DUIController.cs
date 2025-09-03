using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Exam3DUIController : MonoBehaviour
{
    [Header("3D UI 面板")]
    public GameObject welcomePanel3D;
    public GameObject examPanel3D;
    public GameObject resultPanel3D;

    [Header("歡迎面板3D")]
    public TextMeshPro titleText3D;
    public TextMeshPro subtitleText3D;
    public GameObject startButton3D;

    [Header("考試面板3D")]
    public TextMeshPro questionText3D;
    public TextMeshPro progressText3D;
    public TextMeshPro timerText3D;
    public GameObject answerArea3D;
    public GameObject nextButton3D;
    public GameObject submitButton3D;

    [Header("結果面板3D")]
    public TextMeshPro finalScoreText3D;
    public TextMeshPro gradeText3D;
    public TextMeshPro detailsText3D;
    public GameObject retakeButton3D;
    public GameObject backButton3D;

    [Header("考試設定")]
    public float examTimeInSeconds = 1800f;
    public int passingScore = 60;

    [Header("動畫設定")]
    public bool useAnimations = true;
    public float animationSpeed = 0.5f;

    [Header("UI 縮放設定")]
    public float uiScale = 0.3f;
    public Vector3 uiPosition = new Vector3(0, 0, 2);
    public bool autoAdjustForMRTK = true;

    [Header("字體設定")]
    public TMP_FontAsset chineseFontAsset;

    // 考試邏輯變數
    private List<ExamQuestion3D> questions = new List<ExamQuestion3D>();
    private int currentQuestionIndex = 0;
    private List<bool> answers = new List<bool>();
    private float remainingTime;
    private bool examInProgress = false;
    private int totalScore = 0;
    private float percentage = 0f;
    private string grade = "";

    // 當前輸入框
    private TextMeshPro currentInput3D;
    private string currentAnswer = "";

    void Start()
    {
        Debug.Log("=== Exam3DUIController 開始初始化 ===");

        if (autoAdjustForMRTK)
            AdjustUIForMRTK();

        Setup3DButtons();
        PrepareQuestions();
        SetupAllFonts();
        ShowWelcomePanel3D();

        Debug.Log("=== Exam3DUIController 初始化完成 ===");
    }

    void Update()
    {
        if (examInProgress && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimer3D();

            if (remainingTime <= 0)
            {
                TimeUp();
            }
        }

        HandleInput3D();
    }

    #region UI 調整
    void AdjustUIForMRTK()
    {
        Debug.Log("調整 UI 尺寸適應 MRTK");

        // 調整整體縮放
        transform.localScale = Vector3.one * uiScale;

        // 調整位置
        transform.position = uiPosition;

        Debug.Log("UI 縮放調整完成 - Scale: " + transform.localScale + ", Position: " + transform.position);
    }
    #endregion

    #region 字體和顏色設置
    void SetupAllFonts()
    {
        Debug.Log("設置字體和顏色");

        if (chineseFontAsset != null)
        {
            if (titleText3D != null)
            {
                titleText3D.font = chineseFontAsset;
                titleText3D.color = Color.black;
                Debug.Log("TitleText 字體和顏色設置完成");
            }
            if (subtitleText3D != null)
            {
                subtitleText3D.font = chineseFontAsset;
                subtitleText3D.color = Color.black;
                Debug.Log("SubtitleText 字體和顏色設置完成");
            }
            if (questionText3D != null)
            {
                questionText3D.font = chineseFontAsset;
                questionText3D.color = Color.black;
                Debug.Log("QuestionText 字體和顏色設置完成");
            }
            if (progressText3D != null)
            {
                progressText3D.font = chineseFontAsset;
                progressText3D.color = Color.black;
                Debug.Log("ProgressText 字體和顏色設置完成");
            }
            if (timerText3D != null)
            {
                timerText3D.font = chineseFontAsset;
                timerText3D.color = Color.black;
                Debug.Log("TimerText 字體和顏色設置完成");
            }
            if (finalScoreText3D != null)
            {
                finalScoreText3D.font = chineseFontAsset;
                finalScoreText3D.color = Color.black;
                Debug.Log("FinalScoreText 字體和顏色設置完成");
            }
            if (gradeText3D != null)
            {
                gradeText3D.font = chineseFontAsset;
                gradeText3D.color = Color.black;
                Debug.Log("GradeText 字體和顏色設置完成");
            }
            if (detailsText3D != null)
            {
                detailsText3D.font = chineseFontAsset;
                detailsText3D.color = Color.black;
                Debug.Log("DetailsText 字體和顏色設置完成");
            }
        }
        else
        {
            Debug.LogWarning("中文字體未設置");
        }
    }
    #endregion

    #region 按鈕設置
    void Setup3DButtons()
    {
        Debug.Log("設置3D按鈕");

        if (startButton3D != null)
        {
            AddEnhancedButtonHandler(startButton3D, StartExam);
            Debug.Log("StartButton 設置完成");
        }
        else
        {
            Debug.LogError("StartButton3D 引用遺失");
        }

        if (nextButton3D != null)
            AddEnhancedButtonHandler(nextButton3D, NextQuestion);
        else
            Debug.LogError("NextButton3D 引用遺失");

        if (submitButton3D != null)
            AddEnhancedButtonHandler(submitButton3D, SubmitExam);
        else
            Debug.LogError("SubmitButton3D 引用遺失");

        if (retakeButton3D != null)
            AddEnhancedButtonHandler(retakeButton3D, RetakeExam);
        else
            Debug.LogError("RetakeButton3D 引用遺失");

        if (backButton3D != null)
            AddEnhancedButtonHandler(backButton3D, BackToMenu);
        else
            Debug.LogError("BackButton3D 引用遺失");
    }

    void AddEnhancedButtonHandler(GameObject button, System.Action action)
    {
        // 添加碰撞器
        if (button.GetComponent<Collider>() == null)
            button.AddComponent<BoxCollider>();

        // 添加增強按鈕處理器
        EnhancedButton3DHandler handler = button.GetComponent<EnhancedButton3DHandler>();
        if (handler == null)
            handler = button.AddComponent<EnhancedButton3DHandler>();

        handler.OnClick = action;
        handler.useAnimations = useAnimations;
    }
    #endregion

    #region 題目準備
    void PrepareQuestions()
    {
        Debug.Log("準備考試題目");

        questions.Clear();

        questions.Add(new ExamQuestion3D("1 + 1 = ?", "2"));
        questions.Add(new ExamQuestion3D("3 × 4 = ?", "12"));
        questions.Add(new ExamQuestion3D("√16 = ?", "4"));
        questions.Add(new ExamQuestion3D("sin(90度) = ?", "1"));
        questions.Add(new ExamQuestion3D("2的3次方 = ?", "8"));
        questions.Add(new ExamQuestion3D("π 約等於 ? (保留兩位小數)", "3.14"));
        questions.Add(new ExamQuestion3D("10 ÷ 2 = ?", "5"));

        answers.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            answers.Add(false);
        }

        Debug.Log("準備了 " + questions.Count + " 道考試題目");
    }
    #endregion

    #region 考試流程控制
    public void StartExam()
    {
        Debug.Log("開始 3D 考試");

        examInProgress = true;
        remainingTime = examTimeInSeconds;
        currentQuestionIndex = 0;

        ShowExamPanel3D();
        LoadCurrentQuestion3D();
    }

    void LoadCurrentQuestion3D()
    {
        Debug.Log("載入第 " + (currentQuestionIndex + 1) + " 題");

        if (currentQuestionIndex < questions.Count)
        {
            var question = questions[currentQuestionIndex];

            // 設置題目文字
            if (questionText3D != null)
            {
                if (chineseFontAsset != null)
                    questionText3D.font = chineseFontAsset;
                questionText3D.color = Color.black;

                // 直接設置題目文字，不使用動畫以避免問題
                questionText3D.text = question.questionText;

                Debug.Log("題目設置完成: " + question.questionText);
            }
            else
            {
                Debug.LogError("QuestionText3D 引用遺失");
            }

            // 設置進度文字
            if (progressText3D != null)
            {
                if (chineseFontAsset != null)
                    progressText3D.font = chineseFontAsset;
                progressText3D.color = Color.black;
                string progressText = "第 " + (currentQuestionIndex + 1) + " 題 / 共 " + questions.Count + " 題";
                progressText3D.text = progressText;
                Debug.Log("進度設置完成: " + progressText);
            }
            else
            {
                Debug.LogError("ProgressText3D 引用遺失");
            }

            SetupManualInputBox();
        }
        else
        {
            SubmitExam();
        }
    }

    void SetupManualInputBox()
    {
        Debug.Log("設置手動輸入框");

        if (answerArea3D != null)
        {
            // 尋找手動創建的輸入框
            Transform inputBox = answerArea3D.transform.Find("ManualInputBox");
            if (inputBox != null)
            {
                Debug.Log("找到 ManualInputBox");

                // 確保輸入框背景是白色
                Renderer renderer = inputBox.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = Color.white;
                    Debug.Log("確認輸入框背景為白色");
                }

                // 獲取文字組件
                TextMeshPro textComponent = inputBox.GetComponentInChildren<TextMeshPro>();
                if (textComponent != null)
                {
                    currentInput3D = textComponent;

                    // 設置字體
                    if (chineseFontAsset != null)
                        currentInput3D.font = chineseFontAsset;

                    // 重置答案
                    currentAnswer = "";
                    UpdateInputDisplay();

                    Debug.Log("手動輸入框設置完成");
                }
                else
                {
                    Debug.LogError("ManualInputBox 下找不到 TextMeshPro 組件");
                }
            }
            else
            {
                Debug.LogError("找不到 ManualInputBox，請確認物件名稱正確");

                // 列出 AnswerArea3D 下的所有子物件
                Debug.Log("AnswerArea3D 下的子物件：");
                for (int i = 0; i < answerArea3D.transform.childCount; i++)
                {
                    Debug.Log("- " + answerArea3D.transform.GetChild(i).name);
                }
            }
        }
        else
        {
            Debug.LogError("AnswerArea3D 引用遺失");
        }
    }

    void HandleInput3D()
    {
        if (currentInput3D != null && examInProgress)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // 退格鍵
                {
                    if (currentAnswer.Length > 0)
                    {
                        currentAnswer = currentAnswer.Substring(0, currentAnswer.Length - 1);
                        UpdateInputDisplay();
                    }
                }
                else if (c == '\n' || c == '\r') // Enter 鍵
                {
                    if (!string.IsNullOrEmpty(currentAnswer))
                        NextQuestion();
                }
                else if (char.IsLetterOrDigit(c) || c == '.' || c == '-')
                {
                    currentAnswer += c;
                    UpdateInputDisplay();
                }
            }
        }
    }

    void UpdateInputDisplay()
    {
        if (currentInput3D != null)
        {
            if (string.IsNullOrEmpty(currentAnswer))
            {
                currentInput3D.text = "請輸入答案...";
                currentInput3D.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 灰色
                currentInput3D.fontSize = 1.5f; // 小的提示文字
            }
            else
            {
                currentInput3D.text = currentAnswer;
                currentInput3D.color = Color.black;
                currentInput3D.fontSize = 2.5f; // 適中的輸入文字
            }

            Debug.Log("更新輸入顯示 - 文字: '" + currentInput3D.text + "', 字體大小: " + currentInput3D.fontSize);
        }
        else
        {
            Debug.LogError("currentInput3D 是 null，無法更新顯示");
        }
    }

    public void NextQuestion()
    {
        Debug.Log("下一題");

        RecordCurrentAnswer3D();
        currentQuestionIndex++;
        LoadCurrentQuestion3D();
    }

    void RecordCurrentAnswer3D()
    {
        if (currentQuestionIndex < questions.Count)
        {
            string correctAnswer = questions[currentQuestionIndex].correctAnswer;
            bool isCorrect = currentAnswer.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);
            answers[currentQuestionIndex] = isCorrect;

            Debug.Log("第 " + (currentQuestionIndex + 1) + " 題記錄：" + (isCorrect ? "正確" : "錯誤") + " (答案: " + currentAnswer + ", 正解: " + correctAnswer + ")");
        }
    }

    public void SubmitExam()
    {
        Debug.Log("提交考試");

        RecordCurrentAnswer3D();
        examInProgress = false;

        Calculate3DScore();
        ShowResultPanel3D();
        Display3DResults();
    }

    void Calculate3DScore()
    {
        int correctCount = 0;
        for (int i = 0; i < answers.Count; i++)
        {
            if (answers[i]) correctCount++;
        }

        totalScore = correctCount * 10;
        percentage = (float)correctCount / questions.Count * 100f;

        if (percentage >= 90) grade = "A";
        else if (percentage >= 80) grade = "B";
        else if (percentage >= 70) grade = "C";
        else if (percentage >= 60) grade = "D";
        else grade = "F";

        Debug.Log("計算成績完成 - 正確: " + correctCount + "/" + questions.Count + ", 分數: " + totalScore + ", 等級: " + grade);
    }

    void Display3DResults()
    {
        Debug.Log("顯示考試結果");

        if (finalScoreText3D != null)
        {
            if (chineseFontAsset != null)
                finalScoreText3D.font = chineseFontAsset;
            finalScoreText3D.color = Color.black;
            finalScoreText3D.text = "總分：" + totalScore + " 分";
        }

        if (gradeText3D != null)
        {
            if (chineseFontAsset != null)
                gradeText3D.font = chineseFontAsset;
            gradeText3D.color = Color.black;
            gradeText3D.text = "等級：" + grade;
        }

        if (detailsText3D != null)
        {
            if (chineseFontAsset != null)
                detailsText3D.font = chineseFontAsset;
            detailsText3D.color = Color.black;

            int correctCount = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                if (answers[i]) correctCount++;
            }

            string details = "答對：" + correctCount + "/" + questions.Count + "\n";
            details += "正確率：" + percentage.ToString("F1") + "%\n";
            details += percentage >= passingScore ? "恭喜通過！" : "繼續努力！";

            detailsText3D.text = details;
        }
    }

    void UpdateTimer3D()
    {
        if (timerText3D != null)
        {
            if (chineseFontAsset != null)
                timerText3D.font = chineseFontAsset;

            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText3D.text = "時間: " + minutes.ToString("00") + ":" + seconds.ToString("00");

            // 顏色警告
            if (remainingTime < 60f)
                timerText3D.color = Color.red;
            else if (remainingTime < 300f)
                timerText3D.color = new Color(1f, 0.5f, 0f);
            else
                timerText3D.color = Color.black;
        }
    }

    void TimeUp()
    {
        Debug.Log("時間到，自動提交");
        SubmitExam();
    }

    public void RetakeExam()
    {
        Debug.Log("重新考試");
        PrepareQuestions();
        StartExam();
    }

    public void BackToMenu()
    {
        Debug.Log("返回主選單");
        SceneManager.LoadScene("TestScene0623");
    }
    #endregion

    #region 面板切換系統
    void ShowWelcomePanel3D()
    {
        Debug.Log("顯示歡迎面板");

        DirectPanelSwitch(welcomePanel3D, examPanel3D, resultPanel3D);

        // 設置歡迎文字
        if (titleText3D != null)
        {
            if (chineseFontAsset != null)
                titleText3D.font = chineseFontAsset;
            titleText3D.color = Color.black;
            titleText3D.text = "HoloMath";
        }

        if (subtitleText3D != null)
        {
            if (chineseFontAsset != null)
                subtitleText3D.font = chineseFontAsset;
            subtitleText3D.color = Color.black;
            subtitleText3D.text = "3D數學測驗系統";
        }
    }

    void ShowExamPanel3D()
    {
        Debug.Log("顯示考試面板");
        DirectPanelSwitch(examPanel3D, welcomePanel3D, resultPanel3D);
    }

    void ShowResultPanel3D()
    {
        Debug.Log("顯示結果面板");
        DirectPanelSwitch(resultPanel3D, welcomePanel3D, examPanel3D);
    }

    void DirectPanelSwitch(GameObject showPanel, GameObject hidePanel1, GameObject hidePanel2)
    {
        SetPanel3DActive(showPanel, true);
        SetPanel3DActive(hidePanel1, false);
        SetPanel3DActive(hidePanel2, false);
    }
    #endregion

    #region 工具方法
    void SetPanel3DActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
    #endregion
}

// 增強版按鈕處理器
public class EnhancedButton3DHandler : MonoBehaviour
{
    public System.Action OnClick;
    public bool useAnimations = true;

    private Vector3 originalScale;
    private bool isPressed = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnMouseEnter()
    {
        if (useAnimations && !isPressed)
        {
            StartCoroutine(ScaleTo(originalScale * 1.1f, 0.1f));
        }
    }

    void OnMouseExit()
    {
        if (useAnimations && !isPressed)
        {
            StartCoroutine(ScaleTo(originalScale, 0.1f));
        }
    }

    void OnMouseDown()
    {
        isPressed = true;
        if (useAnimations)
        {
            StartCoroutine(ScaleTo(originalScale * 0.9f, 0.05f));
        }

        Debug.Log("按鈕按下: " + gameObject.name);
    }

    void OnMouseUp()
    {
        isPressed = false;
        if (useAnimations)
        {
            StartCoroutine(ScaleTo(originalScale, 0.1f));
        }

        Debug.Log("按鈕放開: " + gameObject.name);
        OnClick?.Invoke();
    }

    System.Collections.IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}

[System.Serializable]
public class ExamQuestion3D
{
    public string questionText;
    public string correctAnswer;

    public ExamQuestion3D(string question, string answer)
    {
        questionText = question;
        correctAnswer = answer;
    }
}