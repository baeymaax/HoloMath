using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImprovedCubeTransformSystem : MonoBehaviour
{
    #region 狀態枚舉和資料結構
    
    public enum CubeState
    {
        NormalUnfolded,  // 正常大小展開
        NormalFolded,    // 正常大小折疊
        ScaledUnfolded,  // 放大展開
        ScaledFolded     // 放大折疊
    }

    [System.Serializable]
    public struct TransformState
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        
        public TransformState(Vector3 pos, Vector3 rot, Vector3 scl)
        {
            position = pos;
            rotation = rot;
            scale = scl;
        }
    }

    [System.Serializable]
    public struct CubeConfiguration
    {
        public Vector3[] vertexPositions;   // 8個頂點位置
        public TransformState[] faceStates; // 6個面的變換狀態
        
        public CubeConfiguration(Vector3[] vertices, TransformState[] faces)
        {
            vertexPositions = vertices;
            faceStates = faces;
        }
    }
    
    #endregion

    #region 配置參數
    
    [Header("基礎參數")]
    [SerializeField] private float scaleMultiplier = 5f;
    [SerializeField] private Vector3 baseVertexA = new Vector3(-0.5f, 4.5f, 0f);
    
    [Header("調試選項")]
    [SerializeField] private bool enableDebugLog = true;
    
    #endregion

    #region 核心座標計算方法

    /// <summary>
    /// 根據目標狀態和基準點計算所有頂點位置
    /// </summary>
    public Vector3[] CalculateVertexPositions(CubeState targetState, Vector3 referencePointA)
    {
        switch (targetState)
        {
            case CubeState.NormalUnfolded:
                return CalculateNormalUnfoldedVertices(referencePointA);
            
            case CubeState.NormalFolded:
                return CalculateNormalFoldedVertices(referencePointA);
            
            case CubeState.ScaledUnfolded:
                return CalculateScaledUnfoldedVertices(referencePointA);
            
            case CubeState.ScaledFolded:
                return CalculateScaledFoldedVertices(referencePointA);
            
            default:
                return CalculateNormalUnfoldedVertices(referencePointA);
        }
    }

    /// <summary>
    /// 根據目標狀態計算所有面的變換狀態
    /// </summary>
    public TransformState[] CalculateFaceStates(CubeState targetState, Vector3 referencePointA)
    {
        switch (targetState)
        {
            case CubeState.NormalUnfolded:
                return CalculateNormalUnfoldedFaces(referencePointA);
            
            case CubeState.NormalFolded:
                return CalculateNormalFoldedFaces(referencePointA);
            
            case CubeState.ScaledUnfolded:
                return CalculateScaledUnfoldedFaces(referencePointA);
            
            case CubeState.ScaledFolded:
                return CalculateScaledFoldedFaces(referencePointA);
            
            default:
                return CalculateNormalUnfoldedFaces(referencePointA);
        }
    }

    /// <summary>
    /// 獲取完整的立方體配置
    /// </summary>
    public CubeConfiguration GetCubeConfiguration(CubeState targetState, Vector3 referencePointA)
    {
        Vector3[] vertices = CalculateVertexPositions(targetState, referencePointA);
        TransformState[] faces = CalculateFaceStates(targetState, referencePointA);
        
        return new CubeConfiguration(vertices, faces);
    }

    #endregion

    #region 具體狀態計算方法

    private Vector3[] CalculateNormalUnfoldedVertices(Vector3 baseA)
    {
        // 基於你提供的正常展開狀態數據
        return new Vector3[]
        {
            baseA,                              // A(-0.5, 4.5, 0)
            baseA + new Vector3(1f, 0f, 0f),    // B(0.5, 4.5, 0)
            baseA + new Vector3(1f, 1f, 0f),    // C(0.5, 5.5, 0)
            baseA + new Vector3(0f, 1f, 0f),    // D(-0.5, 5.5, 0)
            baseA + new Vector3(-1f, 0f, 0f),   // E(-1.5, 4.5, 0)
            baseA + new Vector3(1f, -1f, 0f),   // F(0.5, 3.5, 0)
            baseA + new Vector3(2f, 1f, 0f),    // G(1.5, 5.5, 0)
            baseA + new Vector3(0f, 2f, 0f)     // H(-0.5, 6.5, 0)
        };
    }

    private Vector3[] CalculateNormalFoldedVertices(Vector3 baseA)
    {
        // 基於你提供的正常折疊狀態數據
        // 折疊後A點會向前移動0.5單位
        Vector3 foldedA = baseA + new Vector3(0f, 0f, -0.5f);
        
        return new Vector3[]
        {
            foldedA,                              // A(-0.5, 4.5, -0.5)
            foldedA + new Vector3(1f, 0f, 0f),    // B(0.5, 4.5, -0.5)
            foldedA + new Vector3(1f, 1f, 0f),    // C(0.5, 5.5, -0.5)
            foldedA + new Vector3(0f, 1f, 0f),    // D(-0.5, 5.5, -0.5)
            foldedA + new Vector3(0f, 0f, 1f),    // E(-0.5, 4.5, 0.5)
            foldedA + new Vector3(1f, 0f, 1f),    // F(0.5, 4.5, 0.5)
            foldedA + new Vector3(1f, 1f, 1f),    // G(0.5, 5.5, 0.5)
            foldedA + new Vector3(0f, 1f, 1f)     // H(-0.5, 5.5, 0.5)
        };
    }

    private Vector3[] CalculateScaledUnfoldedVertices(Vector3 baseA)
    {
        // 基於數據分析：放大時有特殊的變換規則
        // A點：(-0.5, 4.5, 0) → (-2.5, 2.5, 5)
        // 規律：X放大5倍，Y下移2單位，Z固定在5
        Vector3 scaledA = new Vector3(
            baseA.x * scaleMultiplier,
            baseA.y - 2f,
            5f
        );
        
        return new Vector3[]
        {
            scaledA,                                    // A(-2.5, 2.5, 5)
            scaledA + new Vector3(5f, 0f, 0f),         // B(2.5, 2.5, 5)
            scaledA + new Vector3(5f, 5f, 0f),         // C(2.5, 7.5, 5)
            scaledA + new Vector3(0f, 5f, 0f),         // D(-2.5, 7.5, 5)
            scaledA + new Vector3(-5f, 0f, 0f),        // E(-7.5, 2.5, 5)
            scaledA + new Vector3(5f, -5f, 0f),        // F(2.5, -2.5, 5)
            scaledA + new Vector3(10f, 0f, 0f),        // G(7.5, 2.5, 5)
            scaledA + new Vector3(0f, 10f, 0f)         // H(-2.5, 12.5, 5)
        };
    }

    private Vector3[] CalculateScaledFoldedVertices(Vector3 baseA)
    {
        // 基於放大折疊狀態數據
        // A點：(-0.5, 4.5, 0) → (-2.5, 2.5, 2.5)
        Vector3 scaledFoldedA = new Vector3(
            baseA.x * scaleMultiplier,
            baseA.y - 2f,
            2.5f  // Z軸偏移：5 - 2.5 = 2.5
        );
        
        return new Vector3[]
        {
            scaledFoldedA,                              // A(-2.5, 2.5, 2.5)
            scaledFoldedA + new Vector3(5f, 0f, 0f),    // B(2.5, 2.5, 2.5)
            scaledFoldedA + new Vector3(5f, 5f, 0f),    // C(2.5, 7.5, 2.5)
            scaledFoldedA + new Vector3(0f, 5f, 0f),    // D(-2.5, 7.5, 2.5)
            scaledFoldedA + new Vector3(0f, 0f, 5f),    // E(-2.5, 2.5, 7.5)
            scaledFoldedA + new Vector3(5f, 0f, 5f),    // F(2.5, 2.5, 7.5)
            scaledFoldedA + new Vector3(5f, 5f, 5f),    // G(2.5, 7.5, 7.5)
            scaledFoldedA + new Vector3(0f, 5f, 5f)     // H(-2.5, 7.5, 7.5)
        };
    }

    #endregion

    #region 面變換狀態計算

    private TransformState[] CalculateNormalUnfoldedFaces(Vector3 baseA)
    {
        // 面的順序：A_Center, B_Right, E_Left, H_Top, F_Bottom, G_Far
        Vector3 faceBaseOffset = new Vector3(0.5f, 0.5f, 0f); // 面中心相對於A點的偏移
        Vector3 faceCenter = baseA + faceBaseOffset;
        
        return new TransformState[]
        {
            new TransformState(faceCenter, Vector3.zero, Vector3.one),                    // FaceA_Center (0, 5, 0)
            new TransformState(faceCenter + new Vector3(1f, 0f, 0f), Vector3.zero, Vector3.one),  // FaceB_Right (1, 5, 0)
            new TransformState(faceCenter + new Vector3(-1f, 0f, 0f), Vector3.zero, Vector3.one), // FaceE_Left (-1, 5, 0)
            new TransformState(faceCenter + new Vector3(0f, 1f, 0f), Vector3.zero, Vector3.one),  // FaceH_Top (0, 6, 0)
            new TransformState(faceCenter + new Vector3(0f, -1f, 0f), Vector3.zero, Vector3.one), // FaceF_Bottom (0, 4, 0)
            new TransformState(faceCenter + new Vector3(2f, 0f, 0f), Vector3.zero, Vector3.one)   // FaceG_Far (2, 5, 0)
        };
    }

    private TransformState[] CalculateNormalFoldedFaces(Vector3 baseA)
    {
        Vector3 faceBaseOffset = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 faceCenter = baseA + faceBaseOffset;
        
        return new TransformState[]
        {
            new TransformState(faceCenter, Vector3.zero, Vector3.one),                              // FaceA_Center (0, 5, -0.5)
            new TransformState(faceCenter + new Vector3(0.5f, 0f, 0.5f), new Vector3(0, 90, 0), Vector3.one),  // FaceB_Right
            new TransformState(faceCenter + new Vector3(-0.5f, 0f, 0.5f), new Vector3(0, -90, 0), Vector3.one), // FaceE_Left
            new TransformState(faceCenter + new Vector3(0f, 0.5f, 0.5f), new Vector3(-90, 0, 0), Vector3.one),  // FaceH_Top
            new TransformState(faceCenter + new Vector3(0f, -0.5f, 0.5f), new Vector3(90, 0, 0), Vector3.one),  // FaceF_Bottom
            new TransformState(faceCenter + new Vector3(0f, 0f, 1f), Vector3.zero, Vector3.one)                // FaceG_Far (0, 5, 0.5)
        };
    }

    private TransformState[] CalculateScaledUnfoldedFaces(Vector3 baseA)
    {
        Vector3 scaledFaceCenter = new Vector3(baseA.x * scaleMultiplier + 2.5f, baseA.y - 2f + 2.5f, 5f);
        Vector3 scaleVector = Vector3.one * scaleMultiplier;
        
        return new TransformState[]
        {
            new TransformState(scaledFaceCenter, Vector3.zero, scaleVector),                      // FaceA_Center (0, 5, 5)
            new TransformState(scaledFaceCenter + new Vector3(5f, 0f, 0f), Vector3.zero, scaleVector),   // FaceB_Right (5, 5, 5)
            new TransformState(scaledFaceCenter + new Vector3(-5f, 0f, 0f), Vector3.zero, scaleVector),  // FaceE_Left (-5, 5, 5)
            new TransformState(scaledFaceCenter + new Vector3(0f, 5f, 0f), Vector3.zero, scaleVector),   // FaceH_Top (0, 10, 5)
            new TransformState(scaledFaceCenter + new Vector3(0f, -5f, 0f), Vector3.zero, scaleVector),  // FaceF_Bottom (0, 0, 5)
            new TransformState(scaledFaceCenter + new Vector3(10f, 0f, 0f), Vector3.zero, scaleVector)   // FaceG_Far (10, 5, 5)
        };
    }

    private TransformState[] CalculateScaledFoldedFaces(Vector3 baseA)
    {
        Vector3 scaledFoldedCenter = new Vector3(baseA.x * scaleMultiplier + 2.5f, baseA.y - 2f + 2.5f, 2.5f);
        Vector3 scaleVector = Vector3.one * scaleMultiplier;
        
        return new TransformState[]
        {
            new TransformState(scaledFoldedCenter, Vector3.zero, scaleVector),                          // FaceA_Center (0, 5, 2.5)
            new TransformState(scaledFoldedCenter + new Vector3(2.5f, 0f, 2.5f), new Vector3(0, 90, 0), scaleVector),   // FaceB_Right
            new TransformState(scaledFoldedCenter + new Vector3(-2.5f, 0f, 2.5f), new Vector3(0, -90, 0), scaleVector), // FaceE_Left
            new TransformState(scaledFoldedCenter + new Vector3(0f, 2.5f, 2.5f), new Vector3(-90, 0, 0), scaleVector),  // FaceH_Top
            new TransformState(scaledFoldedCenter + new Vector3(0f, -2.5f, 2.5f), new Vector3(90, 0, 0), scaleVector),  // FaceF_Bottom
            new TransformState(scaledFoldedCenter + new Vector3(0f, 0f, 5f), Vector3.zero, scaleVector)                // FaceG_Far (0, 5, 7.5)
        };
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 計算兩個狀態之間的插值
    /// </summary>
    public CubeConfiguration LerpBetweenStates(CubeState fromState, CubeState toState, 
        Vector3 referencePointA, float t)
    {
        CubeConfiguration from = GetCubeConfiguration(fromState, referencePointA);
        CubeConfiguration to = GetCubeConfiguration(toState, referencePointA);
        
        Vector3[] lerpedVertices = new Vector3[8];
        TransformState[] lerpedFaces = new TransformState[6];
        
        // 插值頂點位置
        for (int i = 0; i < 8; i++)
        {
            lerpedVertices[i] = Vector3.Lerp(from.vertexPositions[i], to.vertexPositions[i], t);
        }
        
        // 插值面變換
        for (int i = 0; i < 6; i++)
        {
            lerpedFaces[i] = new TransformState(
                Vector3.Lerp(from.faceStates[i].position, to.faceStates[i].position, t),
                Vector3.Lerp(from.faceStates[i].rotation, to.faceStates[i].rotation, t),
                Vector3.Lerp(from.faceStates[i].scale, to.faceStates[i].scale, t)
            );
        }
        
        return new CubeConfiguration(lerpedVertices, lerpedFaces);
    }

    /// <summary>
    /// 驗證計算結果的準確性
    /// </summary>
    public void ValidateCalculations()
    {
        Vector3 testBaseA = new Vector3(-0.5f, 4.5f, 0f);
        
        Debug.Log("=== 座標計算驗證 ===");
        
        // 驗證正常展開狀態
        Vector3[] normalUnfolded = CalculateNormalUnfoldedVertices(testBaseA);
        Debug.Log($"正常展開 A: {normalUnfolded[0]} (預期: (-0.5, 4.5, 0))");
        
        // 驗證放大展開狀態
        Vector3[] scaledUnfolded = CalculateScaledUnfoldedVertices(testBaseA);
        Debug.Log($"放大展開 A: {scaledUnfolded[0]} (預期: (-2.5, 2.5, 5))");
        
        // 驗證放大折疊狀態
        Vector3[] scaledFolded = CalculateScaledFoldedVertices(testBaseA);
        Debug.Log($"放大折疊 A: {scaledFolded[0]} (預期: (-2.5, 2.5, 2.5))");
    }

    #endregion

    #region Unity生命周期

    void Start()
    {
        if (enableDebugLog)
        {
            ValidateCalculations();
        }
    }

    #endregion
}