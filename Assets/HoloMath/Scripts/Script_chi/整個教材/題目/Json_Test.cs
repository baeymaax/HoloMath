using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// 新增單元類別
[Serializable]
public class JsonTutorialUnit
{
    public string unitName;
    public string unitDescription;
    public int unitId;
    public List<JsonTutorialContent> contents;
}

// 原有的內容類別保持不變
[Serializable]
public class JsonTutorialContent
{
    public string contentName;
    public string description;
    public string videoClip;
    public string questionImage;
    public bool hasImage;
    public string threeDObject;
    public string questionText;
    public bool useCustomQuestionTextSettings;
    public Vector3 questionTextPosition;
    public Vector3 questionTextRotation;
    public float questionTextFontSize;
    public Vector2 questionTextSize;
    public bool showQuestionText;
    public List<JsonTutorialQuestion> questions;
    public bool showHints;
    public bool allowRetry;
    public bool showProgress;
    public int passingScore;
    public bool requireAllCorrect;
}

[Serializable]
public class JsonTutorialQuestion
{
    public string questionType;
    public string promptText;
    public string correctAnswer;
    public List<string> acceptableAnswers;
    public string hint;
    public string answerType;
    public float tolerance;
    public bool isCaseSensitive;
    public bool allowPartialMatch;
    public List<JsonQuizOption> options;
    public Vector3 textPosition;
    public Vector3 inputFieldPosition;
    public Vector3 textRotation;
    public Vector3 inputFieldRotation;
    public Vector2 textSize;
    public Vector3 inputFieldScale;
    public bool useCustomPositions;
    public float optionSpacing;
    public Vector3 optionScale;
    public bool useCustomQuestionTextPosition;
    public Vector3 questionTextPosition;
    public bool useCustomOptionPositions;
    public Vector3 optionStartPosition;
    public float questionTextFontSize;
}

[Serializable]
public class JsonQuizOption
{
    public string optionText;
    public bool isCorrect;
}

// 新的JSON根結構
[Serializable]
public class JsonTutorialRoot
{
    public List<JsonTutorialUnit> units;
}

public class Json_Test : MonoBehaviour
{
    private JsonTutorialRoot jsonData;

    public void LoadJson()
    {
            // 文件路徑設定
            string filePath = Path.Combine(Application.dataPath, "HoloMath", "Data", "Chi_Data", "math_questions.json");
            
            // 檢查每一層資料夾是否存在
            string dataPath = Path.Combine(Application.dataPath, "HoloMath");
            
            dataPath = Path.Combine(Application.dataPath, "HoloMath", "Data");
            
            dataPath = Path.Combine(Application.dataPath, "HoloMath", "Data", "Chi_Data");
        
            if (!File.Exists(filePath))
            {
                
                // 嘗試其他可能的路徑
                string[] alternativePaths = {
                    Path.Combine(Application.dataPath, "math_questions.json"),
                    Path.Combine(Application.dataPath, "Data", "math_questions.json"),
                    Path.Combine(Application.dataPath, "HoloMath", "math_questions.json"),
                    Path.Combine(Application.streamingAssetsPath, "math_questions.json")
                };
                
                foreach (string altPath in alternativePaths)
                {
                    if (File.Exists(altPath))
                    {
                        filePath = altPath;
                        break;
                    }
                }
                
                if (!File.Exists(filePath))
                {
                    return;
                }
            }
        
            string jsonContent = File.ReadAllText(filePath);
        
            // 驗證 JSON 格式
            if (string.IsNullOrEmpty(jsonContent))
            {
                return;
            }
            
            if (!jsonContent.TrimStart().StartsWith("{"))
            {
                return;
            }
            
            // 解析新的JSON結構
            jsonData = JsonUtility.FromJson<JsonTutorialRoot>(jsonContent);
        
            if (jsonData != null && jsonData.units != null && jsonData.units.Count > 0)
            {
            
                for (int i = 0; i < jsonData.units.Count; i++)
                {
                    var unit = jsonData.units[i];
                
                    if (unit.contents != null)
                    {
                        for (int j = 0; j < unit.contents.Count; j++)
                        {
                            var content = unit.contents[j];
                        }
                    }
                }
            }
        
    }

    public void LoadJsonFromResources()
    {
            TextAsset jsonFile = Resources.Load<TextAsset>("HoloMath/Data/math_questions");
            
            if (jsonFile == null)
            {
                return;
            }
            
            string jsonContent = jsonFile.text;
            
            jsonData = JsonUtility.FromJson<JsonTutorialRoot>(jsonContent);
    }

    public void ApplyToTutorialManager(TutorialContentManager_Test manager)
    {
        if (jsonData == null || jsonData.units == null || jsonData.units.Count == 0)
        {
            return;
        }
            
            // 將所有單元的內容展開為原來的數組格式（暫時保持相容性）
            List<TutorialContent_Test> allContents = new List<TutorialContent_Test>();
        
            foreach (var unit in jsonData.units)
            {
                
                if (unit.contents != null)
                {
                    foreach (var jsonContent in unit.contents)
                    {
                        var tutorialContent = ConvertJsonContentToTutorialContent(jsonContent);
                        allContents.Add(tutorialContent);
                    }
                }
            }

            // 直接設置 List（不再使用反射）
            // 確保 manager 的 tutorialContents 是 List 類型
            manager.SetTutorialContents(allContents);
            
            // 同時設置單元信息到TutorialManager
            manager.SetUnitsData(jsonData.units);
            
    }

    private TutorialContent_Test ConvertJsonContentToTutorialContent(JsonTutorialContent jsonContent)
    {
        var tutorialContent = new TutorialContent_Test();
        
        // 复制基本属性
        tutorialContent.contentName = jsonContent.contentName ?? "";
        tutorialContent.description = jsonContent.description ?? "";
        tutorialContent.hasImage = jsonContent.hasImage;
        tutorialContent.questionText = jsonContent.questionText ?? "";
        
        // 复制问题文本设置
        tutorialContent.useCustomQuestionTextSettings = jsonContent.useCustomQuestionTextSettings;
        tutorialContent.questionTextPosition = jsonContent.questionTextPosition;
        tutorialContent.questionTextRotation = jsonContent.questionTextRotation;
        tutorialContent.questionTextFontSize = jsonContent.questionTextFontSize;
        tutorialContent.questionTextSize = jsonContent.questionTextSize;
        tutorialContent.showQuestionText = jsonContent.showQuestionText;
        
        // 复制其他设置
        tutorialContent.showHints = jsonContent.showHints;
        tutorialContent.allowRetry = jsonContent.allowRetry;
        tutorialContent.showProgress = jsonContent.showProgress;
        tutorialContent.passingScore = jsonContent.passingScore;
        tutorialContent.requireAllCorrect = jsonContent.requireAllCorrect;
        
        // 处理视频片段
        if (!string.IsNullOrEmpty(jsonContent.videoClip))
        {
            string normalizedPath = jsonContent.videoClip.Replace('\\', '/');

            #if UNITY_EDITOR
            tutorialContent.videoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(normalizedPath);
            if (tutorialContent.videoClip == null)
            {
                Debug.LogWarning($"無法載入影片: {normalizedPath}");
            }
            else
            {
                Debug.Log($"成功載入影片: {normalizedPath} -> {tutorialContent.videoClip.name}");
            }
            #else
            Debug.LogWarning($"非編輯器模式,無法載入影片: {normalizedPath}");
            #endif
        }

        // *** 處理圖片 - 從字串路徑載入 Texture2D ***
        if (jsonContent.hasImage && !string.IsNullOrEmpty(jsonContent.questionImage))
        {
            string imagePath = jsonContent.questionImage; // 現在是 string 類型
            string normalizedImagePath = imagePath.Replace('\\', '/');
            
            
            #if UNITY_EDITOR
            // 在編輯器中使用 AssetDatabase 載入
            tutorialContent.questionImage = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(normalizedImagePath);
            
            if (tutorialContent.questionImage == null)
            {
                tutorialContent.hasImage = false;
            }
            #else
            // 在建置版本中，需要使用其他方法載入圖片
            // 方案1：使用 Resources.Load（需要將圖片放在 Resources 資料夾）
            string resourcePath = normalizedImagePath
                .Replace("Assets/Resources/", "")
                .Replace(".png", "")
                .Replace(".jpg", "")
                .Replace(".jpeg", "");
            
            tutorialContent.questionImage = Resources.Load<Texture2D>(resourcePath);
            
            if (tutorialContent.questionImage == null)
            {
                tutorialContent.hasImage = false;
            }
            #endif
        }
        else
        {
            // 如果沒有圖片或路徑為空，確保設定正確
            tutorialContent.questionImage = null;
            tutorialContent.hasImage = false;
        }
        
        // 处理 3D 物件
        if (!string.IsNullOrEmpty(jsonContent.threeDObject))
        {
            tutorialContent.threeDObject = GameObject.Find(jsonContent.threeDObject);
        }
        
        // 转换问题
        if (jsonContent.questions != null && jsonContent.questions.Count > 0)
        {
            tutorialContent.questions = new List<TutorialQuestion_Test>();
            
            foreach (var jsonQuestion in jsonContent.questions)
            {
                var question = ConvertJsonQuestionToTutorialQuestion(jsonQuestion);
                tutorialContent.questions.Add(question);
            }
        }
        
        return tutorialContent;
    }

    private TutorialQuestion_Test ConvertJsonQuestionToTutorialQuestion(JsonTutorialQuestion jsonQuestion)
    {
        var question = new TutorialQuestion_Test();
        
        // 转换问题类型
        question.questionType = ConvertQuestionType(jsonQuestion.questionType);
        question.promptText = jsonQuestion.promptText ?? "";
        question.correctAnswer = jsonQuestion.correctAnswer ?? "";
        question.acceptableAnswers = jsonQuestion.acceptableAnswers ?? new List<string>();
        question.hint = jsonQuestion.hint ?? "";
        question.answerType_Test = ConvertAnswerType(jsonQuestion.answerType);
        question.tolerance = jsonQuestion.tolerance;
        question.isCaseSensitive = jsonQuestion.isCaseSensitive;
        question.allowPartialMatch = jsonQuestion.allowPartialMatch;
        
        // 复制位置设置
        question.textPosition = jsonQuestion.textPosition;
        question.inputFieldPosition = jsonQuestion.inputFieldPosition;
        question.textRotation = jsonQuestion.textRotation;
        question.inputFieldRotation = jsonQuestion.inputFieldRotation;
        question.textSize = jsonQuestion.textSize;
        question.inputFieldScale = jsonQuestion.inputFieldScale;
        question.useCustomPositions = jsonQuestion.useCustomPositions;
        
        // 复制选择题设置
        question.optionSpacing = jsonQuestion.optionSpacing;
        question.optionScale = jsonQuestion.optionScale;
        question.useCustomQuestionTextPosition = jsonQuestion.useCustomQuestionTextPosition;
        question.questionTextPosition = jsonQuestion.questionTextPosition;
        question.useCustomOptionPositions = jsonQuestion.useCustomOptionPositions;
        question.optionStartPosition = jsonQuestion.optionStartPosition;
        question.questionTextFontSize = jsonQuestion.questionTextFontSize;
        
        // 转换选项
        if (jsonQuestion.options != null && jsonQuestion.options.Count > 0)
        {
            question.options = new List<QuizOption_Test>();
            foreach (var jsonOption in jsonQuestion.options)
            {
                question.options.Add(new QuizOption_Test
                {
                    optionText = jsonOption.optionText ?? "",
                    isCorrect = jsonOption.isCorrect
                });
            }
        }
        
        return question;
    }

    private QuestionType_Test ConvertQuestionType(string typeString)
    {
        switch (typeString)
        {
            case "FillInBlank":
                return QuestionType_Test.FillInBlank;
            case "MultipleChoice":
                return QuestionType_Test.MultipleChoice;
            default:
                return QuestionType_Test.FillInBlank;
        }
    }

    private AnswerType_Test ConvertAnswerType(string typeString)
    {
        switch (typeString)
        {
            case "Text":
                return AnswerType_Test.Text;
            case "Number":
                return AnswerType_Test.Number;
            case "Expression":
                return AnswerType_Test.Expression;
            default:
                return AnswerType_Test.Text;
        }
    }

    // 新增方法：獲取單元數據
    public JsonTutorialRoot GetJsonData()
    {
        return jsonData;
    }

    public List<JsonTutorialUnit> GetUnits()
    {
        return jsonData?.units ?? new List<JsonTutorialUnit>();
    }

    public JsonTutorialUnit GetUnit(int unitIndex)
    {
        if (jsonData?.units != null && unitIndex >= 0 && unitIndex < jsonData.units.Count)
        {
            return jsonData.units[unitIndex];
        }
        return null;
    }

    public JsonTutorialContent GetContent(int unitIndex, int contentIndex)
    {
        var unit = GetUnit(unitIndex);
        if (unit?.contents != null && contentIndex >= 0 && contentIndex < unit.contents.Count)
        {
            return unit.contents[contentIndex];
        }
        return null;
    }
}