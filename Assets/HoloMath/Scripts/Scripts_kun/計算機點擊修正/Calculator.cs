using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Data;

public class Calculator : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    private string currentInput = "";
    private double result = 0.0;

    public void OnButtonClick(string buttonValue)
    {
        if (buttonValue == "=")
        {
            CalculateResult();
        }
        else if (buttonValue == "C")
        {
            ClearInput();
        }
        /* === 🔵 BACK：新增倒退鍵處理 ========================== */
        else if (buttonValue == "BACK")
        {
            if (currentInput.Length > 0)
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
        }
        /* === 🔵 BACK  END ===================================== */
        else
        {
            currentInput += buttonValue;
            UpdateDisplay();
        }
    }

    /* ======= 以下 CalculateResult() ~ UpdateDisplay() 原封不動 ======= */
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
        displayText.text = currentInput;
    }
}

