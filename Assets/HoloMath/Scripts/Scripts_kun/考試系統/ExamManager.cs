using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Newtonsoft.Json;

public class ExamManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshPro examTitleText;
    public TextMeshPro timerText;
    public TextMeshPro questionCounterText;
    public TextMeshPro currentQuestionText;
    public Transform questionContainer;
    public Button nextButton;
    public Button previousButton;
    public Button submitButton;
    public GameObject resultPanel;

    [Header("Question Prefabs")]
    public GameObject multipleChoicePrefab;
    public GameObject fillInBlankPrefab;
    public GameObject trueFalsePrefab;

    [Header("Exam Settings")]
    public string examJsonPath = "ExamData/exam_2_1.json";

    // 私有變量
    private ExamData currentExam;
    private List<QuestionData> examQuestions;
    private Dictionary<string, StudentAnswer> studentAnswers;
    private int currentQuestionIndex = 0;
    private DateTime examStartTime;
    private bool examInProgress = false;
    private Coroutine timerCoroutine;

    // 事件系統
    public event Action<ExamResult> OnExamCompleted;
    public event Action<int> OnQuestionChanged;
    public event Action<TimeSpan> OnTimeUpdate;

    void Start()
    {
        InitializeExam();
    }

    public void InitializeExam()
    {
        LoadExamData();
        SetupUI();
        PrepareQuestions();
    }

    void LoadExamData()
    {
        try
        {
            // 從StreamingAssets讀取JSON
            string jsonPath = System.IO.Path.Combine(Application.streamingAssetsPath, examJsonPath);
            if (System.IO.File.Exists(jsonPath))
            {
                string jsonContent = System.IO.File.ReadAllText(jsonPath);
                currentExam = JsonConvert.DeserializeObject<ExamData>(jsonContent);
                Debug.Log($"載入考試: {currentExam.examName}，共 {currentExam.questions.Count} 題");
            }
            else
            {
                Debug.LogError($"找不到考試檔案: {jsonPath}");
                CreateDemoExam(); // 建立示範考試
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"載入考試資料失敗: {e.Message}");
            CreateDemoExam();
        }
    }

    void CreateDemoExam()
    {
        currentExam = new ExamData
        {
            examId = "demo_2_1",
            examName = "第二章第一節 - 三角函數基礎",
            description = "測試你對三角函數的理解",
            timeLimit = 30,
            passingScore = 70,
            shuffleQuestions = true,
            showResultsImmediately = true,
            questions = new List<QuestionData>
            {
                new QuestionData
                {
                    questionId = "q1",
                    questionText = "sin(90°) 的值是多少？",
                    type = QuestionType.MultipleChoice,
                    options = new List<string> { "0", "1", "0.5", "-1" },
                    correctAnswers = new List<string> { "1" },
                    points = 10,
                    explanation = "sin(90°) = 1，這是單位圓上的基本值"
                },
                new QuestionData
                {
                    questionId = "q2",
                    questionText = "cos(0°) = ___",
                    type = QuestionType.FillInBlank,
                    correctAnswers = new List<string> { "1" },
                    points = 10,
                    explanation = "cos(0°) = 1，在單位圓上0度對應的x坐標為1"
                }
            }
        };
    }

    void SetupUI()
    {
        if (examTitleText != null)
            examTitleText.text = currentExam.examName;

        if (nextButton != null)
            nextButton.onClick.AddListener(NextQuestion);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousQuestion);

        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitExam);
    }

    void PrepareQuestions()
    {
        examQuestions = new List<QuestionData>(currentExam.questions);

        if (currentExam.shuffleQuestions)
        {
            // 洗牌算法
            for (int i = 0; i < examQuestions.Count; i++)
            {
                var temp = examQuestions[i];
                int randomIndex = UnityEngine.Random.Range(i, examQuestions.Count);
                examQuestions[i] = examQuestions[randomIndex];
                examQuestions[randomIndex] = temp;
            }
        }

        studentAnswers = new Dictionary<string, StudentAnswer>();
        currentQuestionIndex = 0;

        StartExam();
    }

    public void StartExam()
    {
        examStartTime = DateTime.Now;
        examInProgress = true;

        // 開始計時器
        if (currentExam.timeLimit > 0)
        {
            timerCoroutine = StartCoroutine(ExamTimer());
        }

        DisplayCurrentQuestion();
        Debug.Log("考試開始！");
    }

    IEnumerator ExamTimer()
    {
        float totalTime = currentExam.timeLimit * 60f; // 轉換為秒
        float remainingTime = totalTime;

        while (remainingTime > 0 && examInProgress)
        {
            remainingTime -= Time.deltaTime;

            // 更新UI顯示
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
            if (timerText != null)
            {
                timerText.text = $"剩餘時間: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

                // 時間不足時變紅色警告
                if (remainingTime < 300) // 5分鐘
                {
                    timerText.color = Color.red;
                }
                else if (remainingTime < 600) // 10分鐘
                {
                    timerText.color = Color.yellow;
                }
            }

            OnTimeUpdate?.Invoke(timeSpan);
            yield return null;
        }

        if (examInProgress)
        {
            Debug.Log("時間到！自動提交考試");
            SubmitExam();
        }
    }

    void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= examQuestions.Count) return;

        var currentQuestion = examQuestions[currentQuestionIndex];

        // 更新題號顯示
        if (questionCounterText != null)
        {
            questionCounterText.text = $"第 {currentQuestionIndex + 1} 題 / 共 {examQuestions.Count} 題";
        }

        // 更新題目文字
        if (currentQuestionText != null)
        {
            currentQuestionText.text = currentQuestion.questionText;
        }

        // 清除之前的題目UI
        ClearQuestionContainer();

        // 根據題目類型生成UI
        CreateQuestionUI(currentQuestion);

        // 更新按鈕狀態
        UpdateNavigationButtons();

        OnQuestionChanged?.Invoke(currentQuestionIndex);
    }

    void ClearQuestionContainer()
    {
        if (questionContainer != null)
        {
            foreach (Transform child in questionContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void CreateQuestionUI(QuestionData question)
    {
        GameObject questionUI = null;

        switch (question.type)
        {
            case QuestionType.MultipleChoice:
            case QuestionType.MultipleAnswer:
                questionUI = Instantiate(multipleChoicePrefab, questionContainer);
                SetupMultipleChoiceUI(questionUI, question);
                break;

            case QuestionType.FillInBlank:
                questionUI = Instantiate(fillInBlankPrefab, questionContainer);
                SetupFillInBlankUI(questionUI, question);
                break;

            case QuestionType.TrueFalse:
                questionUI = Instantiate(trueFalsePrefab, questionContainer);
                SetupTrueFalseUI(questionUI, question);
                break;
        }
    }

    void SetupMultipleChoiceUI(GameObject questionUI, QuestionData question)
    {
        var optionComponents = questionUI.GetComponentsInChildren<Toggle>();

        for (int i = 0; i < optionComponents.Length && i < question.options.Count; i++)
        {
            var toggle = optionComponents[i];
            var optionText = toggle.GetComponentInChildren<TextMeshProUGUI>();

            if (optionText != null)
            {
                optionText.text = question.options[i];
            }

            // 設定事件監聽
            string optionValue = question.options[i];
            toggle.onValueChanged.AddListener((bool isOn) => {
                OnAnswerChanged(question.questionId, optionValue, isOn);
            });

            // 恢復之前的答案
            if (studentAnswers.ContainsKey(question.questionId))
            {
                var previousAnswer = studentAnswers[question.questionId];
                toggle.isOn = previousAnswer.selectedAnswers.Contains(optionValue);
            }
        }
    }

    void SetupFillInBlankUI(GameObject questionUI, QuestionData question)
    {
        var inputField = questionUI.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            // 恢復之前的答案
            if (studentAnswers.ContainsKey(question.questionId))
            {
                var previousAnswer = studentAnswers[question.questionId];
                inputField.text = previousAnswer.selectedAnswers.FirstOrDefault() ?? "";
            }

            // 設定事件監聽
            inputField.onValueChanged.AddListener((string value) => {
                OnAnswerChanged(question.questionId, value, true);
            });
        }
    }

    void SetupTrueFalseUI(GameObject questionUI, QuestionData question)
    {
        var toggles = questionUI.GetComponentsInChildren<Toggle>();

        if (toggles.Length >= 2)
        {
            // 假設第一個是True，第二個是False
            toggles[0].onValueChanged.AddListener((bool isOn) => {
                if (isOn) OnAnswerChanged(question.questionId, "True", true);
            });

            toggles[1].onValueChanged.AddListener((bool isOn) => {
                if (isOn) OnAnswerChanged(question.questionId, "False", true);
            });

            // 恢復之前的答案
            if (studentAnswers.ContainsKey(question.questionId))
            {
                var previousAnswer = studentAnswers[question.questionId];
                string answer = previousAnswer.selectedAnswers.FirstOrDefault();
                toggles[0].isOn = (answer == "True");
                toggles[1].isOn = (answer == "False");
            }
        }
    }

    void OnAnswerChanged(string questionId, string answer, bool isSelected)
    {
        if (!studentAnswers.ContainsKey(questionId))
        {
            studentAnswers[questionId] = new StudentAnswer
            {
                questionId = questionId,
                selectedAnswers = new List<string>()
            };
        }

        var studentAnswer = studentAnswers[questionId];

        if (isSelected && !studentAnswer.selectedAnswers.Contains(answer))
        {
            // 對於單選題，清除之前的答案
            var question = examQuestions.First(q => q.questionId == questionId);
            if (question.type == QuestionType.MultipleChoice || question.type == QuestionType.TrueFalse)
            {
                studentAnswer.selectedAnswers.Clear();
            }

            studentAnswer.selectedAnswers.Add(answer);
        }
        else if (!isSelected)
        {
            studentAnswer.selectedAnswers.Remove(answer);
        }

        Debug.Log($"題目 {questionId} 答案更新: {string.Join(", ", studentAnswer.selectedAnswers)}");
    }

    public void NextQuestion()
    {
        if (currentQuestionIndex < examQuestions.Count - 1)
        {
            currentQuestionIndex++;
            DisplayCurrentQuestion();
        }
    }

    public void PreviousQuestion()
    {
        if (currentQuestionIndex > 0)
        {
            currentQuestionIndex--;
            DisplayCurrentQuestion();
        }
    }

    void UpdateNavigationButtons()
    {
        if (previousButton != null)
            previousButton.interactable = currentQuestionIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentQuestionIndex < examQuestions.Count - 1;

        if (submitButton != null)
            submitButton.gameObject.SetActive(currentQuestionIndex == examQuestions.Count - 1);
    }

    public void SubmitExam()
    {
        if (!examInProgress) return;

        examInProgress = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        // 計算成績
        ExamResult result = CalculateExamResult();

        // 顯示結果
        DisplayExamResult(result);

        // 如果及格，頒發證書
        if (result.passed)
        {
            StartCoroutine(IssueCertificate(result));
        }

        OnExamCompleted?.Invoke(result);
    }

    ExamResult CalculateExamResult()
    {
        var result = new ExamResult
        {
            studentId = "student_001", // 應該從用戶系統獲取
            examId = currentExam.examId,
            answers = studentAnswers,
            completedAt = DateTime.Now,
            timeTaken = DateTime.Now - examStartTime
        };

        int totalScore = 0;
        int maxScore = 0;

        foreach (var question in examQuestions)
        {
            maxScore += question.points;

            if (studentAnswers.ContainsKey(question.questionId))
            {
                var studentAnswer = studentAnswers[question.questionId];
                bool isCorrect = CheckAnswerCorrectness(question, studentAnswer);

                studentAnswer.isCorrect = isCorrect;
                studentAnswer.pointsEarned = isCorrect ? question.points : 0;

                totalScore += studentAnswer.pointsEarned;
            }
        }

        result.totalScore = totalScore;
        result.maxScore = maxScore;
        result.percentage = maxScore > 0 ? (float)totalScore / maxScore * 100 : 0;
        result.passed = result.percentage >= currentExam.passingScore;

        return result;
    }

    bool CheckAnswerCorrectness(QuestionData question, StudentAnswer studentAnswer)
    {
        var correctAnswers = question.correctAnswers.ToHashSet();
        var studentAnswers = studentAnswer.selectedAnswers.ToHashSet();

        return correctAnswers.SetEquals(studentAnswers);
    }

    void DisplayExamResult(ExamResult result)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            var resultText = resultPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (resultText != null)
            {
                resultText.text = $@"
考試結果

總分: {result.totalScore} / {result.maxScore}
百分比: {result.percentage:F1}%
用時: {result.timeTaken.Minutes}分{result.timeTaken.Seconds}秒
結果: {(result.passed ? "及格" : "不及格")}

{(result.passed ? "恭喜！您已獲得證書資格" : "請繼續努力，再次挑戰！")}
";
            }
        }

        Debug.Log($"考試完成 - 分數: {result.totalScore}/{result.maxScore} ({result.percentage:F1}%)");
    }

    IEnumerator IssueCertificate(ExamResult result)
    {
        Debug.Log("開始頒發證書...");

        // 這裡會調用證書系統
        var certificateManager = FindAnyObjectByType<CertificateManager>();
        if (certificateManager != null)
        {
            yield return StartCoroutine(certificateManager.IssueCertificate(result));
        }

        Debug.Log("證書頒發完成");
    }
}