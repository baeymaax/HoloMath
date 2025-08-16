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
    public int timeLimit; // 分鐘
    public int passingScore; // 及格分數
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
    public List<string> options; // 選擇題選項
    public List<string> correctAnswers; // 正確答案
    public int points; // 題目分數
    public string explanation; // 解釋
    public string imageUrl; // 圖片URL（如果有）
}

public enum QuestionType
{
    MultipleChoice,    // 單選
    MultipleAnswer,    // 多選
    FillInBlank,      // 填空
    TrueFalse,        // 是非題
    Essay             // 問答題
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
    public string certificateHash; // 區塊鏈證書hash
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