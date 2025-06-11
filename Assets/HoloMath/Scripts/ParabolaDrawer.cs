using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ParabolaDrawer : MonoBehaviour
{
    [Header("二次參數")]
    public float a = 1, b = 0, c = 0;
    [Header("繪製設定")]
    public int resolution = 50;    // 點數
    public float xMin = -5f, xMax = 5f;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = resolution + 1;
    }

    public void DrawParabola()
    {
        float dx = (xMax - xMin) / resolution;
        for (int i = 0; i <= resolution; i++)
        {
            float x = xMin + dx * i;
            float y = a * x * x + b * x + c;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    // 在 Inspector 變更參數時即時重繪
    #if UNITY_EDITOR
    void OnValidate()
    {
        if (lr != null) DrawParabola();
    }
    #endif
}
