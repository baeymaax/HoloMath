using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

public class VRInteractiveAngleConverter : MonoBehaviour
{
    [Header("視覺元件")]
    public Transform unitCircle;
    public LineRenderer arcRenderer;
    public LineRenderer radiusLine;
    public Transform dragHandle;

    [Header("文字顯示")]
    public TextMeshPro degreeText;
    public TextMeshPro radianText;
    public TextMeshPro specialRadianText;

    [Header("視覺設定")]
    public float circleRadius = 1f;
    public Color arcColor = Color.yellow;
    public Color radiusColor = Color.red;
    public Color handleColor = Color.blue;

    [Header("拖拽設定")]
    public float handleSize = 0.1f;

    [Header("位置約束設定")]
    public bool enablePositionConstraint = true;  // 新增：是否啟用位置約束
    public float constraintTolerance = 0.01f;     // 新增：約束容忍度

    private float currentAngle = 0f;
    private Renderer handleRenderer;
    private Vector3 lastValidPosition;
    private ManipulationHandler manipulationHandler;
    private bool isBeingManipulated = false;

    void Start()
    {
        SetupComponents();
        SetupDragHandle();

        // 初始化為0度 (正東方向)
        currentAngle = 0f;
        SetAngle(0f);
        UpdateVisuals();
        UpdateTexts();
    }

    void SetupComponents()
    {
        if (arcRenderer == null)
            arcRenderer = gameObject.AddComponent<LineRenderer>();

        arcRenderer.material = new Material(Shader.Find("Sprites/Default"));
        arcRenderer.material.color = arcColor;
        arcRenderer.startWidth = 0.05f;
        arcRenderer.endWidth = 0.05f;
        arcRenderer.useWorldSpace = true;
        arcRenderer.sortingOrder = 1;

        if (radiusLine == null)
        {
            GameObject radiusObj = new GameObject("RadiusLine");
            radiusObj.transform.SetParent(transform);
            radiusLine = radiusObj.AddComponent<LineRenderer>();
        }

        radiusLine.material = new Material(Shader.Find("Sprites/Default"));
        radiusLine.material.color = radiusColor;
        radiusLine.startWidth = 0.03f;
        radiusLine.endWidth = 0.03f;
        radiusLine.positionCount = 2;
        radiusLine.useWorldSpace = true;
        radiusLine.sortingOrder = 0;
    }

    void SetupDragHandle()
    {
        if (dragHandle == null)
        {
            GameObject handleObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handleObj.name = "DragHandle";
            handleObj.transform.SetParent(transform);
            handleObj.transform.localScale = Vector3.one * handleSize;
            dragHandle = handleObj.transform;

            handleRenderer = handleObj.GetComponent<Renderer>();
            handleRenderer.material = new Material(Shader.Find("Standard"));
            handleRenderer.material.color = handleColor;

            // 確保有Collider
            if (handleObj.GetComponent<Collider>() == null)
            {
                handleObj.AddComponent<SphereCollider>();
            }

            // 加入MRTK組件
            var touchable = handleObj.AddComponent<NearInteractionTouchable>();
            touchable.EventsToReceive = TouchableEventType.Touch;

            manipulationHandler = handleObj.AddComponent<ManipulationHandler>();
            manipulationHandler.HostTransform = dragHandle;
            manipulationHandler.ManipulationType = ManipulationHandler.HandMovementType.OneHandedOnly;
            manipulationHandler.AllowFarManipulation = true;
            manipulationHandler.SmoothingActive = true;

            // 監聽拖拽事件
            manipulationHandler.OnManipulationStarted.AddListener(OnManipulationStarted);
            manipulationHandler.OnManipulationEnded.AddListener(OnManipulationEnded);
        }
        else
        {
            handleRenderer = dragHandle.GetComponent<Renderer>();
            if (handleRenderer == null)
                Debug.LogWarning("DragHandle needs a Renderer component!");
        }
    }

    void Update()
    {
        // 檢測拖拽中的位置變化
        if (dragHandle != null)
        {
            Vector3 currentPos = dragHandle.localPosition;
            Vector3 direction = new Vector3(0, currentPos.y, currentPos.z);

            if (direction.magnitude > 0.01f)
            {
                // 可選的位置約束到YZ平面的圓周上
                if (enablePositionConstraint)
                {
                    direction = direction.normalized;
                    Vector3 constrainedPos = direction * circleRadius;

                    // 如果位置偏離圓周，則修正
                    if (Vector3.Distance(currentPos, constrainedPos) > constraintTolerance)
                    {
                        dragHandle.localPosition = constrainedPos;
                        currentPos = constrainedPos;
                    }
                }

                // 重新計算角度 - 修正角度計算
                // 使用標準數學座標系：正東方(0,0,-1)為0度，逆時針增加
                float angle = Mathf.Atan2(currentPos.y, -currentPos.z) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                if (Mathf.Abs(angle - currentAngle) > 0.1f)
                {
                    UpdateAngle(angle);
                }
            }
        }
    }

    void OnManipulationStarted(ManipulationEventData eventData)
    {
        Debug.Log("開始拖拽");
        isBeingManipulated = true;

        // 變更顏色提示拖拽開始
        if (handleRenderer != null)
        {
            handleRenderer.material.color = Color.green;
        }
    }

    void OnManipulationEnded(ManipulationEventData eventData)
    {
        Debug.Log("結束拖拽");
        isBeingManipulated = false;

        // 恢復原本顏色
        if (handleRenderer != null)
        {
            handleRenderer.material.color = handleColor;
        }

        // 可選的最終位置約束
        if (enablePositionConstraint)
        {
            // 確保最終位置在YZ平面圓周上
            Vector3 currentPos = dragHandle.localPosition;
            Vector3 direction = new Vector3(0, currentPos.y, currentPos.z).normalized;
            dragHandle.localPosition = direction * circleRadius;

            // 重新計算角度
            float angle = Mathf.Atan2(direction.y, -direction.z) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            UpdateAngle(angle);
        }
        else
        {
            // 不強制約束，只更新角度
            Vector3 currentPos = dragHandle.localPosition;
            Vector3 direction = new Vector3(0, currentPos.y, currentPos.z);
            if (direction.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(currentPos.y, -currentPos.z) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;
                UpdateAngle(angle);
            }
        }
    }

    void UpdateAngle(float angle)
    {
        currentAngle = angle;
        lastValidPosition = dragHandle.localPosition;
        UpdateVisuals();
        UpdateTexts();
    }

    void UpdateVisuals()
    {
        // 根據角度計算位置 - 修正座標計算
        // 0度對應正東方向(0,0,-1)
        float angleRad = currentAngle * Mathf.Deg2Rad;
        float y = Mathf.Sin(angleRad) * circleRadius;
        float z = -Mathf.Cos(angleRad) * circleRadius;

        // 使用世界座標位置
        Vector3 centerWorld = transform.position;
        Vector3 handleWorld = transform.TransformPoint(new Vector3(0, y, z));

        radiusLine.SetPosition(0, centerWorld);
        radiusLine.SetPosition(1, handleWorld);

        DrawArc(currentAngle);
    }

    void DrawArc(float angle)
    {
        if (Mathf.Approximately(angle, 0f))
        {
            arcRenderer.positionCount = 0;
            return;
        }

        int segments = Mathf.Max(20, Mathf.RoundToInt(angle / 5f));
        List<Vector3> points = new List<Vector3>();
        float arcRadius = circleRadius * 0.8f;

        // 從0度開始畫到當前角度，使用世界座標
        for (int i = 0; i <= segments; i++)
        {
            float current = (angle * i) / segments;
            float currentRad = current * Mathf.Deg2Rad;
            float y = Mathf.Sin(currentRad) * arcRadius;
            float z = -Mathf.Cos(currentRad) * arcRadius;

            // 轉換為世界座標
            Vector3 localPos = new Vector3(0, y, z);
            Vector3 worldPos = transform.TransformPoint(localPos);
            points.Add(worldPos);
        }

        arcRenderer.positionCount = points.Count;
        arcRenderer.SetPositions(points.ToArray());
    }

    void UpdateTexts()
    {
        if (degreeText != null)
            degreeText.text = $"{currentAngle:F1}°";

        if (radianText != null)
        {
            float radians = currentAngle * Mathf.Deg2Rad;
            string piExpression = GetPiExpression(currentAngle);

            if (!string.IsNullOrEmpty(piExpression))
            {
                radianText.text = $"{radians:F3} rad\n({piExpression})";
            }
            else
            {
                radianText.text = $"{radians:F3} rad\n({radians / Mathf.PI:F2}π)";
            }
        }

        if (specialRadianText != null)
        {
            string special = GetSpecialRadianForm(currentAngle);
            specialRadianText.text = string.IsNullOrEmpty(special) ? "" : $"{special} rad";
        }
    }

    string GetPiExpression(float degrees)
    {
        float tol = 2f;
        if (Mathf.Abs(degrees - 0f) < tol) return "0";
        if (Mathf.Abs(degrees - 30f) < tol) return "π/6";
        if (Mathf.Abs(degrees - 45f) < tol) return "π/4";
        if (Mathf.Abs(degrees - 60f) < tol) return "π/3";
        if (Mathf.Abs(degrees - 90f) < tol) return "π/2";
        if (Mathf.Abs(degrees - 120f) < tol) return "2π/3";
        if (Mathf.Abs(degrees - 135f) < tol) return "3π/4";
        if (Mathf.Abs(degrees - 150f) < tol) return "5π/6";
        if (Mathf.Abs(degrees - 180f) < tol) return "π";
        if (Mathf.Abs(degrees - 210f) < tol) return "7π/6";
        if (Mathf.Abs(degrees - 225f) < tol) return "5π/4";
        if (Mathf.Abs(degrees - 240f) < tol) return "4π/3";
        if (Mathf.Abs(degrees - 270f) < tol) return "3π/2";
        if (Mathf.Abs(degrees - 300f) < tol) return "5π/3";
        if (Mathf.Abs(degrees - 315f) < tol) return "7π/4";
        if (Mathf.Abs(degrees - 330f) < tol) return "11π/6";
        if (Mathf.Abs(degrees - 360f) < tol) return "2π";
        return "";
    }

    string GetSpecialRadianForm(float degrees)
    {
        float tol = 2f;
        if (Mathf.Abs(degrees - 0f) < tol) return "0";
        if (Mathf.Abs(degrees - 30f) < tol) return "π/6";
        if (Mathf.Abs(degrees - 45f) < tol) return "π/4";
        if (Mathf.Abs(degrees - 60f) < tol) return "π/3";
        if (Mathf.Abs(degrees - 90f) < tol) return "π/2";
        if (Mathf.Abs(degrees - 120f) < tol) return "2π/3";
        if (Mathf.Abs(degrees - 135f) < tol) return "3π/4";
        if (Mathf.Abs(degrees - 150f) < tol) return "5π/6";
        if (Mathf.Abs(degrees - 180f) < tol) return "π";
        if (Mathf.Abs(degrees - 210f) < tol) return "7π/6";
        if (Mathf.Abs(degrees - 225f) < tol) return "5π/4";
        if (Mathf.Abs(degrees - 240f) < tol) return "4π/3";
        if (Mathf.Abs(degrees - 270f) < tol) return "3π/2";
        if (Mathf.Abs(degrees - 300f) < tol) return "5π/3";
        if (Mathf.Abs(degrees - 315f) < tol) return "7π/4";
        if (Mathf.Abs(degrees - 330f) < tol) return "11π/6";
        if (Mathf.Abs(degrees - 360f) < tol) return "2π";
        return "";
    }

    // 修改SetAngle方法，增加可選的強制設定
    public void SetAngle(float angle, bool forcePosition = true)
    {
        while (angle < 0) angle += 360f;
        while (angle >= 360f) angle -= 360f;

        if (forcePosition)
        {
            // 修正位置計算 - 0度對應正東方向
            float angleRad = angle * Mathf.Deg2Rad;
            float y = Mathf.Sin(angleRad) * circleRadius;
            float z = -Mathf.Cos(angleRad) * circleRadius;
            dragHandle.localPosition = new Vector3(0, y, z);
            lastValidPosition = dragHandle.localPosition;
        }

        UpdateAngle(angle);
    }

    // 新增方法：切換位置約束
    public void SetPositionConstraint(bool enable)
    {
        enablePositionConstraint = enable;
    }

    // 新增方法：設定約束容忍度
    public void SetConstraintTolerance(float tolerance)
    {
        constraintTolerance = tolerance;
    }

    public float GetCurrentAngle() => currentAngle;
    public float GetCurrentRadians() => currentAngle * Mathf.Deg2Rad;
}