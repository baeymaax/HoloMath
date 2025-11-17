using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;

public class HoloLensCubeFoldController : MonoBehaviour, IMixedRealityPointerHandler
{
    [Header("Cube Faces")]
    public Transform faceA_Center;
    public Transform faceB_Right;
    public Transform faceE_Left;
    public Transform faceH_Top;
    public Transform faceF_Bottom;
    public Transform faceG_Far;

    [Header("Vertices with Buttons (按順序: A,B,C,D,E,F,G,H)")]
    public Transform[] vertices = new Transform[8];
   
    [Header("Vertex Button Prefab")]
    public GameObject vertexButtonPrefab;
   
    [Header("Interactive Line Drawing")]
    public Material interactiveLineMaterial;
    public float interactiveLineWidth = 0.012f;
    public Color selectedVertexColor = Color.yellow;
    public Color normalVertexColor = Color.white;
   
    [Header("Line Counting Display")]
    public TextMeshPro lineCountText3D;

    [Header("Line Renderers for Question Segments")]
    public LineRenderer lineCH;
    public LineRenderer lineAF;
    public LineRenderer lineDE;
    public LineRenderer lineCF;

    [Header("3D UI Elements")]
    public GameObject foldButton3D;
    public GameObject unfoldButton3D;
    public TextMeshPro statusText3D;
    public GameObject clearLinesButton3D;  // 新增：清除線段按鈕
    public TextMeshPro mathAnswerText3D;

    [Header("Animation Settings")]
    public float foldDuration = 3f;
    public AnimationCurve foldCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visualization Settings")]
    public Material parallelLineMaterial;
    public Material skewLineMaterial;
    public Material vertexMaterial;
    public Material faceMaterial;

    [Header("Coordinate System")]
    public Transform coordinateSystemRoot;
   
    [Header("Vertex Position Control")]
    [SerializeField] private Vector3 targetALocalPosition = new Vector3(-0.5f, 4.5f, 0f);
    [SerializeField] private bool autoUpdateInEditor = true;
    [SerializeField] private bool debugCoordinateTransform = true;

    private Vector3 currentBasePosition = new Vector3(-0.5f, 4.5f, 0f);
    private Vector3[] originalRelativePositions;
    private bool hasInitializedRelativePositions = false;
    private Transform verticesContainer;
    
    private bool isUnfolded = true;
    private bool isAnimating = false;
    
    private TransformData[] unfoldedTransforms;
    private TransformData[] foldedTransforms;
    
    private Transform selectedVertex1 = null;
    private Transform selectedVertex2 = null;
    private List<InteractiveLine> drawnLines = new List<InteractiveLine>();
    private Dictionary<Transform, GameObject> vertexButtons = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, Renderer> vertexRenderers = new Dictionary<Transform, Renderer>();

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

        InitializeVerticesContainer();
        InitializeRelativePositions();
        InitializeVertexButtons();
        InitializeTransforms();
        SetupLineRenderers();
        Setup3DButtons();
        UpdateStatusDisplay();
        UpdateMathDisplay();
        UpdateLineCountDisplay();
        DebugCoordinateSystem();
        DebugVerticesInfo();
    }

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
                GameObject buttonObj = Instantiate(vertexButtonPrefab, vertices[i]);
                buttonObj.name = $"VertexButton_{(char)('A' + i)}";
                buttonObj.transform.localPosition = Vector3.zero;
                buttonObj.transform.localScale = Vector3.one * 0.1f;
                
                vertexButtons[vertices[i]] = buttonObj;
                
                var interactable = buttonObj.GetComponent<Interactable>();
                if (interactable != null)
                {
                    int vertexIndex = i;
                    Transform vertex = vertices[i];
                    interactable.OnClick.AddListener(() => OnVertexButtonClicked(vertex, vertexIndex));
                }

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

    void SetupInteractiveLineRenderer(LineRenderer line)
    {
        if (line != null)
        {
            line.material = interactiveLineMaterial != null ? interactiveLineMaterial : skewLineMaterial;
            line.startWidth = interactiveLineWidth;
            line.endWidth = interactiveLineWidth;
            line.positionCount = 2;
            line.useWorldSpace = true;
        }
    }

    void OnVertexButtonClicked(Transform clickedVertex, int vertexIndex)
    {
        char vertexName = (char)('A' + vertexIndex);
        Debug.Log($"點擊了頂點 {vertexName}");

        if (selectedVertex1 == null)
        {
            selectedVertex1 = clickedVertex;
            SetVertexColor(selectedVertex1, selectedVertexColor);
            Debug.Log($"選擇第一個頂點: {vertexName}");
        }
        else if (selectedVertex2 == null && clickedVertex != selectedVertex1)
        {
            selectedVertex2 = clickedVertex;
            SetVertexColor(selectedVertex2, selectedVertexColor);
            DrawLineBetweenVertices(selectedVertex1, selectedVertex2);
            Debug.Log($"選擇第二個頂點: {vertexName}，開始繪製線段");
        }
        else
        {
            ResetVertexSelection();
            selectedVertex1 = clickedVertex;
            SetVertexColor(selectedVertex1, selectedVertexColor);
            Debug.Log($"重置選擇，重新選擇第一個頂點: {vertexName}");
        }
    }

    public void OnVertexA_Clicked() { OnVertexButtonClicked(vertices[0], 0); }
    public void OnVertexB_Clicked() { OnVertexButtonClicked(vertices[1], 1); }
    public void OnVertexC_Clicked() { OnVertexButtonClicked(vertices[2], 2); }
    public void OnVertexD_Clicked() { OnVertexButtonClicked(vertices[3], 3); }
    public void OnVertexE_Clicked() { OnVertexButtonClicked(vertices[4], 4); }
    public void OnVertexF_Clicked() { OnVertexButtonClicked(vertices[5], 5); }
    public void OnVertexG_Clicked() { OnVertexButtonClicked(vertices[6], 6); }
    public void OnVertexH_Clicked() { OnVertexButtonClicked(vertices[7], 7); }

    void DrawLineBetweenVertices(Transform vertex1, Transform vertex2)
    {
        if (vertex1 == null || vertex2 == null) return;

        bool lineExists = drawnLines.Exists(line =>
            (line.vertex1 == vertex1 && line.vertex2 == vertex2) ||
            (line.vertex1 == vertex2 && line.vertex2 == vertex1));

        if (lineExists)
        {
            Debug.Log("這兩個頂點之間已經存在線段！");
            ResetVertexSelection();
            return;
        }

        GameObject lineObj = new GameObject($"InteractiveLine_{GetVertexName(vertex1)}{GetVertexName(vertex2)}");
        lineObj.transform.SetParent(this.transform);
       
        LineRenderer newLine = lineObj.AddComponent<LineRenderer>();
        SetupInteractiveLineRenderer(newLine);
        
        newLine.SetPosition(0, vertex1.position);
        newLine.SetPosition(1, vertex2.position);

        InteractiveLine interactiveLine = new InteractiveLine(vertex1, vertex2, newLine);
        drawnLines.Add(interactiveLine);

        Debug.Log($"成功繪製線段：{interactiveLine.lineName}");
        ResetVertexSelection();
        UpdateLineCountDisplay();
    }

    void SetVertexColor(Transform vertex, Color color)
    {
        if (vertexRenderers.ContainsKey(vertex) && vertexRenderers[vertex] != null)
        {
            vertexRenderers[vertex].material.color = color;
        }
    }

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

    [ContextMenu("Clear All Drawn Lines")]
    public void ClearAllDrawnLines()
    {
        int clearedCount = drawnLines.Count;

        foreach (var line in drawnLines)
        {
            if (line.lineRenderer != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(line.lineRenderer.gameObject);
                }
                else
                {
                    DestroyImmediate(line.lineRenderer.gameObject);
                }
            }
        }

        drawnLines.Clear();
        ResetVertexSelection();
        UpdateLineCountDisplay();

        // 更新狀態顯示
        if (statusText3D != null)
        {
            string state = isUnfolded ? "展開狀態" : "立方體狀態";
            statusText3D.text = $"{state}\n已清除 {clearedCount} 條線段";

            // 2秒後恢復正常顯示
            if (Application.isPlaying)
            {
                StartCoroutine(ResetStatusTextDelayed(2f));
            }
        }

        Debug.Log($"已清除所有已繪製的線段，共 {clearedCount} 條");
    }

    // 新增：延遲恢復狀態文字的協程
    private IEnumerator ResetStatusTextDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateStatusDisplay();
    }

    public void ClearLastDrawnLine()
    {
        if (drawnLines.Count == 0)
        {
            Debug.Log("沒有可清除的線段");
            return;
        }

        var lastLine = drawnLines[drawnLines.Count - 1];

        if (lastLine.lineRenderer != null)
        {
            if (Application.isPlaying)
            {
                Destroy(lastLine.lineRenderer.gameObject);
            }
            else
            {
                DestroyImmediate(lastLine.lineRenderer.gameObject);
            }
        }

        drawnLines.RemoveAt(drawnLines.Count - 1);
        ResetVertexSelection();
        UpdateLineCountDisplay();

        Debug.Log($"已清除最後一條線段：{lastLine.lineName}");
    }

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

    void InitializeVerticesContainer()
    {
        verticesContainer = transform.Find("Vertices");
        if (verticesContainer == null)
        {
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

    void InitializeRelativePositions()
    {
        if (vertices == null || vertices.Length < 8) return;
        if (verticesContainer == null) InitializeVerticesContainer();

        originalRelativePositions = new Vector3[8];

        Vector3[] defaultUnfoldedPositions = {
            new Vector3(-0.5f, 4.5f, 0f),
            new Vector3(0.5f, 4.5f, 0f),
            new Vector3(0.5f, 5.5f, 0f),
            new Vector3(-0.5f, 5.5f, 0f),
            new Vector3(-1.5f, 4.5f, 0f),
            new Vector3(0.5f, 3.5f, 0f),
            new Vector3(1.5f, 5.5f, 0f),
            new Vector3(-0.5f, 6.5f, 0f)
        };

        Vector3 basePosition = defaultUnfoldedPositions[0];
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

    public void SetVertexGroupLocalPosition(Vector3 newALocalPosition)
    {
        if (!hasInitializedRelativePositions)
        {
            InitializeRelativePositions();
        }

        if (vertices == null || vertices.Length < 8 || verticesContainer == null) return;

        currentBasePosition = newALocalPosition;

        if (debugCoordinateTransform)
        {
            Debug.Log($"=== 移動頂點組到新本地位置 ===");
            Debug.Log($"目標A點本地位置（相對於Vertices容器）: {newALocalPosition}");
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
        }

        for (int i = 0; i < 8; i++)
        {
            if (vertices[i] != null)
            {
                Vector3 newLocalPosition = newALocalPosition + originalRelativePositions[i];
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

    public void MoveVertexGroupToLocal(float localX, float localY, float localZ)
    {
        Vector3 newLocalPosition = new Vector3(localX, localY, localZ);
        SetVertexGroupLocalPosition(newLocalPosition);
    }

    public Vector3 GetCurrentAWorldPosition()
    {
        if (vertices != null && vertices.Length > 0 && vertices[0] != null)
        {
            return vertices[0].position;
        }
        return Vector3.zero;
    }

    public Vector3 GetCurrentALocalPosition()
    {
        if (verticesContainer == null || vertices == null || vertices.Length == 0 || vertices[0] == null)
            return currentBasePosition;

        return verticesContainer.InverseTransformPoint(vertices[0].position);
    }

    public void ResetVertexGroupPosition()
    {
        SetVertexGroupLocalPosition(new Vector3(-0.5f, 4.5f, 0f));
    }

    void OnValidate()
    {
        if (autoUpdateInEditor && Application.isPlaying && hasInitializedRelativePositions)
        {
            SetVertexGroupLocalPosition(targetALocalPosition);
        }
    }

    public void SetVertexGroupWorldPosition(Vector3 newAWorldPosition)
    {
        if (verticesContainer == null) InitializeVerticesContainer();
        Vector3 newALocalPosition = verticesContainer.InverseTransformPoint(newAWorldPosition);
        SetVertexGroupLocalPosition(newALocalPosition);
    }

    void InitializeTransforms()
    {
        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };

        unfoldedTransforms = new TransformData[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            unfoldedTransforms[i] = new TransformData(faces[i]);
        }

        RecalculateFoldTransforms();
    }

    void RecalculateFoldTransforms()
    {
        Transform[] faces = { faceA_Center, faceB_Right, faceE_Left, faceH_Top, faceF_Bottom, faceG_Far };
        foldedTransforms = new TransformData[faces.Length];
        float faceSize = 0.5f;

        foldedTransforms[0] = new TransformData(faces[0]);
        foldedTransforms[0].position = unfoldedTransforms[0].position - Vector3.forward * faceSize;
        foldedTransforms[0].rotation = unfoldedTransforms[0].rotation;

        foldedTransforms[1] = new TransformData(faces[1]);
        foldedTransforms[1].position = unfoldedTransforms[0].position + Vector3.right * faceSize;
        foldedTransforms[1].rotation = Quaternion.Euler(0, 90, 0) * unfoldedTransforms[0].rotation;

        foldedTransforms[2] = new TransformData(faces[2]);
        foldedTransforms[2].position = unfoldedTransforms[0].position + Vector3.left * faceSize;
        foldedTransforms[2].rotation = Quaternion.Euler(0, -90, 0) * unfoldedTransforms[0].rotation;

        foldedTransforms[3] = new TransformData(faces[3]);
        foldedTransforms[3].position = unfoldedTransforms[0].position + Vector3.up * faceSize;
        foldedTransforms[3].rotation = Quaternion.Euler(-90, 0, 0) * unfoldedTransforms[0].rotation;

        foldedTransforms[4] = new TransformData(faces[4]);
        foldedTransforms[4].position = unfoldedTransforms[0].position + Vector3.down * faceSize;
        foldedTransforms[4].rotation = Quaternion.Euler(90, 0, 0) * unfoldedTransforms[0].rotation;

        foldedTransforms[5] = new TransformData(faces[5]);
        foldedTransforms[5].position = unfoldedTransforms[0].position + Vector3.forward * faceSize;
        foldedTransforms[5].rotation = Quaternion.Euler(0, 0, 0) * unfoldedTransforms[0].rotation;
    }

    Vector3[] GetFoldedVertexPositions()
    {
        Vector3 foldedALocal = currentBasePosition + new Vector3(0f, 0f, -0.5f);
        Vector3 centerLocal = foldedALocal + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 cubeCenter = verticesContainer.TransformPoint(centerLocal);
        float halfEdge = 0.2f;

        Vector3[] foldedPositions = new Vector3[8];
        foldedPositions[0] = cubeCenter + new Vector3(-halfEdge, -halfEdge, -halfEdge);
        foldedPositions[1] = cubeCenter + new Vector3(halfEdge, -halfEdge, -halfEdge);
        foldedPositions[2] = cubeCenter + new Vector3(halfEdge, halfEdge, -halfEdge);
        foldedPositions[3] = cubeCenter + new Vector3(-halfEdge, halfEdge, -halfEdge);
        foldedPositions[4] = cubeCenter + new Vector3(-halfEdge, -halfEdge, halfEdge);
        foldedPositions[5] = cubeCenter + new Vector3(halfEdge, -halfEdge, halfEdge);
        foldedPositions[6] = cubeCenter + new Vector3(halfEdge, halfEdge, halfEdge);
        foldedPositions[7] = cubeCenter + new Vector3(-halfEdge, halfEdge, halfEdge);

        if (debugCoordinateTransform)
        {
            Debug.Log("=== 折疊狀態計算 ===");
            Debug.Log($"展開狀態A點基準位置: {currentBasePosition}");
            Debug.Log($"折疊後A點本地位置: {foldedALocal}");
            Debug.Log($"立方體中心本地位置: {centerLocal}");
            Debug.Log($"立方體中心世界位置: {cubeCenter}");
        }

        return foldedPositions;
    }

    Vector3[] GetUnfoldedVertexPositions()
    {
        Vector3[] unfoldedPositions = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 localPosition = currentBasePosition + originalRelativePositions[i];
            unfoldedPositions[i] = verticesContainer.TransformPoint(localPosition);
        }

        if (debugCoordinateTransform)
        {
            Debug.Log("=== 計算展開狀態頂點位置 ===");
            Debug.Log($"使用基準位置: {currentBasePosition}");
            for (int i = 0; i < 8; i++)
            {
                char vertexName = (char)('A' + i);
                Debug.Log($"Vertex {vertexName}[{i}] 展開位置: {unfoldedPositions[i]}");
            }
        }

        return unfoldedPositions;
    }

    void UpdateStatusDisplay()
    {
        if (statusText3D != null)
        {
            string state = isUnfolded ? "展開狀態" : "立方體狀態";
            statusText3D.text = state;
        }
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
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.positionCount = 2;
        }
    }

    void Setup3DButtons()
    {
        SetupButton(foldButton3D, () => StartFolding(true));
        SetupButton(unfoldButton3D, () => StartFolding(false));
        SetupButton(clearLinesButton3D, () => ClearAllDrawnLines());  // 新增
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

        TransformData[] startTransforms = new TransformData[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            startTransforms[i] = new TransformData(faces[i]);
        }

        Vector3[] startVertexPositions = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            startVertexPositions[i] = vertices[i].position;
        }

        TransformData[] targetTransforms = fold ? foldedTransforms : unfoldedTransforms;
        Vector3[] targetVertexPositions = fold ? GetFoldedVertexPositions() : GetUnfoldedVertexPositions();

        while (elapsed < foldDuration)
        {
            elapsed += Time.deltaTime;
            float t = foldCurve.Evaluate(elapsed / foldDuration);

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i].localPosition = Vector3.Lerp(startTransforms[i].position, targetTransforms[i].position, t);
                faces[i].localRotation = Quaternion.Lerp(startTransforms[i].rotation, targetTransforms[i].rotation, t);
                faces[i].localScale = Vector3.Lerp(startTransforms[i].scale, targetTransforms[i].scale, t);
            }

            for (int i = 0; i < 8; i++)
            {
                vertices[i].position = Vector3.Lerp(startVertexPositions[i], targetVertexPositions[i], t);
            }

            UpdateLineRenderers();
            UpdateInteractiveLines();
            yield return null;
        }

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
            UpdateLine(lineCH, 2, 7);
            UpdateLine(lineAF, 0, 5);
            UpdateLine(lineDE, 3, 4);
            UpdateLine(lineCF, 2, 5);
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

    public void OnPointerDown(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        StartFolding(!isUnfolded);
    }

    public void OnVoiceCommandFold()
    {
        StartFolding(true);
    }

    public void OnVoiceCommandUnfold()
    {
        StartFolding(false);
    }

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

    [ContextMenu("驗證折疊後A點位置")]
    public void VerifyFoldedAPosition()
    {
        Vector3[] foldedPos = GetFoldedVertexPositions();
        if (verticesContainer != null && foldedPos.Length > 0)
        {
            Vector3 foldedALocal = verticesContainer.InverseTransformPoint(foldedPos[0]);
            Vector3 expectedALocal = currentBasePosition + new Vector3(0f, 0f, -0.5f);

            Debug.Log($"當前展開A點: {currentBasePosition}");
            Debug.Log($"計算折疊A點: {foldedALocal}");
            Debug.Log($"期望折疊A點: {expectedALocal}");
            Debug.Log($"差異: {foldedALocal - expectedALocal}");
        }
    }

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

        if (verticesContainer != null)
        {
            Vector3 targetLocal = new Vector3(-0.5f, 4.5f, 0f);
            Vector3 targetWorld = verticesContainer.TransformPoint(targetLocal);
            Debug.Log($"目標本地座標 {targetLocal} 對應的世界座標應該是: {targetWorld}");
        }
    }

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

        if (verticesContainer != null)
        {
            Debug.Log($"Vertices容器世界位置: {verticesContainer.position}");
            Debug.Log($"Vertices容器本地位置: {verticesContainer.localPosition}");
        }

        Vector3 currentAWorld = GetCurrentAWorldPosition();
        Vector3 currentALocal = GetCurrentALocalPosition();
        Debug.Log($"A點當前世界座標: {currentAWorld}");
        Debug.Log($"A點當前本地座標: {currentALocal}");

        Vector3 testLocal = new Vector3(-0.5f, 4.5f, 0f);
        Vector3 testWorld = verticesContainer.TransformPoint(testLocal);
        Debug.Log($"測試：本地座標 {testLocal} → 世界座標 {testWorld}");
    }

    public void MoveVertexGroupWorld(float worldX, float worldY, float worldZ)
    {
        Vector3 newWorldPosition = new Vector3(worldX, worldY, worldZ);
        SetVertexGroupWorldPosition(newWorldPosition);
    }

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