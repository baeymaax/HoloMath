using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public class JsonTutorialContent
{
    public string contentName;
    public string description;
    public string videoClip;  // 改为 string，因为 JSON 中存储的是路径
    public object questionImage;  // JSON 中可能为 null
    public bool hasImage;
    public string threeDObject;  // 改为 string，存储 GameObject 的名称
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
    public string questionType;  // 改为 string
    public string promptText;
    public string correctAnswer;
    public List<string> acceptableAnswers;
    public string hint;
    public string answerType;  // 改为 string
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

public class Json_Test : MonoBehaviour
{
    private List<JsonTutorialContent> jsonData;
    
    public void LoadJson()
    {
        try
        {
            #region 文件位置設定

            // 方案一：使用 Resources.Load（推薦）
            // 將文件放在 Assets/Resources/HoloMath/Data/ 資料夾中
            // TextAsset jsonFile = Resources.Load<TextAsset>("HoloMath/Data/math_questions");
            
            // 方案二：使用 Application.dataPath（適用於 Editor 和 Build）
            string filePath = Path.Combine(Application.dataPath, "HoloMath", "Data", "Chi_Data" , "math_questions.json");
            
            // 方案三：如果要使用 StreamingAssets，請將文件移動到 Assets/StreamingAssets/ 資料夾
            // string filePath = Path.Combine(Application.streamingAssetsPath, "math_questions.json");

            #endregion

            Debug.Log($"嘗試讀取文件路径: {filePath}");

            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                Debug.LogError($"JSON 文件不存在于路径: {filePath}");
                
                // 提供一些調試信息
                Debug.Log($"Application.dataPath: {Application.dataPath}");
                Debug.Log($"Application.streamingAssetsPath: {Application.streamingAssetsPath}");
                Debug.Log($"Application.persistentDataPath: {Application.persistentDataPath}");
                
                return;
            }
            
            string jsonContent = File.ReadAllText(filePath);
            Debug.Log($"读取到的 JSON 内容长度: {jsonContent.Length}");
            Debug.Log($"JSON 内容前 200 字符: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}");
            
            // 直接解析为数组
            jsonData = JsonHelper.FromJson<JsonTutorialContent>(jsonContent);
            
            if (jsonData != null && jsonData.Count > 0)
            {
                Debug.Log($"成功解析 JSON，共 {jsonData.Count} 个项目");
                for (int i = 0; i < jsonData.Count; i++)
                {
                    Debug.Log($"项目 {i}: threeDObject = {jsonData[i].threeDObject}, questions count = {jsonData[i].questions?.Count ?? 0}");
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
    
    // 使用 Resources 的替代方法
    public void LoadJsonFromResources()
    {
        try
        {
            // 將 JSON 文件放在 Assets/Resources/HoloMath/Data/ 資料夾中
            // 檔名應該是 math_questions.json，但載入時不需要副檔名
            TextAsset jsonFile = Resources.Load<TextAsset>("HoloMath/Data/math_questions");
            
            if (jsonFile == null)
            {
                Debug.LogError("無法從 Resources 載入 JSON 文件: HoloMath/Data/math_questions");
                Debug.Log("請確保文件位於 Assets/Resources/HoloMath/Data/math_questions.json");
                return;
            }
            
            string jsonContent = jsonFile.text;
            Debug.Log($"从 Resources 读取到的 JSON 内容长度: {jsonContent.Length}");
            
            // 直接解析为数组
            jsonData = JsonHelper.FromJson<JsonTutorialContent>(jsonContent);
            
            if (jsonData != null && jsonData.Count > 0)
            {
                Debug.Log($"成功解析 JSON，共 {jsonData.Count} 个项目");
                for (int i = 0; i < jsonData.Count; i++)
                {
                    Debug.Log($"项目 {i}: threeDObject = {jsonData[i].threeDObject}, questions count = {jsonData[i].questions?.Count ?? 0}");
                }
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
        if (jsonData == null || jsonData.Count == 0)
        {
            Debug.LogWarning("没有可用的 JSON 数据来应用到 TutorialManager");
            return;
        }
        
        try
        {
            // 创建新的 tutorialContents 数组
            TutorialContent_Test[] newContents = new TutorialContent_Test[jsonData.Count];
            
            for (int i = 0; i < jsonData.Count; i++)
            {
                var jsonContent = jsonData[i];
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
                
                // 处理视频片段（需要通过路径加载）
                if (!string.IsNullOrEmpty(jsonContent.videoClip))
                {
                    Debug.Log($"嘗試載入視頻: {jsonContent.videoClip}");
                    
                    // 統一路徑格式，將反斜線轉為正斜線
                    string normalizedPath = jsonContent.videoClip.Replace('\\', '/');
                    
                    #if UNITY_EDITOR
                    // 在 Editor 中使用 AssetDatabase
                    tutorialContent.videoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(normalizedPath);
                    
                    if (tutorialContent.videoClip != null)
                    {
                        //Debug.Log($"✓ 成功載入視頻: {tutorialContent.videoClip.name}");
                    }
                    else
                    {
                        Debug.LogError($"✗ 無法載入視頻: {normalizedPath}");
                        // 嘗試列出可能的路徑
                        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:VideoClip", new[] { "Assets/HoloMath/Video" });
                        Debug.Log($"在 Assets/HoloMath/Video 中找到的視頻文件:");
                        foreach (string guid in guids)
                        {
                            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                            Debug.Log($"  - {path}");
                        }
                    }
                    #else
                    // 在 Build 中，視頻應該已經被 Unity 自動包含
                    // 這裡我們需要用不同的方式，比如將視頻預先指派到 ScriptableObject 或者使用 Resources
                    Debug.LogWarning("在 Build 版本中載入 Assets 中的視頻需要預先設置。建議將視頻移至 Resources 資料夾。");
                    #endif
                }
                
                // 处理 3D 物件（需要通过名称查找）
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
                        
                        tutorialContent.questions.Add(question);
                    }
                }
                
                newContents[i] = tutorialContent;
            }
            
            // 通过反射设置 tutorialContents 字段
            var field = typeof(TutorialContentManager_Test).GetField("tutorialContents", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(manager, newContents);
                Debug.Log($"成功设置 tutorialContents，共 {newContents.Length} 个项目");
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
    
    private string GetResourcePath(string fullPath)
    {
        // 統一使用正斜線
        string resourcePath = fullPath.Replace('\\', '/');
        
        Debug.Log($"原始路徑: {fullPath}");
        Debug.Log($"統一斜線後: {resourcePath}");
        
        // 如果是完整的 Assets 路徑，轉換為 Resources 路徑
        if (resourcePath.StartsWith("Assets/"))
        {
            resourcePath = resourcePath.Substring(7); // 移除 "Assets/"
        }
        
        // 如果路徑包含 Resources/，只取 Resources/ 之後的部分
        int resourcesIndex = resourcePath.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            resourcePath = resourcePath.Substring(resourcesIndex + 10); // 移除 "Resources/"
        }
        
        // 移除文件擴展名（Resources.Load 不需要擴展名）
        int lastDotIndex = resourcePath.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            resourcePath = resourcePath.Substring(0, lastDotIndex);
        }
        
        Debug.Log($"最終 Resources 路徑: {resourcePath}");
        return resourcePath;
    }
}

// JSON 数组解析辅助类
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>("{\"Items\":" + json + "}");
        return wrapper.Items;
    }

    public static string ToJson<T>(List<T> array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(List<T> array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}

