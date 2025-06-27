using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AdvancedGeometryAnimation : MonoBehaviour
{
    [Header("要動畫的物件")]
    public Transform targetObject;

    [Header("動畫設定")]
    public float rotationSpeed = 50f;
    public float scaleSpeed = 1f;
    public float animationSpeed = 2f;
    public float amplitudeX = 3f;
    public float amplitudeY = 2f;
    public float amplitudeZ = 1f;

    [Header("軌跡設定")]
    public bool showTrail = true;
    public Material trailMaterial;
    public float trailWidth = 0.05f;
    public int maxTrailPoints = 200;
    public Color trailColor = Color.cyan;

    [Header("公式顯示")]
    public TextMeshProUGUI formulaText;  // 改為 TextMeshProUGUI
    public Canvas formulaCanvas;

    [Header("數學圖形類型")]
    public GeometryType geometryType = GeometryType.Lissajous;
    private GeometryType lastGeometryType;

    [Header("進階參數")]
    [Range(1f, 5f)]
    public float frequencyX = 1f;
    [Range(1f, 5f)]
    public float frequencyY = 2f;
    [Range(1f, 5f)]
    public float frequencyZ = 1f;
    [Range(0f, 6.28f)]
    public float phaseShift = 0f;

    public enum GeometryType
    {
        Simple,         // 原始的上下移動
        Lissajous,      // 利薩如圖形
        Helix,          // 螺旋
        Rose,           // 玫瑰曲線
        Butterfly,      // 蝴蝶曲線
        Figure8,        // 8字形
        Spiral,         // 螺旋線
        Torus           // 環面運動
    }

    private bool isAnimating = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private float time = 0f;

    // 軌跡相關
    private LineRenderer trailRenderer;
    private Queue<Vector3> trailPoints;
    private GameObject trailObject;

    // 公式字典
    private Dictionary<GeometryType, string> formulas = new Dictionary<GeometryType, string>
    {
        { GeometryType.Simple, "y = A·sin(ωt)" },
        { GeometryType.Lissajous, "x = A·sin(ωₓt + φ)\ny = B·sin(ωᵧt)\nz = C·sin(ωᵩt)" },
        { GeometryType.Helix, "x = r·cos(t)\ny = h·t\nz = r·sin(t)" },
        { GeometryType.Rose, "r = cos(k·θ)\nx = r·cos(θ)\nz = r·sin(θ)" },
        { GeometryType.Butterfly, "r = e^cos(θ) - 2cos(4θ) + sin^5(θ/12)" },
        { GeometryType.Figure8, "x = sin(t)\ny = sin(t)·cos(t)" },
        { GeometryType.Spiral, "r = a·θ\nx = r·cos(θ)\nz = r·sin(θ)" },
        { GeometryType.Torus, "x = (R+r·cos(v))·cos(u)\ny = r·sin(v)\nz = (R+r·cos(v))·sin(u)" }
    };

    void Start()
    {
        // 如果沒有指定 targetObject，就使用自己
        if (targetObject == null)
            targetObject = transform;

        if (targetObject != null)
        {
            originalPosition = targetObject.position;
            originalScale = targetObject.localScale;
        }

        SetupTrail();
        UpdateFormulaDisplay();
        lastGeometryType = geometryType;
    }

    void SetupTrail()
    {
        if (!showTrail) return;

        // 創建軌跡物件
        trailObject = new GameObject("Trail");
        trailObject.transform.SetParent(transform);

        // 設置 LineRenderer
        trailRenderer = trailObject.AddComponent<LineRenderer>();
        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }
        else
        {
            // 創建默認材質
            Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultMaterial.color = trailColor;
            trailRenderer.material = defaultMaterial;
        }

        // 設置漸層顏色
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(trailColor, 0.0f), new GradientColorKey(trailColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        trailRenderer.colorGradient = gradient;

        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = trailWidth * 0.1f;
        trailRenderer.positionCount = 0;
        trailRenderer.useWorldSpace = true;

        // 初始化軌跡點隊列
        trailPoints = new Queue<Vector3>();
    }

    void Update()
    {
        // 檢查幾何類型是否改變
        if (geometryType != lastGeometryType)
        {
            UpdateFormulaDisplay();
            ClearTrail();
            lastGeometryType = geometryType;
        }

        if (isAnimating && targetObject != null)
        {
            time += Time.deltaTime * animationSpeed;

            // 旋轉動畫
            targetObject.Rotate(0, rotationSpeed * Time.deltaTime, 0);

            // 縮放動畫
            float scale = 1 + Mathf.Sin(time * scaleSpeed) * 0.3f;
            targetObject.localScale = originalScale * scale;

            // 根據選擇的幾何類型計算位置
            Vector3 offset = CalculateGeometryOffset(time);
            Vector3 newPosition = originalPosition + offset;
            targetObject.position = newPosition;

            // 更新軌跡
            UpdateTrail(newPosition);
        }
    }

    void UpdateTrail(Vector3 currentPosition)
    {
        if (!showTrail || trailRenderer == null) return;

        // 添加當前位置到軌跡
        trailPoints.Enqueue(currentPosition);

        // 限制軌跡點數量
        while (trailPoints.Count > maxTrailPoints)
        {
            trailPoints.Dequeue();
        }

        // 更新 LineRenderer
        trailRenderer.positionCount = trailPoints.Count;
        int index = 0;
        foreach (Vector3 point in trailPoints)
        {
            trailRenderer.SetPosition(index, point);
            index++;
        }
    }

    void UpdateFormulaDisplay()
    {
        if (formulaText != null && formulas.ContainsKey(geometryType))
        {
            string formula = formulas[geometryType];
            string title = GetGeometryName(geometryType);
            formulaText.text = $"<b>{title}</b>\n\n{formula}";
        }
    }

    string GetGeometryName(GeometryType type)
    {
        switch (type)
        {
            case GeometryType.Simple: return "簡單正弦波";
            case GeometryType.Lissajous: return "利薩如曲線 (Lissajous)";
            case GeometryType.Helix: return "螺旋線 (Helix)";
            case GeometryType.Rose: return "玫瑰曲線 (Rose)";
            case GeometryType.Butterfly: return "蝴蝶曲線 (Butterfly)";
            case GeometryType.Figure8: return "8字曲線 (Figure-8)";
            case GeometryType.Spiral: return "阿基米德螺旋 (Spiral)";
            case GeometryType.Torus: return "環面曲線 (Torus)";
            default: return "未知曲線";
        }
    }

    Vector3 CalculateGeometryOffset(float t)
    {
        Vector3 offset = Vector3.zero;

        switch (geometryType)
        {
            case GeometryType.Simple:
                offset = new Vector3(0, Mathf.Sin(t) * amplitudeY, 0);
                break;

            case GeometryType.Lissajous:
                offset = new Vector3(
                    Mathf.Sin(t * frequencyX + phaseShift) * amplitudeX,
                    Mathf.Sin(t * frequencyY) * amplitudeY,
                    Mathf.Sin(t * frequencyZ) * amplitudeZ
                );
                break;

            case GeometryType.Helix:
                offset = new Vector3(
                    Mathf.Cos(t) * amplitudeX,
                    t * 0.5f % (amplitudeY * 2) - amplitudeY,
                    Mathf.Sin(t) * amplitudeZ
                );
                break;

            case GeometryType.Rose:
                float r = Mathf.Cos(frequencyX * t) * amplitudeX;
                offset = new Vector3(
                    r * Mathf.Cos(t),
                    Mathf.Sin(t * frequencyY) * amplitudeY,
                    r * Mathf.Sin(t)
                );
                break;

            case GeometryType.Butterfly:
                float butterflyR = Mathf.Exp(Mathf.Cos(t)) - 2 * Mathf.Cos(4 * t) + Mathf.Pow(Mathf.Sin(t / 12), 5);
                offset = new Vector3(
                    butterflyR * Mathf.Cos(t) * amplitudeX * 0.3f,
                    Mathf.Sin(t * 2) * amplitudeY,
                    butterflyR * Mathf.Sin(t) * amplitudeZ * 0.3f
                );
                break;

            case GeometryType.Figure8:
                float figure8Scale = amplitudeX;
                offset = new Vector3(
                    figure8Scale * Mathf.Sin(t),
                    figure8Scale * Mathf.Sin(t) * Mathf.Cos(t) + Mathf.Sin(t * frequencyY) * amplitudeY * 0.3f,
                    Mathf.Sin(t * 0.5f) * amplitudeZ
                );
                break;

            case GeometryType.Spiral:
                float spiralR = t * 0.5f;
                offset = new Vector3(
                    spiralR * Mathf.Cos(t) * amplitudeX * 0.3f,
                    Mathf.Sin(t * frequencyY) * amplitudeY,
                    spiralR * Mathf.Sin(t) * amplitudeZ * 0.3f
                );
                break;

            case GeometryType.Torus:
                float majorRadius = amplitudeX * 0.7f;
                float minorRadius = amplitudeX * 0.3f;
                offset = new Vector3(
                    (majorRadius + minorRadius * Mathf.Cos(t * frequencyY)) * Mathf.Cos(t),
                    minorRadius * Mathf.Sin(t * frequencyY) + Mathf.Sin(t * 0.5f) * amplitudeY * 0.3f,
                    (majorRadius + minorRadius * Mathf.Cos(t * frequencyY)) * Mathf.Sin(t)
                );
                break;
        }

        return offset;
    }

    public void ToggleAnimation()
    {
        isAnimating = !isAnimating;
        if (!isAnimating)
        {
            // 重置到原始狀態
            targetObject.position = originalPosition;
            targetObject.localScale = originalScale;
            targetObject.rotation = Quaternion.identity;
            time = 0f;
            ClearTrail();
        }
    }

    public void ClearTrail()
    {
        if (trailPoints != null)
        {
            trailPoints.Clear();
            if (trailRenderer != null)
                trailRenderer.positionCount = 0;
        }
    }

    public void SetGeometryType(int typeIndex)
    {
        if (typeIndex >= 0 && typeIndex < System.Enum.GetValues(typeof(GeometryType)).Length)
        {
            geometryType = (GeometryType)typeIndex;
            UpdateFormulaDisplay();
            ClearTrail();
        }
    }

    public void ResetAnimation()
    {
        time = 0f;
        if (targetObject != null)
        {
            targetObject.position = originalPosition;
            targetObject.rotation = Quaternion.identity;
        }
        ClearTrail();
    }

    public void ToggleTrail()
    {
        showTrail = !showTrail;
        if (trailRenderer != null)
            trailRenderer.enabled = showTrail;
    }

    // 當在 Inspector 中改變幾何類型時自動更新公式
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateFormulaDisplay();
        }
    }
}