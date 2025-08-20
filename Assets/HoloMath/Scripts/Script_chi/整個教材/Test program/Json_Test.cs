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
        try
        {
            // 文件路徑設定
            string filePath = Path.Combine(Application.dataPath, "HoloMath", "Data", "Chi_Data", "math_questions.json");
            
            Debug.Log($"嘗試讀取文件路径: {filePath}");
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"JSON 文件不存在于路径: {filePath}");
                return;
            }
            
            string jsonContent = File.ReadAllText(filePath);
            Debug.Log($"读取到的 JSON 内容长度: {jsonContent.Length}");
            
            // 解析新的JSON結構
            jsonData = JsonUtility.FromJson<JsonTutorialRoot>(jsonContent);
            
            if (jsonData != null && jsonData.units != null && jsonData.units.Count > 0)
            {
                Debug.Log($"成功解析 JSON，共 {jsonData.units.Count} 個單元");
                
                for (int i = 0; i < jsonData.units.Count; i++)
                {
                    var unit = jsonData.units[i];
                    Debug.Log($"單元 {i}: {unit.unitName}, 內容數量: {unit.contents?.Count ?? 0}");
                    
                    if (unit.contents != null)
                    {
                        for (int j = 0; j < unit.contents.Count; j++)
                        {
                            var content = unit.contents[j];
                            Debug.Log($"  內容 {j}: {content.contentName}, 題目數量: {content.questions?.Count ?? 0}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("JSON 解析失败或数据为空");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载 JSON 时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    public void LoadJsonFromResources()
    {
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("HoloMath/Data/math_questions");
            
            if (jsonFile == null)
            {
                Debug.LogError("無法從 Resources 載入 JSON 文件");
                return;
            }
            
            string jsonContent = jsonFile.text;
            Debug.Log($"从 Resources 读取到的 JSON 内容长度: {jsonContent.Length}");
            
            jsonData = JsonUtility.FromJson<JsonTutorialRoot>(jsonContent);
            
            if (jsonData != null && jsonData.units != null && jsonData.units.Count > 0)
            {
                Debug.Log($"成功解析 JSON，共 {jsonData.units.Count} 個單元");
            }
            else
            {
                Debug.LogError("JSON 解析失败或数据为空");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"从 Resources 加载 JSON 时出错: {e.Message}\n{e.StackTrace}");
        }
    }

    public void ApplyToTutorialManager(TutorialContentManager_Test manager)
    {
        if (jsonData == null || jsonData.units == null || jsonData.units.Count == 0)
        {
            Debug.LogWarning("没有可用的 JSON 数据来应用到 TutorialManager");
            return;
        }

        try
        {
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

            // 創建新的 tutorialContents 數組
            TutorialContent_Test[] newContents = allContents.ToArray();

            // 通過反射設置 tutorialContents 字段
            var field = typeof(TutorialContentManager_Test).GetField("tutorialContents",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(manager, newContents);
                Debug.Log($"成功设置 tutorialContents，共 {newContents.Length} 个项目");
                
                // 同時設置單元信息到TutorialManager
                manager.SetUnitsData(jsonData.units);
            }
            else
            {
                Debug.LogError("无法找到 tutorialContents 字段");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"应用 JSON 数据到 TutorialManager 时出错: {e.Message}\n{e.StackTrace}");
        }
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
                Debug.LogError($"✗ 無法載入視頻: {normalizedPath}");
            }
            #else
            Debug.LogWarning("在 Build 版本中載入 Assets 中的視頻需要預先設置。");
            #endif
        }

        // *** 處理圖片 - 從字串路徑載入 Texture2D ***
        if (jsonContent.hasImage && !string.IsNullOrEmpty(jsonContent.questionImage))
        {
            string imagePath = jsonContent.questionImage; // 現在是 string 類型
            string normalizedImagePath = imagePath.Replace('\\', '/');
            
            Debug.Log($"嘗試載入圖片: {normalizedImagePath}");
            
            #if UNITY_EDITOR
            // 在編輯器中使用 AssetDatabase 載入
            tutorialContent.questionImage = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(normalizedImagePath);
            
            if (tutorialContent.questionImage == null)
            {
                Debug.LogError($"✗ 無法載入圖片: {normalizedImagePath}");
                Debug.LogError("請確保：");
                Debug.LogError("1. 圖片檔案存在於指定路徑");
                Debug.LogError("2. 圖片已正確匯入到 Unity 專案中");
                Debug.LogError("3. 路徑格式正確（使用正斜杠 '/'）");
                tutorialContent.hasImage = false;
            }
            else
            {
                Debug.Log($"✓ 成功載入圖片: {normalizedImagePath}");
                Debug.Log($"圖片尺寸: {tutorialContent.questionImage.width}x{tutorialContent.questionImage.height}");
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
                Debug.LogWarning($"建置版本中無法載入圖片: {resourcePath}");
                Debug.LogWarning("請將圖片移至 Assets/Resources/ 資料夾中");
                tutorialContent.hasImage = false;
            }
            else
            {
                Debug.Log($"✓ 從 Resources 成功載入圖片: {resourcePath}");
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
            if (tutorialContent.threeDObject == null)
            {
                Debug.LogWarning($"无法找到 3D 物件: {jsonContent.threeDObject}");
            }
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
                Debug.LogWarning($"未知的问题类型: {typeString}");
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
                Debug.LogWarning($"未知的答案类型: {typeString}");
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