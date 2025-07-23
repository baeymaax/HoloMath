using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Data;

public class Calculator3D : MonoBehaviour
{
    [Header("Display Settings")]
    public TextMeshPro displayText;  // 改為 TextMeshPro (3D版本)
    
    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip errorSound;
    
    private string currentInput = "";
    private double result = 0.0;

    void Start()
    {
        // 確保顯示初始化
        if (displayText == null)
        {
            Debug.LogError("DisplayText (TextMeshPro) 未設置！");
            return;
        }
        
        // 設置3D文本的基本屬性
        displayText.fontSize = 2.0f;  // 適合3D場景的字體大小
        displayText.alignment = TextAlignmentOptions.Right;  // 計算器通常右對齊
        displayText.color = Color.white;
        
        UpdateDisplay();
    }

    public void OnButtonClick(string buttonValue)
    {
        // 播放按鈕音效
        PlayButtonSound();
        
        if (buttonValue == "=")
        {
            CalculateResult();
        }
        else if (buttonValue == "C")
        {
            ClearInput();
        }
        /* === 🔵 BACK：倒退鍵處理 ========================== */
        else if (buttonValue == "BACK")
        {
            if (currentInput.Length > 0)
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
        }
        /* === 🔵 BACK END ===================================== */
        else
        {
            currentInput += buttonValue;
            UpdateDisplay();
        }
    }

    /* ======= 計算結果方法 ======= */
    public void CalculateResult()
    {
        try
        {
            if (currentInput.Contains("π"))
            {
                int piCount = 0;
                foreach (char c in currentInput) if (c == 'π') piCount++;

                string coeffExpr = currentInput.Replace("π", "");

                coeffExpr = coeffExpr.Trim();
                while (coeffExpr.StartsWith("*") || coeffExpr.StartsWith("/"))
                    coeffExpr = coeffExpr[1..];
                while (coeffExpr.EndsWith("*") || coeffExpr.EndsWith("/"))
                    coeffExpr = coeffExpr[..^1];

                if (string.IsNullOrWhiteSpace(coeffExpr))
                    coeffExpr = "1";

                double coeff;
                try
                {
                    coeff = Convert.ToDouble(new DataTable().Compute(coeffExpr, ""));
                }
                catch
                {
                    coeff = 1;
                }

                bool hasAddSub = coeffExpr.Contains("+") || coeffExpr.Contains("-");
                int exponent = hasAddSub ? 1 : piCount;

                if (Math.Abs(coeff) < 1e-10)
                {
                    currentInput = "0";
                }
                else
                {
                    string coeffStr =
                        Math.Abs(coeff - 1) < 1e-10 ? "" :
                        Math.Abs(coeff + 1) < 1e-10 ? "-" :
                        coeff.ToString("G10");

                    string piStr = exponent == 1 ? "π" : $"π^{exponent}";
                    currentInput = coeffStr + piStr;
                }

                UpdateDisplay();
                return;
            }

            result = Convert.ToDouble(new DataTable().Compute(currentInput, ""));
            currentInput = result.ToString();
            UpdateDisplay();
        }
        catch
        {
            currentInput = "Error";
            PlayErrorSound();
            UpdateDisplay();
        }
    }

    private void ClearInput()
    {
        currentInput = "";
        result = 0.0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            // 如果輸入為空，顯示0
            string displayValue = string.IsNullOrEmpty(currentInput) ? "0" : currentInput;
            displayText.text = displayValue;
            
            // 根據內容長度調整字體大小（可選）
            if (displayValue.Length > 15)
                displayText.fontSize = 1.5f;
            else if (displayValue.Length > 10)
                displayText.fontSize = 1.8f;
            else
                displayText.fontSize = 2.0f;
        }
    }
    
    // 音效播放方法
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    private void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
    }
    
    // 公共方法：設置顯示文本顏色
    public void SetDisplayColor(Color color)
    {
        if (displayText != null)
        {
            displayText.color = color;
        }
    }
    
    // 公共方法：設置字體大小
    public void SetFontSize(float size)
    {
        if (displayText != null)
        {
            displayText.fontSize = size;
        }
    }
}