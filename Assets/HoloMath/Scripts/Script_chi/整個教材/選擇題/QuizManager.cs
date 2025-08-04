using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

[System.Serializable]
public class QuizOption
{
    public string optionText;
    public bool isCorrect;
}

[System.Serializable]
public class QuizQuestion
{
    public string questionText;
    public List<QuizOption> options = new List<QuizOption>();
}

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Content")]
    public QuizQuestion question = new QuizQuestion();
    
    [Header("Layout Settings")]
    [SerializeField] private Vector3 questionPosition = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 questionScale = Vector3.one;
    [SerializeField] private float optionSpacing = 0.3f;
    [SerializeField] private Vector3 optionScale = Vector3.one;
    
    [Header("Prefab Settings")]
    [SerializeField] private GameObject questionPrefab;
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Transform containerParent;
    
    [Header("Runtime References")]
    private GameObject questionObject;
    private List<GameObject> optionObjects = new List<GameObject>();
    
    // 修改：使用 HashSet 來追蹤多個選中的選項，而不是單一的 selectedOptionIndex
    private HashSet<int> selectedOptionIndices = new HashSet<int>();

    void Start()
    {
        GenerateQuiz();
    }

    [ContextMenu("Generate Quiz")]
    public void GenerateQuiz()
    {
        ClearExistingQuiz();
        CreateQuestionObject();
        CreateOptionObjects();
    }

    private void ClearExistingQuiz()
    {
        if (questionObject != null)
        {
            DestroyImmediate(questionObject);
            questionObject = null;
        }

        foreach (GameObject option in optionObjects)
        {
            if (option != null)
                DestroyImmediate(option);
        }
        optionObjects.Clear();
        
        // 清空選中狀態
        selectedOptionIndices.Clear();
    }

    private void CreateQuestionObject()
    {
        if (questionPrefab == null || containerParent == null) return;

        questionObject = Instantiate(questionPrefab, containerParent);
        questionObject.name = "Question";
        
        // 設置題目位置和大小
        questionObject.transform.localPosition = questionPosition;
        questionObject.transform.localScale = questionScale;
        
        // 設置題目文字
        QuizQuestionComponent questionComponent = questionObject.GetComponent<QuizQuestionComponent>();
        if (questionComponent != null)
        {
            questionComponent.SetQuestionText(question.questionText);
        }
    }

    private void CreateOptionObjects()
    {
        if (optionPrefab == null || containerParent == null) return;

        for (int i = 0; i < question.options.Count; i++)
        {
            GameObject optionObj = Instantiate(optionPrefab, containerParent);
            optionObj.name = $"Option_{i}";
            
            // 計算選項位置（從題目位置開始，向下排列）
            Vector3 optionPosition = questionPosition + new Vector3(0, -(i + 1) * optionSpacing, 0);
            optionObj.transform.localPosition = optionPosition;
            optionObj.transform.localScale = optionScale;
            
            // 設置選項內容
            QuizOptionComponent optionComponent = optionObj.GetComponent<QuizOptionComponent>();
            if (optionComponent != null)
            {
                optionComponent.Initialize(i, question.options[i].optionText, this);
            }
            
            optionObjects.Add(optionObj);
        }
    }

    // 新的方法：處理選項切換
    public void OnOptionToggled(int optionIndex, bool isSelected)
    {
        Debug.Log($"Option {optionIndex} toggled to: {isSelected}");
        
        if (isSelected)
        {
            selectedOptionIndices.Add(optionIndex);
        }
        else
        {
            selectedOptionIndices.Remove(optionIndex);
        }
        
        // 輸出當前選中的所有選項（用於調試）
        Debug.Log($"Currently selected options: [{string.Join(", ", selectedOptionIndices)}]");
    }

    // 保留原有方法以維持相容性（但改為使用新的多選邏輯）
    public void OnOptionSelected(int optionIndex)
    {
        // 檢查該選項是否已經選中
        bool isCurrentlySelected = selectedOptionIndices.Contains(optionIndex);
        
        // 切換該選項的狀態
        if (isCurrentlySelected)
        {
            selectedOptionIndices.Remove(optionIndex);
        }
        else
        {
            selectedOptionIndices.Add(optionIndex);
        }
        
        // 更新該選項的視覺狀態
        if (optionIndex >= 0 && optionIndex < optionObjects.Count)
        {
            QuizOptionComponent optionComponent = optionObjects[optionIndex].GetComponent<QuizOptionComponent>();
            if (optionComponent != null)
            {
                optionComponent.SetSelected(!isCurrentlySelected);
            }
        }
    }

    // 新方法：獲取所有選中的選項
    public HashSet<int> GetSelectedOptions()
    {
        return new HashSet<int>(selectedOptionIndices);
    }

    // 新方法：獲取選中的選項列表
    public List<int> GetSelectedOptionsList()
    {
        return new List<int>(selectedOptionIndices);
    }

    // 修改：檢查是否有任何選項被選中
    public bool HasSelectedOptions()
    {
        return selectedOptionIndices.Count > 0;
    }

    // 修改：檢查答案是否正確（支援多選）
    public bool IsCorrectAnswer()
    {
        // 找出所有正確答案
        HashSet<int> correctAnswers = new HashSet<int>();
        for (int i = 0; i < question.options.Count; i++)
        {
            if (question.options[i].isCorrect)
            {
                correctAnswers.Add(i);
            }
        }
        
        // 比較選中的答案和正確答案是否完全一致
        return selectedOptionIndices.SetEquals(correctAnswers);
    }

    // 新方法：獲取正確答案的索引
    public HashSet<int> GetCorrectAnswers()
    {
        HashSet<int> correctAnswers = new HashSet<int>();
        for (int i = 0; i < question.options.Count; i++)
        {
            if (question.options[i].isCorrect)
            {
                correctAnswers.Add(i);
            }
        }
        return correctAnswers;
    }

    // 新方法：清空所有選擇
    public void ClearAllSelections()
    {
        foreach (int optionIndex in selectedOptionIndices)
        {
            if (optionIndex >= 0 && optionIndex < optionObjects.Count)
            {
                QuizOptionComponent optionComponent = optionObjects[optionIndex].GetComponent<QuizOptionComponent>();
                if (optionComponent != null)
                {
                    optionComponent.SetSelected(false);
                }
            }
        }
        selectedOptionIndices.Clear();
    }

    // Inspector 中的按鈕功能
    [ContextMenu("Add Option")]
    public void AddOption()
    {
        question.options.Add(new QuizOption());
    }

    [ContextMenu("Remove Last Option")]
    public void RemoveLastOption()
    {
        if (question.options.Count > 0)
        {
            question.options.RemoveAt(question.options.Count - 1);
        }
    }

    // 新增：用於調試的方法
    [ContextMenu("Clear All Selections")]
    public void ClearAllSelectionsDebug()
    {
        ClearAllSelections();
    }
}