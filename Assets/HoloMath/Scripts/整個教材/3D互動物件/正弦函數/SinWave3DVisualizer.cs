using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using System.Collections.Generic;

public class SinWave3DVisualizer : MonoBehaviour
{
    [Header("3D Wave Settings")]
    public int resolution = 50;
    public float xRange = 6f;
    public float yRange = 6f;
    public float amplitude = 1f;
    public float frequency = 1f;
    public float phaseShift = 0f;
    public float verticalShift = 0f;
    public float overallScale = 0.2f;

    [Header("Parameter Ranges")]
    public float minFrequency = -3f;
    public float maxFrequency = 3f;
    public float minAmplitude = 0f;
    public float maxAmplitude = 2f;
    public float minPhaseShift = -Mathf.PI;
    public float maxPhaseShift = Mathf.PI;
    public float minVerticalShift = -1f;
    public float maxVerticalShift = 1f;

    [Header("Mesh Settings")]
    public bool showWireframe = false;
    public float thickness = 0.1f;
    public bool smoothNormals = true;

    [Header("MRTK Components")]
    public PinchSlider frequencySlider;
    public PinchSlider amplitudeSlider;
    public PinchSlider phaseShiftSlider;
    public PinchSlider verticalShiftSlider;
    public TextMeshPro formulaText;

    [Header("Mesh Components")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    [Header("Animation")]
    public bool enableAnimation = false;
    public float animationSpeed = 1f;
    private float timeOffset = 0f;

    [Header("Symmetry Visualization")]
    public bool showSymmetryPoints = true;
    public bool showSymmetryLines = true;
    public GameObject symmetryPointPrefab;
    public GameObject symmetryLinePrefab;
    public Transform symmetryPointsParent;
    public Transform symmetryLinesParent;

    [Header("Reference Lines")]
    public bool showVerticalShiftLine = true;
    public GameObject verticalShiftLinePrefab;
    public Transform verticalShiftLineParent;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        InitializeMesh();

        if (meshRenderer != null)
        {
            if (showWireframe)
            {
                meshRenderer.material.SetInt("_Cull", 0);
            }
        }

        // 初始化所有滑桿
        if (frequencySlider != null)
        {
            frequencySlider.OnValueUpdated.AddListener(OnFrequencyChanged);
            frequencySlider.SliderValue = 0.5f;
        }

        if (amplitudeSlider != null)
        {
            amplitudeSlider.OnValueUpdated.AddListener(OnAmplitudeChanged);
            amplitudeSlider.SliderValue = 0.5f;
        }

        if (phaseShiftSlider != null)
        {
            phaseShiftSlider.OnValueUpdated.AddListener(OnPhaseShiftChanged);
            phaseShiftSlider.SliderValue = 0.5f;
        }

        if (verticalShiftSlider != null)
        {
            verticalShiftSlider.OnValueUpdated.AddListener(OnVerticalShiftChanged);
            verticalShiftSlider.SliderValue = 0.5f;
        }

        GenerateWave();
        UpdateFormulaDisplay();
        UpdateSymmetryVisuals();
        UpdateVerticalShiftLine();
    }

    void InitializeMesh()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        int vertexCount = (resolution + 1) * (resolution + 1) * 2;
        vertices = new Vector3[vertexCount];

        int triangleCount = resolution * resolution * 6 * 2;
        triangles = new int[triangleCount];

        GenerateBasicGrid();
    }

    void GenerateBasicGrid()
    {
        int vertexIndex = 0;

        // 生成上面的頂點
        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float xPos = (float)x / resolution * xRange - xRange / 2;
                float zPos = (float)z / resolution * yRange - yRange / 2;
                vertices[vertexIndex] = new Vector3(xPos * overallScale, thickness / 2, zPos * overallScale);
                vertexIndex++;
            }
        }

        // 生成下面的頂點
        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float xPos = (float)x / resolution * xRange - xRange / 2;
                float zPos = (float)z / resolution * yRange - yRange / 2;
                vertices[vertexIndex] = new Vector3(xPos * overallScale, -thickness / 2, zPos * overallScale);
                vertexIndex++;
            }
        }

        // 生成三角形
        int triangleIndex = 0;
        int vertPerRow = resolution + 1;

        // 上面的三角形
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * vertPerRow + x;

                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + vertPerRow;
                triangles[triangleIndex + 2] = i + 1;

                triangles[triangleIndex + 3] = i + 1;
                triangles[triangleIndex + 4] = i + vertPerRow;
                triangles[triangleIndex + 5] = i + vertPerRow + 1;

                triangleIndex += 6;
            }
        }

        // 下面的三角形
        int bottomOffset = (resolution + 1) * (resolution + 1);
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * vertPerRow + x + bottomOffset;

                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + vertPerRow;

                triangles[triangleIndex + 3] = i + 1;
                triangles[triangleIndex + 4] = i + vertPerRow + 1;
                triangles[triangleIndex + 5] = i + vertPerRow;

                triangleIndex += 6;
            }
        }
    }

    void GenerateWave()
    {
        int vertPerRow = resolution + 1;
        int topVertCount = vertPerRow * vertPerRow;

        // 更新上面的頂點
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float xPos = vertices[i].x / overallScale;
                float zPos = vertices[i].z / overallScale;

                float height = CalculateWaveHeight(xPos, zPos) * overallScale;

                vertices[i].y = height + thickness / 2;
            }
        }

        // 更新下面的頂點
        for (int i = topVertCount, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float xPos = vertices[i].x / overallScale;
                float zPos = vertices[i].z / overallScale;

                float height = CalculateWaveHeight(xPos, zPos) * overallScale;

                vertices[i].y = height - thickness / 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        if (smoothNormals)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            CalculateFlatNormals();
        }

        mesh.RecalculateBounds();
    }

    void CalculateFlatNormals()
    {
        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            normals[i1] = normal;
            normals[i2] = normal;
            normals[i3] = normal;
        }

        mesh.normals = normals;
    }

    float CalculateWaveHeight(float x, float z)
    {
        if (Mathf.Abs(frequency) < 0.001f)
        {
            return verticalShift;
        }

        // y = A·sin(Bx - C) + D
        return amplitude * Mathf.Sin(frequency * x - phaseShift) + verticalShift;
    }

    // 滑桿事件處理方法
    public void OnFrequencyChanged(SliderEventData eventData)
    {
        frequency = Mathf.Lerp(minFrequency, maxFrequency, eventData.NewValue);

        if (Mathf.Abs(frequency) < 0.001f)
        {
            frequency = 0f;
        }

        UpdateAll();
    }

    public void OnAmplitudeChanged(SliderEventData eventData)
    {
        amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, eventData.NewValue);
        UpdateAll();
    }

    public void OnPhaseShiftChanged(SliderEventData eventData)
    {
        phaseShift = Mathf.Lerp(minPhaseShift, maxPhaseShift, eventData.NewValue);
        UpdateAll();
    }

    public void OnVerticalShiftChanged(SliderEventData eventData)
    {
        verticalShift = Mathf.Lerp(minVerticalShift, maxVerticalShift, eventData.NewValue);
        UpdateAll();
    }

    void UpdateAll()
    {
        UpdateFormulaDisplay();
        GenerateWave();
        UpdateSymmetryVisuals();
        UpdateVerticalShiftLine();
    }

    void UpdateFormulaDisplay()
    {
        if (formulaText != null)
        {
            string formula = GenerateFormulaString();
            formulaText.text = formula;
        }
    }

    string GenerateFormulaString()
    {
        string ampStr = amplitude.ToString("F2");
        string freqStr = frequency.ToString("F2");
        string phaseStr = phaseShift.ToString("F2");
        string vertStr = verticalShift.ToString("F2");

        // 處理特殊情況
        if (Mathf.Approximately(amplitude, 0f))
        {
            if (Mathf.Approximately(verticalShift, 0f))
            {
                return "y = 0";
            }
            else
            {
                return $"y = {vertStr}";
            }
        }

        if (Mathf.Approximately(frequency, 0f))
        {
            float constantValue = amplitude + verticalShift;
            return $"y = {constantValue.ToString("F2")}";
        }

        // 建構基本的 sin 函數部分
        string sinPart = "sin(";

        // 頻率部分
        if (Mathf.Approximately(frequency, 1.0f))
        {
            sinPart += "x";
        }
        else
        {
            sinPart += $"{freqStr}x";
        }

        // 相位偏移部分
        if (!Mathf.Approximately(phaseShift, 0f))
        {
            if (phaseShift > 0)
            {
                sinPart += $" - {phaseStr}";
            }
            else
            {
                sinPart += $" + {Mathf.Abs(phaseShift).ToString("F2")}";
            }
        }

        sinPart += ")";

        // 振幅部分
        string result = "";
        if (Mathf.Approximately(amplitude, 1.0f))
        {
            result = sinPart;
        }
        else
        {
            result = $"{ampStr}{sinPart}";
        }

        // 垂直偏移部分
        if (!Mathf.Approximately(verticalShift, 0f))
        {
            if (verticalShift > 0)
            {
                result += $" + {vertStr}";
            }
            else
            {
                result += $" - {Mathf.Abs(verticalShift).ToString("F2")}";
            }
        }

        return $"y = {result}";
    }

    void Update()
    {
        if (enableAnimation)
        {
            timeOffset += Time.deltaTime * animationSpeed;
            GenerateAnimatedWave();
        }
    }

    void GenerateAnimatedWave()
    {
        int vertPerRow = resolution + 1;
        int topVertCount = vertPerRow * vertPerRow;

        // 更新上面的頂點
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float xPos = vertices[i].x / overallScale;
                float zPos = vertices[i].z / overallScale;

                float height = CalculateWaveHeight(xPos, zPos) * overallScale;

                vertices[i].y = height + thickness / 2;
            }
        }

        // 更新下面的頂點
        for (int i = topVertCount, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float xPos = vertices[i].x / overallScale;
                float zPos = vertices[i].z / overallScale;

                float height = CalculateWaveHeight(xPos, zPos) * overallScale;

                vertices[i].y = height - thickness / 2;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    #region Symmetry Visualization - 修正旋轉問題

    private List<GameObject> symmetryPoints = new List<GameObject>();
    private List<GameObject> symmetryLines = new List<GameObject>();
    private GameObject verticalShiftLine;

    void UpdateSymmetryVisuals()
    {
        UpdateSymmetryPoints();
        UpdateSymmetryLines();
    }

    void UpdateSymmetryPoints()
    {
        if (symmetryPointPrefab == null) return;

        // 如果沒有指派 Parent，自動創建一個
        if (symmetryPointsParent == null)
        {
            GameObject parentObj = new GameObject("SymmetryPointsParent");
            parentObj.transform.SetParent(transform);
            symmetryPointsParent = parentObj.transform;
        }

        // 如果頻率接近 0 或不顯示對稱點，隱藏所有現有的點
        if (!showSymmetryPoints || Mathf.Abs(frequency) < 0.01f)
        {
            foreach (GameObject point in symmetryPoints)
            {
                if (point != null)
                {
                    point.SetActive(false);
                }
            }
            return;
        }

        // sin(Bx - C) 的零點在 Bx - C = nπ，即 x = (nπ + C) / B
        float zeroSpacing = Mathf.PI / Mathf.Abs(frequency);
        float zeroOffset = phaseShift / frequency;
        float maxRange = xRange / 2f;

        // 計算需要多少個零點
        int maxPoints = Mathf.CeilToInt(maxRange / zeroSpacing) + 2;

        List<float> requiredPositions = new List<float>();

        // 計算所有需要的位置
        for (int n = -maxPoints; n <= maxPoints; n++)
        {
            float xPos = (n * Mathf.PI + phaseShift) / frequency;
            if (Mathf.Abs(xPos) <= maxRange)
            {
                requiredPositions.Add(xPos);
            }
        }

        // 確保有足夠的點物件
        while (symmetryPoints.Count < requiredPositions.Count)
        {
            GameObject newPoint = Instantiate(symmetryPointPrefab, symmetryPointsParent);
            newPoint.transform.localScale = Vector3.one * 0.1f;
            symmetryPoints.Add(newPoint);
        }

        // 更新點的位置和可見性
        for (int i = 0; i < symmetryPoints.Count; i++)
        {
            if (symmetryPoints[i] != null)
            {
                if (i < requiredPositions.Count)
                {
                    // 使用本地座標系統 - 這是關鍵修正
                    Vector3 localPos = new Vector3(
                        requiredPositions[i] * overallScale,
                        verticalShift * overallScale,
                        0
                    );
                    symmetryPoints[i].transform.localPosition = localPos;
                    symmetryPoints[i].SetActive(true);
                }
                else
                {
                    // 隱藏多餘的點
                    symmetryPoints[i].SetActive(false);
                }
            }
        }
    }

    void UpdateSymmetryLines()
    {
        if (symmetryLinePrefab == null) return;

        // 如果沒有指派 Parent，自動創建一個
        if (symmetryLinesParent == null)
        {
            GameObject parentObj = new GameObject("SymmetryLinesParent");
            parentObj.transform.SetParent(transform);
            symmetryLinesParent = parentObj.transform;
        }

        // 如果頻率接近 0 或不顯示對稱線，隱藏所有現有的線
        if (!showSymmetryLines || Mathf.Abs(frequency) < 0.01f)
        {
            foreach (GameObject line in symmetryLines)
            {
                if (line != null)
                {
                    line.SetActive(false);
                }
            }
            return;
        }

        // sin(Bx - C) 的極值點在 Bx - C = π/2 + nπ，即 x = (π/2 + nπ + C) / B
        float extremeSpacing = Mathf.PI / Mathf.Abs(frequency);
        float firstExtreme = (Mathf.PI / 2f + phaseShift) / frequency;
        float maxRange = xRange / 2f;

        // 計算需要多少條對稱線
        int maxLines = Mathf.CeilToInt(maxRange / extremeSpacing) + 2;

        List<float> requiredPositions = new List<float>();

        // 計算所有需要的位置
        for (int n = -maxLines; n <= maxLines; n++)
        {
            float xPos = (Mathf.PI / 2f + n * Mathf.PI + phaseShift) / frequency;

            if (Mathf.Abs(xPos) <= maxRange)
            {
                requiredPositions.Add(xPos);
            }
        }

        // 確保有足夠的線物件
        while (symmetryLines.Count < requiredPositions.Count)
        {
            GameObject newLine = Instantiate(symmetryLinePrefab, symmetryLinesParent);
            symmetryLines.Add(newLine);
        }

        // 更新線的位置和可見性
        for (int i = 0; i < symmetryLines.Count; i++)
        {
            if (symmetryLines[i] != null)
            {
                if (i < requiredPositions.Count)
                {
                    // 使用本地座標系統 - 這是關鍵修正
                    Vector3 localPos = new Vector3(
                        requiredPositions[i] * overallScale,
                        verticalShift * overallScale,
                        0
                    );
                    symmetryLines[i].transform.localPosition = localPos;

                    // 更新縮放 - 線的高度根據振幅調整
                    symmetryLines[i].transform.localScale = new Vector3(
                        0.01f, // 線的寬度
                        amplitude * overallScale * 2.5f, // 線的高度
                        0.01f  // 線的深度
                    );

                    symmetryLines[i].SetActive(true);
                }
                else
                {
                    // 隱藏多餘的線
                    symmetryLines[i].SetActive(false);
                }
            }
        }
    }

    void UpdateVerticalShiftLine()
    {
        if (!showVerticalShiftLine || verticalShiftLinePrefab == null) return;

        // 如果沒有指派 Parent，自動創建一個
        if (verticalShiftLineParent == null)
        {
            GameObject parentObj = new GameObject("VerticalShiftLineParent");
            parentObj.transform.SetParent(transform);
            verticalShiftLineParent = parentObj.transform;
        }

        // 如果垂直偏移接近 0，隱藏線
        if (Mathf.Abs(verticalShift) < 0.01f)
        {
            if (verticalShiftLine != null)
            {
                verticalShiftLine.SetActive(false);
            }
            return;
        }

        // 創建或更新垂直偏移線
        if (verticalShiftLine == null)
        {
            verticalShiftLine = Instantiate(verticalShiftLinePrefab, verticalShiftLineParent);
        }

        // 使用本地座標系統 - 這是關鍵修正
        Vector3 localPos = new Vector3(0, verticalShift * overallScale, 0);
        verticalShiftLine.transform.localPosition = localPos;

        // 設置為水平線
        verticalShiftLine.transform.localScale = new Vector3(
            xRange * overallScale, // 線的長度
            0.01f, // 線的高度
            0.01f  // 線的深度
        );

        verticalShiftLine.SetActive(true);
    }

    // 清理方法，在物件被銷毀時調用
    void OnDestroy()
    {
        // 清理對稱點
        foreach (GameObject point in symmetryPoints)
        {
            if (point != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(point);
                }
                else
                {
                    DestroyImmediate(point);
                }
            }
        }
        symmetryPoints.Clear();

        // 清理對稱線
        foreach (GameObject line in symmetryLines)
        {
            if (line != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(line);
                }
                else
                {
                    DestroyImmediate(line);
                }
            }
        }
        symmetryLines.Clear();

        // 清理垂直偏移線
        if (verticalShiftLine != null)
        {
            if (Application.isPlaying)
            {
                Destroy(verticalShiftLine);
            }
            else
            {
                DestroyImmediate(verticalShiftLine);
            }
        }
    }

    #endregion

    // 在 Inspector 中測試用的函數
    [ContextMenu("Test Update Symmetry")]
    void TestUpdateSymmetry()
    {
        UpdateSymmetryVisuals();
        UpdateVerticalShiftLine();
    }
}