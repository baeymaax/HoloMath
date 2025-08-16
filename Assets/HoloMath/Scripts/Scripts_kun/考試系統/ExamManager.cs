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

    // �p���ܶq
    private ExamData currentExam;
    private List<QuestionData> examQuestions;
    private Dictionary<string, StudentAnswer> studentAnswers;
    private int currentQuestionIndex = 0;
    private DateTime examStartTime;
    private bool examInProgress = false;
    private Coroutine timerCoroutine;

    // �ƥ�t��
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
            // �qStreamingAssetsŪ��JSON
            string jsonPath = System.IO.Path.Combine(Application.streamingAssetsPath, examJsonPath);
            if (System.IO.File.Exists(jsonPath))
            {
                string jsonContent = System.IO.File.ReadAllText(jsonPath);
                currentExam = JsonConvert.DeserializeObject<ExamData>(jsonContent);
                Debug.Log($"���J�Ҹ�: {currentExam.examName}�A�@ {currentExam.questions.Count} �D");
            }
            else
            {
                Debug.LogError($"�䤣��Ҹ��ɮ�: {jsonPath}");
                CreateDemoExam(); // �إߥܽd�Ҹ�
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���J�Ҹո�ƥ���: {e.Message}");
            CreateDemoExam();
        }
    }

    void CreateDemoExam()
    {
        currentExam = new ExamData
        {
            examId = "demo_2_1",
            examName = "�ĤG���Ĥ@�` - �T����ư�¦",
            description = "���էA��T����ƪ��z��",
            timeLimit = 30,
            passingScore = 70,
            shuffleQuestions = true,
            showResultsImmediately = true,
            questions = new List<QuestionData>
            {
                new QuestionData
                {
                    questionId = "q1",
                    questionText = "sin(90�X) ���ȬO�h�֡H",
                    type = QuestionType.MultipleChoice,
                    options = new List<string> { "0", "1", "0.5", "-1" },
                    correctAnswers = new List<string> { "1" },
                    points = 10,
                    explanation = "sin(90�X) = 1�A�o�O����W���򥻭�"
                },
                new QuestionData
                {
                    questionId = "q2",
                    questionText = "cos(0�X) = ___",
                    type = QuestionType.FillInBlank,
                    correctAnswers = new List<string> { "1" },
                    points = 10,
                    explanation = "cos(0�X) = 1�A�b����W0�׹�����x���Ь�1"
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
            // �~�P��k
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

        // �}�l�p�ɾ�
        if (currentExam.timeLimit > 0)
        {
            timerCoroutine = StartCoroutine(ExamTimer());
        }

        DisplayCurrentQuestion();
        Debug.Log("�Ҹն}�l�I");
    }

    IEnumerator ExamTimer()
    {
        float totalTime = currentExam.timeLimit * 60f; // �ഫ����
        float remainingTime = totalTime;

        while (remainingTime > 0 && examInProgress)
        {
            remainingTime -= Time.deltaTime;

            // ��sUI���
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
            if (timerText != null)
            {
                timerText.text = $"�Ѿl�ɶ�: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

                // �ɶ��������ܬ���ĵ�i
                if (remainingTime < 300) // 5����
                {
                    timerText.color = Color.red;
                }
                else if (remainingTime < 600) // 10����
                {
                    timerText.color = Color.yellow;
                }
            }

            OnTimeUpdate?.Invoke(timeSpan);
            yield return null;
        }

        if (examInProgress)
        {
            Debug.Log("�ɶ���I�۰ʴ���Ҹ�");
            SubmitExam();
        }
    }

    void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= examQuestions.Count) return;

        var currentQuestion = examQuestions[currentQuestionIndex];

        // ��s�D�����
        if (questionCounterText != null)
        {
            questionCounterText.text = $"�� {currentQuestionIndex + 1} �D / �@ {examQuestions.Count} �D";
        }

        // ��s�D�ؤ�r
        if (currentQuestionText != null)
        {
            currentQuestionText.text = currentQuestion.questionText;
        }

        // �M�����e���D��UI
        ClearQuestionContainer();

        // �ھ��D�������ͦ�UI
        CreateQuestionUI(currentQuestion);

        // ��s���s���A
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

            // �]�w�ƥ��ť
            string optionValue = question.options[i];
            toggle.onValueChanged.AddListener((bool isOn) => {
                OnAnswerChanged(question.questionId, optionValue, isOn);
            });

            // ��_���e������
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
            // ��_���e������
            if (studentAnswers.ContainsKey(question.questionId))
            {
                var previousAnswer = studentAnswers[question.questionId];
                inputField.text = previousAnswer.selectedAnswers.FirstOrDefault() ?? "";
            }

            // �]�w�ƥ��ť
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
            // ���]�Ĥ@�ӬOTrue�A�ĤG�ӬOFalse
            toggles[0].onValueChanged.AddListener((bool isOn) => {
                if (isOn) OnAnswerChanged(question.questionId, "True", true);
            });

            toggles[1].onValueChanged.AddListener((bool isOn) => {
                if (isOn) OnAnswerChanged(question.questionId, "False", true);
            });

            // ��_���e������
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
            // ������D�A�M�����e������
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

        Debug.Log($"�D�� {questionId} ���ק�s: {string.Join(", ", studentAnswer.selectedAnswers)}");
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

        // �p�⦨�Z
        ExamResult result = CalculateExamResult();

        // ��ܵ��G
        DisplayExamResult(result);

        // �p�G�ή�A�{�o�Ү�
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
            studentId = "student_001", // ���ӱq�Τ�t�����
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
�Ҹյ��G

�`��: {result.totalScore} / {result.maxScore}
�ʤ���: {result.percentage:F1}%
�ή�: {result.timeTaken.Minutes}��{result.timeTaken.Seconds}��
���G: {(result.passed ? "�ή�" : "���ή�")}

{(result.passed ? "���ߡI�z�w��o�ҮѸ��" : "���~��V�O�A�A���D�ԡI")}
";
            }
        }

        Debug.Log($"�Ҹէ��� - ����: {result.totalScore}/{result.maxScore} ({result.percentage:F1}%)");
    }

    IEnumerator IssueCertificate(ExamResult result)
    {
        Debug.Log("�}�l�{�o�Ү�...");

        // �o�̷|�ե��ҮѨt��
        var certificateManager = FindAnyObjectByType<CertificateManager>();
        if (certificateManager != null)
        {
            yield return StartCoroutine(certificateManager.IssueCertificate(result));
        }

        Debug.Log("�Үѹ{�o����");
    }
}