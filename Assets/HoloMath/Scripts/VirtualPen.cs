using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class VirtualPen : MonoBehaviour, IMixedRealityPointerHandler
{
    public static VirtualPen Instance { get; private set; }

    [Header("Brush Settings")]
    [Tooltip("筆刷預製體，內含 LineRenderer")]
    public GameObject brushPrefab;

    private Color brushColor = Color.black;
    private float brushThickness = 0.01f;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    private IMixedRealityInputSystem InputSystem => CoreServices.InputSystem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    private void OnDisable()
    {
        InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }

    #region IMixedRealityPointerHandler
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        Vector3 hitPos = eventData.Pointer.Result != null
            ? eventData.Pointer.Result.Details.Point
            : eventData.Pointer.Position;
        StartStroke(hitPos);
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (currentLine == null) return;
        Vector3 hitPos = eventData.Pointer.Result != null
            ? eventData.Pointer.Result.Details.Point
            : eventData.Pointer.Position;
        AddPoint(hitPos);
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        currentLine = null;
        points.Clear();
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    #endregion

    private void StartStroke(Vector3 startPosition)
    {
        var brush = Instantiate(brushPrefab);
        currentLine = brush.GetComponent<LineRenderer>();

        // 設定顏色梯度（直接用 brushColor）
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(brushColor, 0f),
                new GradientColorKey(brushColor, 1f)
            },
            new[] {
                new GradientAlphaKey(brushColor.a, 0f),
                new GradientAlphaKey(brushColor.a, 1f)
            }
        );
        currentLine.colorGradient = g;

        // 設定線寬
        currentLine.startWidth = brushThickness;
        currentLine.endWidth = brushThickness;

        // 開始記錄線條
        points.Clear();
        points.Add(startPosition);
        currentLine.positionCount = 1;
        currentLine.SetPosition(0, startPosition);
    }

    private void AddPoint(Vector3 position)
    {
        if (Vector3.Distance(points[points.Count - 1], position) > 0.005f)
        {
            points.Add(position);
            currentLine.positionCount = points.Count;
            currentLine.SetPositions(points.ToArray());
        }
    }

    /// <summary>
    /// UI 呼叫：設定畫筆顏色
    /// </summary>
    public void SetColor(Color color)
    {
        brushColor = color;

        if (currentLine != null)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(brushColor, 0f),
                    new GradientColorKey(brushColor, 1f)
                },
                new[] {
                    new GradientAlphaKey(brushColor.a, 0f),
                    new GradientAlphaKey(brushColor.a, 1f)
                }
            );
            currentLine.colorGradient = g;
        }
    }

    /// <summary>
    /// UI 呼叫：設定畫筆粗細
    /// </summary>
    public void SetThickness(float t)
    {
        brushThickness = t;

        if (currentLine != null)
        {
            currentLine.startWidth = brushThickness;
            currentLine.endWidth = brushThickness;
        }
    }
}
