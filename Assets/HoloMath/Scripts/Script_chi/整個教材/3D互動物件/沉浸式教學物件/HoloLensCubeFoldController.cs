using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;

public class HoloLensCubeFoldController : MonoBehaviour,
IMixedRealityPointerHandler
{
    [Header("Cube Faces")]
    public Transform faceA_Center; // A面（中心參考面）[A,B,C,D]
    public Transform faceB_Right;  // B面（右面）[C,B,G,F]
    public Transform faceE_Left;   // E面（左面）[H,D,E,A]
    public Transform faceH_Top;    // H面（頂面）[H,D,C,G]
    public Transform faceF_Bottom; // F面（底面）[A,B,E,F]
    public Transform faceG_Far;    // G面（遠面）[G,H,E,F]

    [Header("Vertices with Buttons (按順序: A,B,C,D,E,F,G,H)")]
    public Transform[] vertices = new Transform[8];
   
    [Header("Vertex Button Prefab")]
    public GameObject vertexButtonPrefab; // PressableButtonHoloLens2預製物件
   
    [Header("Interactive Line Drawing")]
    public Material interactiveLineMaterial; // 互動線段材質
    public float interactiveLineWidth = 0.03f;
    public Color selectedVertexColor = Color.yellow;
    public Color normalVertexColor = Color.white;
   
    [Header("Line Counting Display")]
    public TextMeshPro lineCountText3D; // 顯示線段數量的3D文字

    [Header("Line Renderers for Question Segments")]
    public LineRenderer lineCH;  // CH線段 - 歪斜
    public LineRenderer lineAF;  // AF線段 - 歪斜  
    public LineRenderer lineDE;  // DE線段 - 歪斜
    public LineRenderer lineCF;  // CF線段 - 歪斜

    [Header("3D UI Elements")]
    public GameObject foldButton3D;     // 3D按鈕
    public GameObject unfoldButton3D;   // 3D按鈕
    public TextMeshPro statusText3D;    // 3D文字
    public TextMeshPro mathAnswerText3D; // 數學答案3D文字

    // ===== 新增：縮放功能按鈕 =====
    [Header("Scale Control Buttons")]
    public GameObject scaleUpButton3D;    // 放大5倍按鈕
    public GameObject scaleNormalButton3D; // 恢復正常大小按鈕

    [Header("Animation Settings")]
    public float foldDuration = 3f;
    public AnimationCurve foldCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visualization Settings")]
    public Material parallelLineMaterial;  // 平行線材質（綠色）
    public Material skewLineMaterial;      // 歪斜線材質（紅色）
    public Material vertexMaterial;        // 頂點材質
    public Material faceMaterial;          // 面材質

    [Header("Coordinate System")]
    public Transform coordinateSystemRoot; // 座標系統根節點
   
    [Header("Vertex Position Control - Complete Fix")]
    [SerializeField] private Vector3 targetALocalPosition = new Vector3(-0.5f, 4.5f, 0f);
    [SerializeField] private bool autoUpdateInEditor = true;
    [SerializeField] private bool debugCoordinateTransform = true;

    // ===== 新增：縮放相關變數 =====
    [Header("Scale Settings")]
    [SerializeField] private float scaleMultiplier = 5f; // 縮放倍數
    [SerializeField] private bool isScaled = false; // 當前是否已放大
    [SerializeField] private bool debugScaling = true;

    // 儲存原始尺寸資料
    private ScaleData[] originalFaceScales;
    private Vector3[] originalFacePositions;
    private float[] originalLineWidths;
    private Vector3[] originalVertexScales;

    // 儲存當前的基準位置（動態更新）
    private Vector3 currentBasePosition = new Vector3(-0.5f, 4.5f, 0f);
   
    // 儲存原始相對位置關係（以A點為基準）
    private Vector3[] originalRelativePositions;
    private bool hasInitializedRelativePositions = false;
   
    // Vertices容器的引用
    private Transform verticesContainer;
    private bool isUnfolded = true;
    private bool isAnimating = false;

    // 展開和折疊狀態的變換資料
    private TransformData[] unfoldedTransforms;
    private TransformData[] foldedTransforms;

    // ===== 新增：互動線段繪製相關變數 =====
    private Transform selectedVertex1 = null;  // 第一個選中的頂點
    private Transform selectedVertex2 = null;  // 第二個選中的頂點
    private List<InteractiveLine> drawnLines = new List<InteractiveLine>(); // 已繪製的線段列表
    private Dictionary<Transform, GameObject> vertexButtons = new Dictionary<Transform, GameObject>(); // 頂點按鈕對應表
    private Dictionary<Transform, Renderer> vertexRenderers = new Dictionary<Transform, Renderer>(); // 頂點渲染器對應表

    // ===== 新增：縮放資料結構 =====
    [System.Serializable]
    public struct ScaleData
    {
        public Vector3 localPosition;
        public Vector3 localScale;
        
        public ScaleData(Transform t)
        {
            localPosition = t.localPosition;
            localScale = t.localScale;
        }
    }

    // 互動線段資料結構
    [System.Serializable]
    public class InteractiveLine
    {
        public Transform vertex1;
        public Transform vertex2;
        public LineRenderer lineRenderer;
        public string lineName;

        public InteractiveLine(Transform v1, Transform v2, LineRenderer lr)
        {
            vertex1 = v1;
            vertex2 = v2;
            lineRenderer = lr;
            lineName = GetVertexName(v1) + GetVertexName(v2);
        }

        private string GetVertexName(Transform vertex)
        {
            // 根據頂點在陣列中的索引返回名稱 (A, B, C, D, E, F, G, H)
            for (int i = 0; i < 8; i++)
            {
                var controller = vertex.GetComponentInParent<HoloLensCubeFoldController>();
                if (controller != null && controller.vertices[i] == vertex)
                {
                    return ((char)('A' + i)).ToString();
                }
            }
            return "?";
        }
    }

    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Transform t)
        {
            position = t.localPosition;
            rotation = t.localRotation;
            scale = t.localScale;
        }
    }

    void Start()
    {
        if (coordinateSystemRoot == null)
            coordinateSystemRoot = this.transform;

        // 初始化Vertices容器和相對位置關係
        InitializeVerticesContainer();
        InitializeRelativePositions();
       
        // ===== 新增：初始化縮放資料 =====
        InitializeScaleData();

        // ===== 新增：初始化頂點按鈕 =====
        InitializeVertexButtons();
        InitializeTransforms();
        SetupLineRenderers();
        Setup3DButtons();

        // ===== 新增：設置縮放按鈕 =====
        SetupScaleButtons();

        UpdateStatusDisplay();
        UpdateMathDisplay();
       
        // ===== 新增：更新線段計數顯示 =====
        UpdateLineCountDisplay();
        DebugCoordinateSystem();
        DebugVerticesInfo();
    }

    // ===== 新增：初始化縮放資料 =====
    void InitializeScaleData()
    {
        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };
        
        // 儲存原始面的位置和縮放
        originalFacePositions = new Vector3[faces.Length];
        originalFaceScales = new ScaleData[faces.Length];
        
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                originalFacePositions[i] = faces[i].localPosition;
                originalFaceScales[i] = new ScaleData(faces[i]);
            }
        }

        // 儲存原始頂點縮放
        originalVertexScales = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] != null)
            {
                originalVertexScales[i] = vertices[i].localScale;
            }
        }

        // 儲存原始線段寬度
        LineRenderer[] lines = { lineCH, lineAF, lineDE, lineCF };
        originalLineWidths = new float[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
            {
                originalLineWidths[i] = lines[i].startWidth;
            }
        }

        if (debugScaling)
        {
            Debug.Log("=== 初始化縮放資料完成 ===");
            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i] != null)
                {
                    Debug.Log($"Face {i} ({faces[i].name}): 原始位置={originalFacePositions[i]}, 原始縮放={originalFaceScales[i].localScale}");
                }
            }
        }
    }

    // ===== 新增：設置縮放按鈕 =====
    void SetupScaleButtons()
    {
        SetupButton(scaleUpButton3D, () => ScaleCube(true));
        SetupButton(scaleNormalButton3D, () => ScaleCube(false));
    }

    // ===== 新增：縮放立方體功能 =====
    public void ScaleCube(bool scaleUp)
    {
        if (isAnimating) return;
        if (scaleUp && isScaled) return; // 已經放大了
        if (!scaleUp && !isScaled) return; // 已經是正常大小了

        StartCoroutine(ScaleAnimation(scaleUp));
    }

    // ===== 新增：縮放動畫 =====
    IEnumerator ScaleAnimation(bool scaleUp)
    {
        isAnimating = true;
        float elapsed = 0f;
        float animationDuration = 1f; // 縮放動畫時長

        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };

        // 記錄起始狀態
        Vector3[] startFacePositions = new Vector3[faces.Length];
        Vector3[] startFaceScales = new Vector3[faces.Length];
        Vector3[] startVertexPositions = new Vector3[vertices.Length];
        Vector3[] startVertexScales = new Vector3[vertices.Length];
        
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                startFacePositions[i] = faces[i].localPosition;
                startFaceScales[i] = faces[i].localScale;
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] != null)
            {
                startVertexPositions[i] = vertices[i].localPosition;
                startVertexScales[i] = vertices[i].localScale;
            }
        }

        // 計算目標狀態
        Vector3[] targetFacePositions = new Vector3[faces.Length];
        Vector3[] targetFaceScales = new Vector3[faces.Length];
        Vector3[] targetVertexPositions = new Vector3[vertices.Length];
        Vector3[] targetVertexScales = new Vector3[vertices.Length];

        if (scaleUp)
        {
            // 放大5倍
            CalculateScaledTransforms(targetFacePositions, targetFaceScales, targetVertexPositions, targetVertexScales, true);
        }
        else
        {
            // 恢復正常大小
            CalculateScaledTransforms(targetFacePositions, targetFaceScales, targetVertexPositions, targetVertexScales, false);
        }

        // 動畫插值
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = foldCurve.Evaluate(elapsed / animationDuration);

            // 插值面的變換
            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i] != null)
                {
                    faces[i].localPosition = Vector3.Lerp(startFacePositions[i], targetFacePositions[i], t);
                    faces[i].localScale = Vector3.Lerp(startFaceScales[i], targetFaceScales[i], t);
                }
            }

            // 插值頂點的變換
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i] != null)
                {
                    vertices[i].localPosition = Vector3.Lerp(startVertexPositions[i], targetVertexPositions[i], t);
                    vertices[i].localScale = Vector3.Lerp(startVertexScales[i], targetVertexScales[i], t);
                }
            }

            // 插值線段寬度
            UpdateLineWidthsForScale(t, scaleUp);

            UpdateLineRenderers();
            UpdateInteractiveLines();
            yield return null;
        }

        // 確保最終狀態準確
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                faces[i].localPosition = targetFacePositions[i];
                faces[i].localScale = targetFaceScales[i];
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] != null)
            {
                vertices[i].localPosition = targetVertexPositions[i];
                vertices[i].localScale = targetVertexScales[i];
            }
        }

        UpdateLineWidthsForScale(1f, scaleUp);
        
        isScaled = scaleUp;
        isAnimating = false;

        // 重新計算折疊變換資料（因為尺寸改變了）
        RecalculateFoldTransforms();

        UpdateStatusDisplay();
        UpdateLineRenderers();
        UpdateInteractiveLines();

        if (debugScaling)
        {
            Debug.Log($"縮放動畫完成！當前縮放狀態: {(isScaled ? "放大5倍" : "正常大小")}");
        }
    }

    // ===== 新增：計算縮放後的變換 =====
    void CalculateScaledTransforms(Vector3[] facePositions, Vector3[] faceScales, 
                                   Vector3[] vertexPositions, Vector3[] vertexScales, bool scaleUp)
    {
        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };
        float multiplier = scaleUp ? scaleMultiplier : 1f;

        // FaceA_Center 作為基準，位置不變，只改變縮放
        Vector3 baseFacePosition = originalFacePositions[0]; // FaceA_Center的原始位置

        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                if (i == 0) // FaceA_Center
                {
                    facePositions[i] = originalFacePositions[i]; // 位置不變
                    faceScales[i] = new Vector3(
                        originalFaceScales[i].localScale.x * multiplier,
                        originalFaceScales[i].localScale.y * multiplier,
                        originalFaceScales[i].localScale.z * multiplier
                    );
                }
                else
                {
                    // 其他面：位置相對於FaceA_Center按比例縮放
                    Vector3 offset = originalFacePositions[i] - baseFacePosition;
                    facePositions[i] = baseFacePosition + offset * multiplier;
                    faceScales[i] = new Vector3(
                        originalFaceScales[i].localScale.x * multiplier,
                        originalFaceScales[i].localScale.y * multiplier,
                        originalFaceScales[i].localScale.z * multiplier
                    );
                }

                if (debugScaling)
                {
                    Debug.Log($"Face {i} ({faces[i].name}): 目標位置={facePositions[i]}, 目標縮放={faceScales[i]}");
                }
            }
        }

        // 計算頂點的縮放變換
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] != null)
            {
                // 頂點位置需要相對於基準點按比例縮放
                // 這裡需要根據當前展開/折疊狀態來計算
                if (isUnfolded)
                {
                    Vector3 currentRelativePos = originalRelativePositions[i];
                    Vector3 scaledBasePos = scaleUp ? currentBasePosition * multiplier : currentBasePosition;
                    Vector3 scaledRelativePos = currentRelativePos * multiplier;
                    
                    if (verticesContainer != null)
                    {
                        Vector3 newLocalPos = scaledBasePos + scaledRelativePos;
                        vertexPositions[i] = verticesContainer.InverseTransformPoint(
                            verticesContainer.TransformPoint(newLocalPos));
                    }
                    else
                    {
                        vertexPositions[i] = scaledBasePos + scaledRelativePos;
                    }
                }
                else
                {
                    // 折疊狀態下，頂點位置會在折疊動畫中處理
                    vertexPositions[i] = vertices[i].localPosition;
                }

                vertexScales[i] = originalVertexScales[i] * multiplier;

                if (debugScaling)
                {
                    char vertexName = (char)('A' + i);
                    Debug.Log($"Vertex {vertexName}[{i}]: 目標位置={vertexPositions[i]}, 目標縮放={vertexScales[i]}");
                }
            }
        }
    }

    // ===== 新增：更新線段寬度 =====
    void UpdateLineWidthsForScale(float t, bool scaleUp)
    {
        LineRenderer[] lines = { lineCH, lineAF, lineDE, lineCF };
        float targetMultiplier = scaleUp ? scaleMultiplier : 1f;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null && i < originalLineWidths.Length)
            {
                float startWidth = scaleUp ? originalLineWidths[i] : originalLineWidths[i] * scaleMultiplier;
                float endWidth = originalLineWidths[i] * targetMultiplier;
                float currentWidth = Mathf.Lerp(startWidth, endWidth, t);
                
                lines[i].startWidth = currentWidth;
                lines[i].endWidth = currentWidth;
            }
        }

        // 更新互動線段寬度
        float interactiveTargetWidth = scaleUp ? interactiveLineWidth * scaleMultiplier : interactiveLineWidth;
        float interactiveStartWidth = scaleUp ? interactiveLineWidth : interactiveLineWidth * scaleMultiplier;
        float currentInteractiveWidth = Mathf.Lerp(interactiveStartWidth, interactiveTargetWidth, t);
        
        foreach (var line in drawnLines)
        {
            if (line.lineRenderer != null)
            {
                line.lineRenderer.startWidth = currentInteractiveWidth;
                line.lineRenderer.endWidth = currentInteractiveWidth;
            }
        }
    }

    // ===== 新增：重新計算折疊變換（考慮縮放） =====
    void RecalculateFoldTransforms()
    {
        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };
        float currentMultiplier = isScaled ? scaleMultiplier : 1f;

        // 重新儲存當前展開狀態
        unfoldedTransforms = new TransformData[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                unfoldedTransforms[i] = new TransformData(faces[i]);
            }
        }

        // 重新計算折疊狀態的目標變換（考慮縮放）
        foldedTransforms = new TransformData[faces.Length];
        float faceSize = 0.5f * currentMultiplier; // 立方體邊長（考慮縮放）

        // A面（中心面）- 作為基準面
        foldedTransforms[0] = new TransformData(faces[0]);
        foldedTransforms[0].position = unfoldedTransforms[0].position - Vector3.forward * faceSize;
        foldedTransforms[0].rotation = unfoldedTransforms[0].rotation;

        // B面（右面）：從右側折疊90度成為右側面
        foldedTransforms[1] = new TransformData(faces[1]);
        foldedTransforms[1].position = unfoldedTransforms[0].position + Vector3.right * faceSize;
        foldedTransforms[1].rotation = Quaternion.Euler(0, 90, 0) * unfoldedTransforms[0].rotation;

        // E面（左面）：從左側折疊90度成為左側面
        foldedTransforms[2] = new TransformData(faces[2]);
        foldedTransforms[2].position = unfoldedTransforms[0].position + Vector3.left * faceSize;
        foldedTransforms[2].rotation = Quaternion.Euler(0, -90, 0) * unfoldedTransforms[0].rotation;

        // H面（頂面）：從上方折疊90度成為頂面
        foldedTransforms[3] = new TransformData(faces[3]);
        foldedTransforms[3].position = unfoldedTransforms[0].position + Vector3.up * faceSize;
        foldedTransforms[3].rotation = Quaternion.Euler(-90, 0, 0) * unfoldedTransforms[0].rotation;

        // F面（底面）：從下方折疊90度成為底面
        foldedTransforms[4] = new TransformData(faces[4]);
        foldedTransforms[4].position = unfoldedTransforms[0].position + Vector3.down * faceSize;
        foldedTransforms[4].rotation = Quaternion.Euler(90, 0, 0) * unfoldedTransforms[0].rotation;

        // G面（遠面）：需要兩次折疊，先向右到B面位置，再向後90度
        foldedTransforms[5] = new TransformData(faces[5]);
        foldedTransforms[5].position = unfoldedTransforms[0].position + Vector3.forward * faceSize;
        foldedTransforms[5].rotation = Quaternion.Euler(0, 0, 0) * unfoldedTransforms[0].rotation;

        if (debugScaling)
        {
            Debug.Log($"重新計算折疊變換完成，當前縮放倍數: {currentMultiplier}, 面尺寸: {faceSize}");
        }
    }

    // ===== 修改：獲取折疊狀態頂點位置方法（考慮縮放） =====
    Vector3[] GetFoldedVertexPositions()
    {
        float currentMultiplier = isScaled ? scaleMultiplier : 1f;
        
        // 關鍵修正：基於展開狀態的A點位置，計算正確的立方體中心
        Vector3 foldedALocal = currentBasePosition + new Vector3(0f, 0f, -0.5f * currentMultiplier);
        
        // 計算立方體中心：A點是立方體的左下前角，所以中心要向右上後各移動0.5
        Vector3 centerLocal = foldedALocal + new Vector3(0.5f * currentMultiplier, 0.5f * currentMultiplier, 0.5f * currentMultiplier);
        Vector3 cubeCenter = verticesContainer.TransformPoint(centerLocal);

        float halfEdge = 0.5f * currentMultiplier;

        Vector3[] foldedPositions = new Vector3[8];

        // 計算立方體的8個頂點（保持原有的頂點對應關係）
        foldedPositions[0] = cubeCenter + new Vector3(-halfEdge, -halfEdge, -halfEdge); // A：左下前
        foldedPositions[1] = cubeCenter + new Vector3(halfEdge, -halfEdge, -halfEdge);  // B：右下前
        foldedPositions[2] = cubeCenter + new Vector3(halfEdge, halfEdge, -halfEdge);   // C：右上前
        foldedPositions[3] = cubeCenter + new Vector3(-halfEdge, halfEdge, -halfEdge);  // D：左上前
        foldedPositions[4] = cubeCenter + new Vector3(-halfEdge, -halfEdge, halfEdge);  // E：左下後
        foldedPositions[5] = cubeCenter + new Vector3(halfEdge, -halfEdge, halfEdge);   // F：右下後
        foldedPositions[6] = cubeCenter + new Vector3(halfEdge, halfEdge, halfEdge);    // G：右上後
        foldedPositions[7] = cubeCenter + new Vector3(-halfEdge, halfEdge, halfEdge);   // H：左上後

        if (debugCoordinateTransform)
        {
            Debug.Log("=== 修正後的折疊狀態計算（考慮縮放） ===");
            Debug.Log($"當前縮放倍數: {currentMultiplier}");
            Debug.Log($"展開狀態A點基準位置: {currentBasePosition}");
            Debug.Log($"折疊後A點本地位置: {foldedALocal}");
            Debug.Log($"立方體中心本地位置: {centerLocal}");
            Debug.Log($"立方體中心世界位置: {cubeCenter}");
            Debug.Log($"立方體半邊長: {halfEdge}");
        }

        return foldedPositions;
    }

    // ===== 修改：獲取展開狀態頂點位置（考慮縮放） =====
    Vector3[] GetUnfoldedVertexPositions()
    {
        float currentMultiplier = isScaled ? scaleMultiplier : 1f;
        
        // 使用當前的基準位置和縮放倍數計算展開狀態
        Vector3[] unfoldedPositions = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 scaledRelativePos = originalRelativePositions[i] * currentMultiplier;
            Vector3 localPosition = currentBasePosition + scaledRelativePos;
            unfoldedPositions[i] = verticesContainer.TransformPoint(localPosition);
        }

        if (debugCoordinateTransform)
        {
            Debug.Log("=== 計算展開狀態頂點位置（考慮縮放） ===");
            Debug.Log($"當前縮放倍數: {currentMultiplier}");
            Debug.Log($"使用基準位置: {currentBasePosition}");
            for (int i = 0; i < 8; i++)
            {
                char vertexName = (char)('A' + i);
                Debug.Log($"Vertex {vertexName}[{i}] 展開位置: {unfoldedPositions[i]}");
            }
        }

        return unfoldedPositions;
    }

    // ===== 修改：更新狀態顯示（加入縮放資訊） =====
    void UpdateStatusDisplay()
    {
        if (statusText3D != null)
        {
            string state = isUnfolded ? "展開狀態" : "立方體狀態";
            string scaleState = isScaled ? " (放大5倍)" : " (正常大小)";
            statusText3D.text = state + scaleState;
        }
    }

    // ===== 新增：公開方法供按鈕調用 =====
    public void OnScaleUpButtonClicked()
    {
        ScaleCube(true);
    }

    public void OnScaleNormalButtonClicked()
    {
        ScaleCube(false);
    }

    // ===== 修改：設置頂點顏色（考慮縮放後的按鈕尺寸） =====
    void InitializeVertexButtons()
    {
        if (vertexButtonPrefab == null)
        {
            Debug.LogError("請指定 vertexButtonPrefab (PressableButtonHoloLens2 預製物件)！");
            return;
        }

        for (int i = 0; i < vertices.Length && i < 8; i++)
        {
            if (vertices[i] != null)
            {
                // 為每個頂點創建按鈕
                GameObject buttonObj = Instantiate(vertexButtonPrefab, vertices[i]);
                buttonObj.name = $"VertexButton_{(char)('A' + i)}";
               
                // 設置按鈕位置（稍微偏移避免重疊）
                buttonObj.transform.localPosition = Vector3.zero;
                buttonObj.transform.localScale = Vector3.one * 0.1f; // 縮小按鈕尺寸

                // 儲存按鈕引用
                vertexButtons[vertices[i]] = buttonObj;

                // 獲取按鈕組件並設置點擊事件
                var interactable = buttonObj.GetComponent<Interactable>();
                if (interactable != null)
                {
                    int vertexIndex = i; // 閉包變數
                    Transform vertex = vertices[i]; // 閉包變數
                    interactable.OnClick.AddListener(() => OnVertexButtonClicked(vertex, vertexIndex));
                }

                // 獲取頂點渲染器用於改變顏色
                Renderer vertexRenderer = vertices[i].GetComponent<Renderer>();
                if (vertexRenderer != null)
                {
                    vertexRenderers[vertices[i]] = vertexRenderer;
                    vertexRenderer.material.color = normalVertexColor;
                }

                Debug.Log($"為頂點 {(char)('A' + i)} 創建了按鈕");
            }
        }
    }

    // ===== 修改：設置互動線段渲染器（考慮縮放） =====
    void SetupInteractiveLineRenderer(LineRenderer line)
    {
        if (line != null)
        {
            line.material = interactiveLineMaterial != null ? interactiveLineMaterial : skewLineMaterial;
            float currentWidth = isScaled ? interactiveLineWidth * scaleMultiplier : interactiveLineWidth;
            line.startWidth = currentWidth;
            line.endWidth = currentWidth;
            line.positionCount = 2;
            line.useWorldSpace = true;
        }
    }

    // ===== 新增：縮放相關的Context Menu方法 =====
    [ContextMenu("Scale Up 5x")]
    public void TestScaleUp()
    {
        ScaleCube(true);
    }

    [ContextMenu("Scale Normal")]
    public void TestScaleNormal()
    {
        ScaleCube(false);
    }

    [ContextMenu("Debug Scale State")]
    public void DebugScaleState()
    {
        Debug.Log("=== 縮放狀態調試 ===");
        Debug.Log($"當前縮放狀態: {(isScaled ? "放大5倍" : "正常大小")}");
        Debug.Log($"縮放倍數: {scaleMultiplier}");
        Debug.Log($"是否展開: {isUnfolded}");
        Debug.Log($"是否動畫中: {isAnimating}");

        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                Debug.Log($"Face {i} ({faces[i].name}): 位置={faces[i].localPosition}, 縮放={faces[i].localScale}");
            }
        }
    }

    // ===== 繼續原有的方法實現 =====
    void OnVertexButtonClicked(Transform clickedVertex, int vertexIndex)
    {
        char vertexName = (char)('A' + vertexIndex);
        Debug.Log($"點擊了頂點 {vertexName}");

        if (selectedVertex1 == null)
        {
            // 選擇第一個頂點
            selectedVertex1 = clickedVertex;
            SetVertexColor(selectedVertex1, selectedVertexColor);
            Debug.Log($"選擇第一個頂點: {vertexName}");
        }
        else if (selectedVertex2 == null && clickedVertex != selectedVertex1)
        {
            // 選擇第二個頂點（不能與第一個相同）
            selectedVertex2 = clickedVertex;
            SetVertexColor(selectedVertex2, selectedVertexColor);
           
            // 兩個頂點都已選擇，開始繪製線段
            DrawLineBetweenVertices(selectedVertex1, selectedVertex2);
           
            Debug.Log($"選擇第二個頂點: {vertexName}，開始繪製線段");
        }
        else
        {
            // 重置選擇
            ResetVertexSelection();
           
            // 選擇新的第一個頂點
            selectedVertex1 = clickedVertex;
            SetVertexColor(selectedVertex1, selectedVertexColor);
            Debug.Log($"重置選擇，重新選擇第一個頂點: {vertexName}");
        }
    }

    // ===== 新增：為 Inspector 手動設定準備的公開方法 =====
    public void OnVertexA_Clicked() { OnVertexButtonClicked(vertices[0], 0); }
    public void OnVertexB_Clicked() { OnVertexButtonClicked(vertices[1], 1); }
    public void OnVertexC_Clicked() { OnVertexButtonClicked(vertices[2], 2); }
    public void OnVertexD_Clicked() { OnVertexButtonClicked(vertices[3], 3); }
    public void OnVertexE_Clicked() { OnVertexButtonClicked(vertices[4], 4); }
    public void OnVertexF_Clicked() { OnVertexButtonClicked(vertices[5], 5); }
    public void OnVertexG_Clicked() { OnVertexButtonClicked(vertices[6], 6); }
    public void OnVertexH_Clicked() { OnVertexButtonClicked(vertices[7], 7); }

    // ===== 新增：在兩個頂點之間繪製線段 =====
    void DrawLineBetweenVertices(Transform vertex1, Transform vertex2)
    {
        if (vertex1 == null || vertex2 == null) return;

        // 檢查是否已經存在相同的線段
        bool lineExists = drawnLines.Exists(line =>
            (line.vertex1 == vertex1 && line.vertex2 == vertex2) ||
            (line.vertex1 == vertex2 && line.vertex2 == vertex1));

        if (lineExists)
        {
            Debug.Log("這兩個頂點之間已經存在線段！");
            ResetVertexSelection();
            return;
        }

        // 創建新的LineRenderer
        GameObject lineObj = new GameObject($"InteractiveLine_{GetVertexName(vertex1)}{GetVertexName(vertex2)}");
        lineObj.transform.SetParent(this.transform);
       
        LineRenderer newLine = lineObj.AddComponent<LineRenderer>();
        SetupInteractiveLineRenderer(newLine);

        // 設置線段位置
        newLine.SetPosition(0, vertex1.position);
        newLine.SetPosition(1, vertex2.position);

        // 創建並儲存線段資料
        InteractiveLine interactiveLine = new InteractiveLine(vertex1, vertex2, newLine);
        drawnLines.Add(interactiveLine);

        Debug.Log($"成功繪製線段：{interactiveLine.lineName}");

        // 重置頂點選擇
        ResetVertexSelection();

        // 更新線段計數顯示
        UpdateLineCountDisplay();
    }

    // ===== 新增：設置頂點顏色 =====
    void SetVertexColor(Transform vertex, Color color)
    {
        if (vertexRenderers.ContainsKey(vertex) && vertexRenderers[vertex] != null)
        {
            vertexRenderers[vertex].material.color = color;
        }
    }

    // ===== 新增：重置頂點選擇 =====
    void ResetVertexSelection()
    {
        if (selectedVertex1 != null)
        {
            SetVertexColor(selectedVertex1, normalVertexColor);
            selectedVertex1 = null;
        }
       
        if (selectedVertex2 != null)
        {
            SetVertexColor(selectedVertex2, normalVertexColor);
            selectedVertex2 = null;
        }
    }

    // ===== 新增：獲取頂點名稱 =====
    string GetVertexName(Transform vertex)
    {
        for (int i = 0; i < vertices.Length && i < 8; i++)
        {
            if (vertices[i] == vertex)
            {
                return ((char)('A' + i)).ToString();
            }
        }
        return "Unknown";
    }

    // ===== 新增：更新線段計數顯示 =====
    void UpdateLineCountDisplay()
    {
        if (lineCountText3D != null)
        {
            lineCountText3D.text = $"已繪製線段數量: {drawnLines.Count}";
           
            if (drawnLines.Count > 0)
            {
                lineCountText3D.text += "\n線段列表:\n";
                foreach (var line in drawnLines)
                {
                    lineCountText3D.text += $"• {line.lineName}\n";
                }
            }
        }
    }

    // ===== 新增：清除所有已繪製的線段 =====
    [ContextMenu("Clear All Drawn Lines")]
    public void ClearAllDrawnLines()
    {
        foreach (var line in drawnLines)
        {
            if (line.lineRenderer != null)
            {
                DestroyImmediate(line.lineRenderer.gameObject);
            }
        }
       
        drawnLines.Clear();
        ResetVertexSelection();
        UpdateLineCountDisplay();
       
        Debug.Log("已清除所有已繪製的線段");
    }

    // ===== 新增：更新所有互動線段位置（在動畫過程中調用） =====
    void UpdateInteractiveLines()
    {
        foreach (var line in drawnLines)
        {
            if (line.lineRenderer != null && line.vertex1 != null && line.vertex2 != null)
            {
                line.lineRenderer.SetPosition(0, line.vertex1.position);
                line.lineRenderer.SetPosition(1, line.vertex2.position);
            }
        }
    }

    // 調試頂點信息
    void DebugVerticesInfo()
    {
        Debug.Log("=== 頂點結構調試信息 ===");
        if (verticesContainer != null)
        {
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
            Debug.Log($"Vertices容器本地位置: {verticesContainer.localPosition}");
        }

        for (int i = 0; i < vertices.Length && i < 8; i++)
        {
            if (vertices[i] != null)
            {
                char vertexName = (char)('A' + i);
                Debug.Log($"Vertex {vertexName}[{i}]:");
                Debug.Log($"  世界位置: {vertices[i].position}");
                if (verticesContainer != null)
                {
                    Debug.Log($"  相對於Vertices容器的本地位置: {verticesContainer.InverseTransformPoint(vertices[i].position)}");
                }
            }
        }
    }

    void DebugCoordinateSystem()
    {
        Debug.Log($"=== 座標系統調試信息 ===");
        Debug.Log($"CoordinateSystemRoot: {coordinateSystemRoot.name}");
        for (int i = 0; i < 8 && i < vertices.Length; i++)
        {
            char vertexName = (char)('A' + i);
            if (vertices[i] != null)
            {
                Debug.Log($"Vertex {vertexName}[{i}]: World {vertices[i].position}, Local {vertices[i].localPosition}");
            }
        }
    }

    // 初始化Vertices容器引用
    void InitializeVerticesContainer()
    {
        // 尋找Vertices容器
        verticesContainer = transform.Find("Vertices");
        if (verticesContainer == null)
        {
            // 如果在當前物件下找不到，嘗試在父物件中找
            Transform parent = transform.parent;
            if (parent != null)
            {
                verticesContainer = parent.Find("Vertices");
            }
        }

        if (verticesContainer == null)
        {
            Debug.LogError("找不到Vertices容器！請檢查物件結構。");
            return;
        }

        Debug.Log($"找到Vertices容器: {verticesContainer.name}, 世界位置: {verticesContainer.position}");
    }

    // 修正的初始化相對位置關係
    void InitializeRelativePositions()
    {
        if (vertices == null || vertices.Length < 8) return;
        if (verticesContainer == null) InitializeVerticesContainer();

        originalRelativePositions = new Vector3[8];

        // 使用預設的展開狀態座標來計算相對位置
        Vector3[] defaultUnfoldedPositions = {
            new Vector3(-0.5f, 4.5f, 0f),   // A[0]
            new Vector3(0.5f, 4.5f, 0f),    // B[1]
            new Vector3(0.5f, 5.5f, 0f),    // C[2]
            new Vector3(-0.5f, 5.5f, 0f),   // D[3]
            new Vector3(-1.5f, 4.5f, 0f),   // E[4]
            new Vector3(0.5f, 3.5f, 0f),    // F[5]
            new Vector3(1.5f, 5.5f, 0f),    // G[6]
            new Vector3(-0.5f, 6.5f, 0f)    // H[7]
        };

        // 計算相對於A點的位置差值
        Vector3 basePosition = defaultUnfoldedPositions[0]; // A點作為基準
        for (int i = 0; i < 8; i++)
        {
            originalRelativePositions[i] = defaultUnfoldedPositions[i] - basePosition;
        }

        hasInitializedRelativePositions = true;

        if (debugCoordinateTransform)
        {
            Debug.Log("=== 初始化相對位置關係（預設展開狀態） ===");
            Debug.Log($"基準點A的位置: {basePosition}");
            for (int i = 0; i < 8; i++)
            {
                char vertexName = (char)('A' + i);
                Debug.Log($"Vertex {vertexName}[{i}] 相對於A點的差值: {originalRelativePositions[i]}");
            }
        }
    }

    // 主要的設定A點本地位置方法
    public void SetVertexGroupLocalPosition(Vector3 newALocalPosition)
    {
        if (!hasInitializedRelativePositions)
        {
            InitializeRelativePositions();
        }
        if (vertices == null || vertices.Length < 8 || verticesContainer == null) return;

        // 更新當前基準位置
        currentBasePosition = newALocalPosition;

        if (debugCoordinateTransform)
        {
            Debug.Log($"=== 移動頂點組到新本地位置 ===");
            Debug.Log($"目標A點本地位置（相對於Vertices容器）: {newALocalPosition}");
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
        }

        // 以新的A點本地位置為基準，重新計算所有頂點
        float currentMultiplier = isScaled ? scaleMultiplier : 1f;
        for (int i = 0; i < 8; i++)
        {
            if (vertices[i] != null)
            {
                Vector3 scaledRelativePos = originalRelativePositions[i] * currentMultiplier;
                Vector3 newLocalPosition = newALocalPosition + scaledRelativePos;
                Vector3 newWorldPosition = verticesContainer.TransformPoint(newLocalPosition);

                vertices[i].position = newWorldPosition;

                if (debugCoordinateTransform)
                {
                    char vertexName = (char)('A' + i);
                    Debug.Log($"Vertex {vertexName}[{i}]: 本地={newLocalPosition}, 世界={newWorldPosition}");
                }
            }
        }

        UpdateLineRenderers();
        UpdateInteractiveLines();
       
        Debug.Log($"頂點組移動完成！A點本地位置: {newALocalPosition}");
    }

    // 便利方法
    public void MoveVertexGroupToLocal(float localX, float localY, float localZ)
    {
        Vector3 newLocalPosition = new Vector3(localX, localY, localZ);
        SetVertexGroupLocalPosition(newLocalPosition);
    }

    // 修正的獲取當前A點世界座標
    public Vector3 GetCurrentAWorldPosition()
    {
        if (vertices != null && vertices.Length > 0 && vertices[0] != null)
        {
            return vertices[0].position;
        }
        return Vector3.zero;
    }

    // 修正的獲取當前A點本地座標
    public Vector3 GetCurrentALocalPosition()
    {
        if (verticesContainer == null || vertices == null || vertices.Length == 0 || vertices[0] == null)
            return currentBasePosition;

        return verticesContainer.InverseTransformPoint(vertices[0].position);
    }

    // 修正的重置方法
    public void ResetVertexGroupPosition()
    {
        SetVertexGroupLocalPosition(new Vector3(-0.5f, 4.5f, 0f));
    }

    // 修正的OnValidate方法
    void OnValidate()
    {
        if (autoUpdateInEditor && Application.isPlaying && hasInitializedRelativePositions)
        {
            SetVertexGroupLocalPosition(targetALocalPosition);
        }
    }

    // 修正的世界座標方法
    public void SetVertexGroupWorldPosition(Vector3 newAWorldPosition)
    {
        if (verticesContainer == null) InitializeVerticesContainer();
        Vector3 newALocalPosition = verticesContainer.InverseTransformPoint(newAWorldPosition);
        SetVertexGroupLocalPosition(newALocalPosition);
    }

    void InitializeTransforms()
    {
        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };

        // 儲存當前展開狀態
        unfoldedTransforms = new TransformData[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            unfoldedTransforms[i] = new TransformData(faces[i]);
        }

        // 設定折疊狀態的目標變換
        RecalculateFoldTransforms();
    }

    void SetupLineRenderers()
    {
        SetupLineRenderer(lineCH);
        SetupLineRenderer(lineAF);
        SetupLineRenderer(lineDE);
        SetupLineRenderer(lineCF);
    }

    void SetupLineRenderer(LineRenderer line)
    {
        if (line != null)
        {
            line.material = skewLineMaterial;
            float currentWidth = isScaled ? 0.02f * scaleMultiplier : 0.02f;
            line.startWidth = currentWidth;
            line.endWidth = currentWidth;
            line.positionCount = 2;
        }
    }

    void Setup3DButtons()
    {
        SetupButton(foldButton3D, () => StartFolding(true));
        SetupButton(unfoldButton3D, () => StartFolding(false));
    }

    void SetupButton(GameObject button, System.Action onClickAction)
    {
        if (button != null)
        {
            var interactable = button.GetComponent<Interactable>();
            if (interactable == null)
                interactable = button.AddComponent<Interactable>();

            interactable.OnClick.AddListener(() => onClickAction());
        }
    }

    public void StartFolding(bool fold)
    {
        if (isAnimating || (fold && !isUnfolded) || (!fold && isUnfolded))
            return;

        StartCoroutine(FoldAnimation(fold));
    }

    IEnumerator FoldAnimation(bool fold)
    {
        isAnimating = true;
        float elapsed = 0f;

        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };

        // 記錄起始狀態
        TransformData[] startTransforms = new TransformData[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            startTransforms[i] = new TransformData(faces[i]);
        }

        // 記錄起始頂點位置
        Vector3[] startVertexPositions = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            startVertexPositions[i] = vertices[i].position;
        }

        TransformData[] targetTransforms = fold ? foldedTransforms : unfoldedTransforms;

        // 計算目標頂點位置
        Vector3[] targetVertexPositions = fold ? GetFoldedVertexPositions() : GetUnfoldedVertexPositions();

        while (elapsed < foldDuration)
        {
            elapsed += Time.deltaTime;
            float t = foldCurve.Evaluate(elapsed / foldDuration);

            // 插值所有面的變換
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i].localPosition = Vector3.Lerp(startTransforms[i].position, targetTransforms[i].position, t);
                faces[i].localRotation = Quaternion.Lerp(startTransforms[i].rotation, targetTransforms[i].rotation, t);
                faces[i].localScale = Vector3.Lerp(startTransforms[i].scale, targetTransforms[i].scale, t);
            }

            // 插值所有頂點位置
            for (int i = 0; i < 8; i++)
            {
                vertices[i].position = Vector3.Lerp(startVertexPositions[i], targetVertexPositions[i], t);
            }

            UpdateLineRenderers();
            UpdateInteractiveLines();
            yield return null;
        }

        // 確保最終狀態準確
        for (int i = 0; i < faces.Length; i++)
        {
            faces[i].localPosition = targetTransforms[i].position;
            faces[i].localRotation = targetTransforms[i].rotation;
            faces[i].localScale = targetTransforms[i].scale;
        }

        for (int i = 0; i < 8; i++)
        {
            vertices[i].position = targetVertexPositions[i];
        }

        isUnfolded = !fold;
        isAnimating = false;

        UpdateStatusDisplay();
        UpdateInteractiveLines();

        Debug.Log($"Animation complete. IsUnfolded: {isUnfolded}");
    }

    void UpdateLineRenderers()
    {
        if (vertices.Length >= 8)
        {
            UpdateLine(lineCH, 2, 7); // C到H
            UpdateLine(lineAF, 0, 5); // A到F
            UpdateLine(lineDE, 3, 4); // D到E
            UpdateLine(lineCF, 2, 5); // C到F
        }
    }

    void UpdateLine(LineRenderer line, int startIndex, int endIndex)
    {
        if (line != null && vertices[startIndex] != null && vertices[endIndex] != null)
        {
            line.SetPosition(0, vertices[startIndex].position);
            line.SetPosition(1, vertices[endIndex].position);
        }
    }

    void UpdateMathDisplay()
    {
        if (mathAnswerText3D != null)
        {
            mathAnswerText3D.text = "幾何關係分析:\n\n" +
                                   "1. CH與AF線段: 歪斜\n" +
                                   "   (非平行且不相交)\n\n" +
                                   "2. DE與CF線段: 歪斜\n" +
                                   "   (非平行且不相交)\n\n" +
                                   "用手勢或語音說 '折疊' 或 '展開'";
        }
    }

    void Update()
    {
        if (!isAnimating)
        {
            UpdateLineRenderers();
            UpdateInteractiveLines();
        }
    }

    // MRTK指針處理
    public void OnPointerDown(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        StartFolding(!isUnfolded);
    }

    // 語音命令處理
    public void OnVoiceCommandFold()
    {
        StartFolding(true);
    }

    public void OnVoiceCommandUnfold()
    {
        StartFolding(false);
    }

    // 數學驗證方法
    public void AnalyzeLineRelationship()
    {
        if (vertices.Length >= 8)
        {
            bool chAfParallel = AreLinesParallel(vertices[2], vertices[7], vertices[0], vertices[5]);
            bool chAfSkew = AreLinesSkew(vertices[2], vertices[7], vertices[0], vertices[5]);

            bool deCfParallel = AreLinesParallel(vertices[3], vertices[4], vertices[2], vertices[5]);
            bool deCfSkew = AreLinesSkew(vertices[3], vertices[4], vertices[2], vertices[5]);

            Debug.Log($"CH與AF: 平行={chAfParallel}, 歪斜={chAfSkew}");
            Debug.Log($"DE與CF: 平行={deCfParallel}, 歪斜={deCfSkew}");
        }
    }

    bool AreLinesParallel(Transform p1, Transform p2, Transform p3, Transform p4)
    {
        Vector3 dir1 = (p2.position - p1.position).normalized;
        Vector3 dir2 = (p4.position - p3.position).normalized;
        return Vector3.Cross(dir1, dir2).magnitude < 0.01f;
    }

    bool AreLinesSkew(Transform p1, Transform p2, Transform p3, Transform p4)
    {
        Vector3 dir1 = p2.position - p1.position;
        Vector3 dir2 = p4.position - p3.position;
        Vector3 connecting = p3.position - p1.position;

        float mixedProduct = Vector3.Dot(Vector3.Cross(dir1, dir2), connecting);
        return Mathf.Abs(mixedProduct) > 0.01f;
    }

    // ===== 其他原有的測試和調試方法 =====
    [ContextMenu("驗證折疊後A點位置")]
    public void VerifyFoldedAPosition()
    {
        Vector3[] foldedPos = GetFoldedVertexPositions();
        if (verticesContainer != null && foldedPos.Length > 0)
        {
            Vector3 foldedALocal = verticesContainer.InverseTransformPoint(foldedPos[0]);
            float currentMultiplier = isScaled ? scaleMultiplier : 1f;
            Vector3 expectedALocal = currentBasePosition + new Vector3(0f, 0f, -0.5f * currentMultiplier);

            Debug.Log($"當前展開A點: {currentBasePosition}");
            Debug.Log($"計算折疊A點: {foldedALocal}");
            Debug.Log($"期望折疊A點: {expectedALocal}");
            Debug.Log($"差異: {foldedALocal - expectedALocal}");
        }
    }

    // 便利方法：快速設定常用位置（世界座標）
    [ContextMenu("Move to World Origin")]
    public void MoveToWorldOrigin()
    {
        MoveVertexGroupWorld(0, 0, 0);
    }

    [ContextMenu("Move to Original Position")]
    public void MoveToOriginalPosition()
    {
        ResetVertexGroupPosition();
    }

    [ContextMenu("Move Up 1 Unit (World)")]
    public void MoveUpWorld()
    {
        Vector3 current = GetCurrentAWorldPosition();
        MoveVertexGroupWorld(current.x, current.y + 1f, current.z);
    }

    [ContextMenu("Move Down 1 Unit (World)")]
    public void MoveDownWorld()
    {
        Vector3 current = GetCurrentAWorldPosition();
        MoveVertexGroupWorld(current.x, current.y - 1f, current.z);
    }

    // 修正後的便利方法
    [ContextMenu("Move to Local Origin")]
    public void MoveToLocalOrigin()
    {
        MoveVertexGroupToLocal(0, 0, 0);
    }

    [ContextMenu("Move to Target Position (-0.5, 4.5, 0)")]
    public void MoveToTargetPosition()
    {
        MoveVertexGroupToLocal(-0.5f, 4.5f, 0f);
    }

    [ContextMenu("Move Up 1 Unit (Local)")]
    public void MoveUpLocal()
    {
        Vector3 current = GetCurrentALocalPosition();
        MoveVertexGroupToLocal(current.x, current.y + 1f, current.z);
    }

    [ContextMenu("Move Down 1 Unit (Local)")]
    public void MoveDownLocal()
    {
        Vector3 current = GetCurrentALocalPosition();
        MoveVertexGroupToLocal(current.x, current.y - 1f, current.z);
    }

    [ContextMenu("Test Move A to (0, 0, 0) Local")]
    public void TestMoveAToLocalOrigin()
    {
        MoveVertexGroupToLocal(0f, 0f, 0f);
    }

    [ContextMenu("Debug Current Positions")]
    public void DebugCurrentPositions()
    {
        Debug.Log("=== 當前位置調試信息 ===");
        if (verticesContainer != null)
        {
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
            Debug.Log($"Vertices容器本地位置: {verticesContainer.localPosition}");

            Transform cubeController = verticesContainer.parent;
            if (cubeController != null)
            {
                Debug.Log($"CubeController世界位置: {cubeController.position}");
            }
        }

        Vector3 currentAWorld = GetCurrentAWorldPosition();
        Vector3 currentALocal = GetCurrentALocalPosition();

        Debug.Log($"當前A點世界座標: {currentAWorld}");
        Debug.Log($"當前A點本地座標（相對於Vertices容器）: {currentALocal}");

        // 計算如果A點在(-0.5, 4.5, 0)本地座標時的世界座標
        if (verticesContainer != null)
        {
            Vector3 targetLocal = new Vector3(-0.5f, 4.5f, 0f);
            Vector3 targetWorld = verticesContainer.TransformPoint(targetLocal);
            Debug.Log($"目標本地座標 {targetLocal} 對應的世界座標應該是: {targetWorld}");
        }
    }

    // 測試和調試方法
    [ContextMenu("Move A to Target (-0.5, 4.5, 0)")]
    public void MoveAToTarget()
    {
        MoveVertexGroupToLocal(-0.5f, 4.5f, 0f);
    }

    [ContextMenu("Move A to (0, 0, 0)")]
    public void MoveAToLocalOrigin()
    {
        MoveVertexGroupToLocal(0f, 0f, 0f);
    }

    [ContextMenu("Test Fold Animation")]
    public void TestFoldAnimation()
    {
        Debug.Log("開始測試折疊動畫...");
        StartFolding(true);
    }

    [ContextMenu("Test Unfold Animation")]
    public void TestUnfoldAnimation()
    {
        Debug.Log("開始測試展開動畫...");
        StartFolding(false);
    }

    [ContextMenu("Debug Complete State")]
    public void DebugCompleteState()
    {
        Debug.Log("=== 完整狀態調試 ===");
        Debug.Log($"當前基準位置: {currentBasePosition}");
        Debug.Log($"是否展開: {isUnfolded}");
        Debug.Log($"是否動畫中: {isAnimating}");
        Debug.Log($"是否已縮放: {isScaled}");
        Debug.Log($"縮放倍數: {scaleMultiplier}");

        if (verticesContainer != null)
        {
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
            Debug.Log($"Vertices容器本地位置: {verticesContainer.localPosition}");
        }

        Vector3 currentAWorld = GetCurrentAWorldPosition();
        Vector3 currentALocal = GetCurrentALocalPosition();

        Debug.Log($"A點當前世界座標: {currentAWorld}");
        Debug.Log($"A點當前本地座標: {currentALocal}");

        // 測試座標轉換
        Vector3 testLocal = new Vector3(-0.5f, 4.5f, 0f);
        Vector3 testWorld = verticesContainer.TransformPoint(testLocal);
        Debug.Log($"測試：本地座標 {testLocal} → 世界座標 {testWorld}");
    }

    // 修正的通過世界座標設定方法
    public void MoveVertexGroupWorld(float worldX, float worldY, float worldZ)
    {
        Vector3 newWorldPosition = new Vector3(worldX, worldY, worldZ);
        SetVertexGroupWorldPosition(newWorldPosition);
    }

    // 用於動畫過程中的平滑移動（世界座標）
    public void SmoothMoveVertexGroupWorld(Vector3 targetWorldPosition, float duration = 1f)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(SmoothMoveWorldCoroutine(targetWorldPosition, duration));
    }

    private IEnumerator SmoothMoveWorldCoroutine(Vector3 targetWorldPosition, float duration)
    {
        Vector3 startWorldPosition = GetCurrentAWorldPosition();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            Vector3 currentWorldPosition = Vector3.Lerp(startWorldPosition, targetWorldPosition, t);
            SetVertexGroupWorldPosition(currentWorldPosition);
            yield return null;
        }

        SetVertexGroupWorldPosition(targetWorldPosition);
    }

    // 獲取完整的座標信息（用於調試）
    [ContextMenu("Debug All Positions")]
    public void DebugAllPositions()
    {
        Debug.Log("=== 完整座標信息 ===");
        Debug.Log($"CubeController位置: {transform.root.position}");

        if (verticesContainer != null)
        {
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
            Debug.Log($"Vertices容器本地位置: {verticesContainer.localPosition}");
        }

        Vector3 currentAWorld = GetCurrentAWorldPosition();
        Vector3 currentALocal = GetCurrentALocalPosition();

        Debug.Log($"當前A點世界座標: {currentAWorld}");
        Debug.Log($"當前A點本地座標（相對於Vertices容器）: {currentALocal}");
    }
}