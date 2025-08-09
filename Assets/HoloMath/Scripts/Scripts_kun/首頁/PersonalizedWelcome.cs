using UnityEngine;
using TMPro;
using System.Collections;

public class PersonalizedWelcome : MonoBehaviour
{
    public TextMeshProUGUI welcomeText;

    void Start()
    {
        if (welcomeText == null)
            welcomeText = GetComponent<TextMeshProUGUI>();

        string userName = PlayerPrefs.GetString("UserName", "學習者");
        string fullText = $"歡迎回來，{userName}！";

        StartCoroutine(TypewriterEffect(fullText));
    }

    System.Collections.IEnumerator TypewriterEffect(string fullText)
    {
        if (welcomeText == null) yield break;

        welcomeText.text = "";

        foreach (char c in fullText)
        {
            welcomeText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    }
}