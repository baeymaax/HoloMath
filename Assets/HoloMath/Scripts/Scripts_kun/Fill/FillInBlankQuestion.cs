using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FillInBlankQuestion
{
    public string questionId;
    public string questionText; // 例如: "3 + 5 = ___"
    public List<string> correctAnswers; // 可能有多個正確答案
    public string hint;
    public int points;

    public FillInBlankQuestion(string id, string text, List<string> answers, string hint = "", int points = 10)
    {
        this.questionId = id;
        this.questionText = text;
        this.correctAnswers = answers;
        this.hint = hint;
        this.points = points;
    }
}