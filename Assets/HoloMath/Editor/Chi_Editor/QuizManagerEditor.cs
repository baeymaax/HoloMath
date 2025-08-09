#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuizManager))]
public class QuizManagerEditor : Editor
{
    private SerializedProperty questionProp;
    private SerializedProperty questionPositionProp;
    private SerializedProperty questionScaleProp;
    private SerializedProperty optionSpacingProp;
    private SerializedProperty optionScaleProp;
    private SerializedProperty questionPrefabProp;
    private SerializedProperty optionPrefabProp;
    private SerializedProperty containerParentProp;

    void OnEnable()
    {
        questionProp = serializedObject.FindProperty("question");
        questionPositionProp = serializedObject.FindProperty("questionPosition");
        questionScaleProp = serializedObject.FindProperty("questionScale");
        optionSpacingProp = serializedObject.FindProperty("optionSpacing");
        optionScaleProp = serializedObject.FindProperty("optionScale");
        questionPrefabProp = serializedObject.FindProperty("questionPrefab");
        optionPrefabProp = serializedObject.FindProperty("optionPrefab");
        containerParentProp = serializedObject.FindProperty("containerParent");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        QuizManager quizManager = (QuizManager)target;

        // 標題
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quiz Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 測驗內容區域
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Quiz Content", EditorStyles.boldLabel);
        
        // 題目輸入
        SerializedProperty questionTextProp = questionProp.FindPropertyRelative("questionText");
        EditorGUILayout.LabelField("Question:");
        questionTextProp.stringValue = EditorGUILayout.TextArea(questionTextProp.stringValue, GUILayout.MinHeight(60));
        
        EditorGUILayout.Space();
        
        // 選項區域
        EditorGUILayout.LabelField("Options:", EditorStyles.boldLabel);
        SerializedProperty optionsProp = questionProp.FindPropertyRelative("options");
        
        // 選項列表
        for (int i = 0; i < optionsProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            SerializedProperty optionProp = optionsProp.GetArrayElementAtIndex(i);
            SerializedProperty optionTextProp = optionProp.FindPropertyRelative("optionText");
            SerializedProperty isCorrectProp = optionProp.FindPropertyRelative("isCorrect");
            
            // 選項編號
            EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(20));
            
            // 選項文字
            optionTextProp.stringValue = EditorGUILayout.TextField(optionTextProp.stringValue);
            
            // 正確答案勾選
            isCorrectProp.boolValue = EditorGUILayout.Toggle("Correct", isCorrectProp.boolValue, GUILayout.Width(70));
            
            // 刪除按鈕
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                optionsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // 添加選項按鈕
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+ Add Option", GUILayout.Width(100)))
        {
            optionsProp.arraySize++;
            SerializedProperty newOption = optionsProp.GetArrayElementAtIndex(optionsProp.arraySize - 1);
            newOption.FindPropertyRelative("optionText").stringValue = $"Option {optionsProp.arraySize}";
            newOption.FindPropertyRelative("isCorrect").boolValue = false;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // 排版設定區域
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Layout Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(questionPositionProp, new GUIContent("Question Position"));
        EditorGUILayout.PropertyField(questionScaleProp, new GUIContent("Question Scale"));
        EditorGUILayout.PropertyField(optionSpacingProp, new GUIContent("Option Spacing"));
        EditorGUILayout.PropertyField(optionScaleProp, new GUIContent("Option Scale"));
        
        EditorGUILayout.EndVertical();
        
        // Prefab 設定區域
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(questionPrefabProp, new GUIContent("Question Prefab"));
        EditorGUILayout.PropertyField(optionPrefabProp, new GUIContent("Option Prefab"));
        EditorGUILayout.PropertyField(containerParentProp, new GUIContent("Container Parent"));
        
        EditorGUILayout.EndVertical();
        
        // 生成按鈕
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Quiz", GUILayout.Height(30)))
        {
            quizManager.GenerateQuiz();
        }
        
        // 預覽資訊
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("Preview Info:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Question: {(string.IsNullOrEmpty(questionTextProp.stringValue) ? "Empty" : questionTextProp.stringValue.Substring(0, Mathf.Min(30, questionTextProp.stringValue.Length)) + "...")}");
        EditorGUILayout.LabelField($"Options Count: {optionsProp.arraySize}");
        
        int correctCount = 0;
        for (int i = 0; i < optionsProp.arraySize; i++)
        {
            if (optionsProp.GetArrayElementAtIndex(i).FindPropertyRelative("isCorrect").boolValue)
                correctCount++;
        }
        EditorGUILayout.LabelField($"Correct Answers: {correctCount}");
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif