using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;

[Serializable]
public class ExamData
{
    public string examId;
    public string examName;
    public string description;
    public List<QuestionData> questions;
    public int timeLimit; // ����
    public int passingScore; // �ή����
    public bool shuffleQuestions;
    public bool showResultsImmediately;
    public DateTime examDate;
}

[Serializable]
public class QuestionData
{
    public string questionId;
    public string questionText;
    public QuestionType type;
    public List<string> options; // ����D�ﶵ
    public List<string> correctAnswers; // ���T����
    public int points; // �D�ؤ���
    public string explanation; // ����
    public string imageUrl; // �Ϥ�URL�]�p�G���^
}

public enum QuestionType
{
    MultipleChoice,    // ���
    MultipleAnswer,    // �h��
    FillInBlank,      // ���
    TrueFalse,        // �O�D�D
    Essay             // �ݵ��D
}

[Serializable]
public class ExamResult
{
    public string studentId;
    public string examId;
    public Dictionary<string, StudentAnswer> answers;
    public int totalScore;
    public int maxScore;
    public float percentage;
    public TimeSpan timeTaken;
    public DateTime completedAt;
    public bool passed;
    public string certificateHash; // �϶����Ү�hash
}

[Serializable]
public class StudentAnswer
{
    public string questionId;
    public List<string> selectedAnswers;
    public bool isCorrect;
    public int pointsEarned;
    public TimeSpan timeSpent;
}