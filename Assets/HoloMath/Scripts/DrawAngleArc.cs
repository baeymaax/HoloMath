using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawAngleArc : MonoBehaviour
{
    public float radius = 0.5f;
    [Range(0, 360)] public float angleDegree = 60f;
    public int segments = 100; // 增加細節
    public Vector3 center = Vector3.zero; // 弧心（可放在 Cylinder 上方）

    void Start()
    {
        DrawArc();
    }

    void DrawArc()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = segments + 1;
        lr.useWorldSpace = false; // 繪製在本地座標系內
        lr.widthMultiplier = 0.01f;

        float angleRad = Mathf.Deg2Rad * angleDegree;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float currentAngle = t * angleRad;
            float x = Mathf.Cos(currentAngle) * radius;
            float z = Mathf.Sin(currentAngle) * radius;
            lr.SetPosition(i, new Vector3(x, 0, z) + center); // 圖形可調整
        }
    }
}
