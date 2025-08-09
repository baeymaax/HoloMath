using UnityEngine;
using TMPro;

public class QuizQuestionComponent : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro questionTextMesh;
    
    void Awake()
    {
        // 如果沒有指定 TextMeshPro，嘗試自動找到
        if (questionTextMesh == null)
        {
            questionTextMesh = GetComponentInChildren<TextMeshPro>();
        }
    }

    public void SetQuestionText(string text)
    {
        if (questionTextMesh != null)
        {
            questionTextMesh.text = text;
        }
        else
        {
            Debug.LogWarning("QuestionTextMesh is not assigned in " + gameObject.name);
        }
    }
}