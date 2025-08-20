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
        Debug.Log("Exam3DUIController 開始初始化");

        Setup3DButtons();
        PrepareQuestions();
        SetupAllFonts();
        ShowWelcomePanel3D();

        Debug.Log("Exam3DUIController 初始化完成");
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

    #region 字體設置
    void SetupAllFonts()
    {
        Debug.Log("設置字體");

        if (chineseFontAsset != null)
        {
            if (titleText3D != null) { titleText3D.font = chineseFontAsset; Debug.Log("TitleText 字體設置完成"); }
            if (subtitleText3D != null) { subtitleText3D.font = chineseFontAsset; Debug.Log("SubtitleText 字體設置完成"); }
            if (questionText3D != null) { questionText3D.font = chineseFontAsset; Debug.Log("QuestionText 字體設置完成"); }
            if (progressText3D != null) { progressText3D.font = chineseFontAsset; Debug.Log("ProgressText 字體設置完成"); }
            if (timerText3D != null) { timerText3D.font = chineseFontAsset; Debug.Log("TimerText 字體設置完成"); }
            if (finalScoreText3D != null) { finalScoreText3D.font = chineseFontAsset; Debug.Log("FinalScoreText 字體設置完成"); }
            if (gradeText3D != null) { gradeText3D.font = chineseFontAsset; Debug.Log("GradeText 字體設置完成"); }
            if (detailsText3D != null) { detailsText3D.font = chineseFontAsset; Debug.Log("DetailsText 字體設置完成"); }
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

                if (useAnimations)
                    StartCoroutine(TypewriterEffect(questionText3D, question.questionText));
                else
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
                string progressText = "第 " + (currentQuestionIndex + 1) + " 題 / 共 " + questions.Count + " 題";
                progressText3D.text = progressText;
                Debug.Log("進度設置完成: " + progressText);
            }
            else
            {
                Debug.LogError("ProgressText3D 引用遺失");
            }

            CreateAnswer3DUI();
        }
        else
        {
            SubmitExam();
        }
    }

    void CreateAnswer3DUI()
    {
        Debug.Log("創建 3D 答案輸入框");

        if (answerArea3D != null)
        {
            // 清除舊的答案區域
            foreach (Transform child in answerArea3D.transform)
            {
                if (child.name.Contains("AnswerInput"))
                {
                    Destroy(child.gameObject);
                    Debug.Log("清除舊輸入框");
                }
            }

            // 創建輸入框背景
            GameObject inputDisplay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            inputDisplay.name = "AnswerInputDisplay";
            inputDisplay.transform.SetParent(answerArea3D.transform, false);
            inputDisplay.transform.localPosition = Vector3.zero;
            inputDisplay.transform.localScale = new Vector3(2, 0.5f, 1);

            // 設置輸入框材質 - 使用簡單白色材質
            Renderer renderer = inputDisplay.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.white;
            renderer.material = mat;

            // 創建文字顯示
            GameObject textObj = new GameObject("InputText3D");
            textObj.transform.SetParent(inputDisplay.transform, false);
            textObj.transform.localPosition = new Vector3(0, 0, -0.01f);

            currentInput3D = textObj.AddComponent<TextMeshPro>();
            currentInput3D.text = "請輸入答案...";
            currentInput3D.fontSize = 6;
            currentInput3D.color = Color.gray;
            currentInput3D.alignment = TextAlignmentOptions.Center;

            if (chineseFontAsset != null)
                currentInput3D.font = chineseFontAsset;

            currentAnswer = "";

            // 輸入框出現動畫
            if (useAnimations)
                StartCoroutine(InputBoxAppearAnimation(inputDisplay));

            Debug.Log("3D 輸入框創建完成");
        }
        else
        {
            Debug.LogError("AnswerArea3D 引用遺失，請在 Inspector 中設置");
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
                currentInput3D.color = Color.gray;
            }
            else
            {
                currentInput3D.text = currentAnswer;
                currentInput3D.color = Color.black;
            }
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

            if (useAnimations)
                StartCoroutine(CountUpScore(0, totalScore, finalScoreText3D));
            else
                finalScoreText3D.text = "總分：" + totalScore + " 分";
        }

        if (gradeText3D != null)
        {
            if (chineseFontAsset != null)
                gradeText3D.font = chineseFontAsset;

            string gradeText = "等級：" + grade;
            if (useAnimations)
                StartCoroutine(TypewriterEffect(gradeText3D, gradeText));
            else
                gradeText3D.text = gradeText;
        }

        if (detailsText3D != null)
        {
            if (chineseFontAsset != null)
                detailsText3D.font = chineseFontAsset;

            int correctCount = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                if (answers[i]) correctCount++;
            }

            string details = "答對：" + correctCount + "/" + questions.Count + "\n";
            details += "正確率：" + percentage.ToString("F1") + "%\n";
            details += percentage >= passingScore ? "恭喜通過！" : "繼續努力！";

            if (useAnimations)
                StartCoroutine(TypewriterEffect(detailsText3D, details));
            else
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

            // 時間警告顏色
            if (remainingTime < 60f)
                timerText3D.color = Color.red;
            else if (remainingTime < 300f)
                timerText3D.color = Color.yellow;
            else
                timerText3D.color = Color.white;
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

        if (useAnimations)
            StartCoroutine(SmoothPanelTransition(welcomePanel3D, examPanel3D, resultPanel3D));
        else
            DirectPanelSwitch(welcomePanel3D, examPanel3D, resultPanel3D);

        // 設置歡迎文字
        if (titleText3D != null)
        {
            if (chineseFontAsset != null)
                titleText3D.font = chineseFontAsset;

            if (useAnimations)
                StartCoroutine(TypewriterEffect(titleText3D, "HoloMath"));
            else
                titleText3D.text = "HoloMath";
        }

        if (subtitleText3D != null)
        {
            if (chineseFontAsset != null)
                subtitleText3D.font = chineseFontAsset;

            if (useAnimations)
                StartCoroutine(TypewriterEffect(subtitleText3D, "3D數學測驗系統"));
            else
                subtitleText3D.text = "3D數學測驗系統";
        }
    }

    void ShowExamPanel3D()
    {
        Debug.Log("顯示考試面板");

        if (useAnimations)
            StartCoroutine(SmoothPanelTransition(examPanel3D, welcomePanel3D, resultPanel3D));
        else
            DirectPanelSwitch(examPanel3D, welcomePanel3D, resultPanel3D);
    }

    void ShowResultPanel3D()
    {
        Debug.Log("顯示結果面板");

        if (useAnimations)
            StartCoroutine(SmoothPanelTransition(resultPanel3D, welcomePanel3D, examPanel3D));
        else
            DirectPanelSwitch(resultPanel3D, welcomePanel3D, examPanel3D);
    }

    void DirectPanelSwitch(GameObject showPanel, GameObject hidePanel1, GameObject hidePanel2)
    {
        SetPanel3DActive(showPanel, true);
        SetPanel3DActive(hidePanel1, false);
        SetPanel3DActive(hidePanel2, false);
    }

    System.Collections.IEnumerator SmoothPanelTransition(GameObject showPanel, GameObject hidePanel1, GameObject hidePanel2)
    {
        // 簡化版滑動轉場
        if (showPanel != null)
        {
            Vector3 originalPos = showPanel.transform.localPosition;
            Vector3 startPos = originalPos + Vector3.down * 2f;

            SetPanel3DActive(showPanel, true);
            SetPanel3DActive(hidePanel1, false);
            SetPanel3DActive(hidePanel2, false);

            showPanel.transform.localPosition = startPos;

            float elapsed = 0f;
            while (elapsed < animationSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationSpeed;
                float smoothT = Mathf.SmoothStep(0, 1, t);

                showPanel.transform.localPosition = Vector3.Lerp(startPos, originalPos, smoothT);
                yield return null;
            }

            showPanel.transform.localPosition = originalPos;
        }
    }
    #endregion

    #region 動畫效果
    System.Collections.IEnumerator TypewriterEffect(TextMeshPro textComponent, string fullText)
    {
        if (textComponent == null) yield break;

        textComponent.text = "";
        float charDelay = 0.05f;

        for (int i = 0; i <= fullText.Length; i++)
        {
            if (textComponent != null)
                textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(charDelay);
        }
    }

    System.Collections.IEnumerator CountUpScore(int startScore, int endScore, TextMeshPro scoreText)
    {
        if (scoreText == null) yield break;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            int currentScore = Mathf.RoundToInt(Mathf.Lerp(startScore, endScore, t));
            if (scoreText != null)
                scoreText.text = "總分：" + currentScore + " 分";
            yield return null;
        }

        if (scoreText != null)
            scoreText.text = "總分：" + endScore + " 分";
    }

    System.Collections.IEnumerator InputBoxAppearAnimation(GameObject inputBox)
    {
        if (inputBox == null) yield break;

        Vector3 originalScale = inputBox.transform.localScale;
        inputBox.transform.localScale = Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            inputBox.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, smoothT);
            yield return null;
        }

        inputBox.transform.localScale = originalScale;
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

// 增強版按鈕處理器 - 不改變原有材質
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