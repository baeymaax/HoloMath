using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathController : MonoBehaviour
{
    [Header("Curve Parameters")]
    public CurveType curveType = CurveType.Helix;
    public float radius = 2f;
    public float height = 5f;
    public int pointCount = 200;
    public float speed = 1f;
    
    [Header("Animation")]
    public bool animateGeneration = true;
    public float animationSpeed = 2f;
    
    [Header("Visual Settings")]
    public bool useGradientColors = true;
    public Color startColor = Color.cyan;
    public Color endColor = Color.magenta;
    public float lineWidth = 0.05f;
    
    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private float currentT = 0f;
    
    public enum CurveType
    {
        Helix,
        Lissajous,
        TorusKnot,
        Rose3D,
        Spherical,
        Butterfly,
        DNA,
        Mobius,
        Trefoil
    }
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        SetupLineRenderer();
        
        if (!animateGeneration)
        {
            GenerateFullCurve();
        }
    }
    
    void SetupLineRenderer()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        if (useGradientColors)
        {
            // Create gradient from start to end color
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            lineRenderer.colorGradient = gradient;
        }
        else
        {
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = startColor;
        }
        lineRenderer.material = CreateLineMaterial();
    }
    
    Material CreateLineMaterial()
    {
        // Create a simple emissive material for the line
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = startColor;
        return mat;
    }
    
    void Update()
    {
        if (animateGeneration)
        {
            AnimateGeneration();
        }
        
        // Allow real-time parameter changes
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchCurveType();
        }
        
        // Color controls
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeColors();
        }
        
        // Toggle gradient
        if (Input.GetKeyDown(KeyCode.V))
        {
            useGradientColors = !useGradientColors;
            SetupLineRenderer();
        }
    }
    
    void AnimateGeneration()
    {
        currentT += animationSpeed * Time.deltaTime;
        
        Vector3 newPoint = CalculatePoint(currentT);
        points.Add(newPoint);
        
        // Limit points to prevent memory issues
        if (points.Count > pointCount)
        {
            points.RemoveAt(0);
        }
        
        UpdateLineRenderer();
    }
    
    void GenerateFullCurve()
    {
        points.Clear();
        
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / pointCount * 4 * Mathf.PI;
            points.Add(CalculatePoint(t));
        }
        
        UpdateLineRenderer();
    }
    
    Vector3 CalculatePoint(float t)
    {
        switch (curveType)
        {
            case CurveType.Helix:
                return new Vector3(
                    radius * Mathf.Cos(t),
                    height * t / (2 * Mathf.PI),
                    radius * Mathf.Sin(t)
                );
                
            case CurveType.Lissajous:
                return new Vector3(
                    radius * Mathf.Sin(2 * t),
                    radius * Mathf.Sin(3 * t + Mathf.PI/4),
                    radius * Mathf.Sin(t)
                );
                
            case CurveType.TorusKnot:
                float R = 3f, r = 1f, p = 2f, q = 3f;
                return new Vector3(
                    (R + r * Mathf.Cos(q * t)) * Mathf.Cos(p * t),
                    (R + r * Mathf.Cos(q * t)) * Mathf.Sin(p * t),
                    r * Mathf.Sin(q * t)
                );
                
            case CurveType.Rose3D:
                float k = 3f; // petal count
                float rho = radius * Mathf.Cos(k * t);
                return new Vector3(
                    rho * Mathf.Cos(t),
                    rho * Mathf.Sin(t),
                    radius * Mathf.Sin(2 * t) * 0.5f
                );
                
            case CurveType.Spherical:
                return new Vector3(
                    radius * Mathf.Sin(t) * Mathf.Cos(3 * t),
                    radius * Mathf.Sin(t) * Mathf.Sin(3 * t),
                    radius * Mathf.Cos(t)
                );
                
            case CurveType.Butterfly:
                float scale = radius * 0.5f;
                return new Vector3(
                    scale * Mathf.Sin(t) * (Mathf.Exp(Mathf.Cos(t)) - 2 * Mathf.Cos(4 * t) - Mathf.Pow(Mathf.Sin(t / 12), 5)),
                    scale * Mathf.Cos(t) * (Mathf.Exp(Mathf.Cos(t)) - 2 * Mathf.Cos(4 * t) - Mathf.Pow(Mathf.Sin(t / 12), 5)),
                    scale * Mathf.Sin(2 * t)
                );
                
            case CurveType.DNA:
                float helixRadius = radius * 0.8f;
                float helixHeight = height * 0.3f;
                return new Vector3(
                    helixRadius * Mathf.Cos(t) + 0.3f * Mathf.Cos(3 * t),
                    helixHeight * t / (2 * Mathf.PI),
                    helixRadius * Mathf.Sin(t) + 0.3f * Mathf.Sin(3 * t)
                );
                
            case CurveType.Mobius:
                float u = t;
                float v = Mathf.Sin(2 * t) * 0.5f;
                return new Vector3(
                    (radius + v * Mathf.Cos(u / 2)) * Mathf.Cos(u),
                    (radius + v * Mathf.Cos(u / 2)) * Mathf.Sin(u),
                    v * Mathf.Sin(u / 2)
                );
                
            case CurveType.Trefoil:
                return new Vector3(
                    radius * (Mathf.Sin(t) + 2 * Mathf.Sin(2 * t)),
                    radius * (Mathf.Cos(t) - 2 * Mathf.Cos(2 * t)),
                    radius * (-Mathf.Sin(3 * t))
                );
                
            default:
                return Vector3.zero;
        }
    }
    
    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
    
    void SwitchCurveType()
    {
        curveType = (CurveType)(((int)curveType + 1) % 9);
        points.Clear();
        currentT = 0f;
        ChangeColorBasedOnCurve();
    }
    
    void ChangeColors()
    {
        // Cycle through predefined color combinations
        Color[] colorSets = {
            Color.cyan, Color.magenta, Color.yellow,
            Color.red, Color.blue, Color.green,
            new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f), // orange, purple
            Color.white, new Color(0f, 1f, 0.5f) // white, mint
        };
        
        int randomIndex = Random.Range(0, colorSets.Length - 1);
        startColor = colorSets[randomIndex];
        endColor = colorSets[randomIndex + 1];
        
        SetupLineRenderer();
    }
    
    void ChangeColorBasedOnCurve()
    {
        // Auto-assign colors based on curve type
        switch (curveType)
        {
            case CurveType.Helix:
                startColor = Color.cyan;
                endColor = Color.blue;
                break;
            case CurveType.Lissajous:
                startColor = Color.red;
                endColor = Color.yellow;
                break;
            case CurveType.TorusKnot:
                startColor = Color.magenta;
                endColor = Color.white;
                break;
            case CurveType.Rose3D:
                startColor = new Color(1f, 0.4f, 0.7f); // pink
                endColor = Color.red;
                break;
            case CurveType.Spherical:
                startColor = Color.green;
                endColor = new Color(0f, 1f, 0.5f); // mint
                break;
            case CurveType.Butterfly:
                startColor = new Color(1f, 0.5f, 0f); // orange
                endColor = new Color(1f, 1f, 0f); // yellow
                break;
            case CurveType.DNA:
                startColor = new Color(0f, 0.8f, 1f); // light blue
                endColor = new Color(0.5f, 0f, 1f); // purple
                break;
            case CurveType.Mobius:
                startColor = Color.white;
                endColor = new Color(0.7f, 0.7f, 0.7f); // light gray
                break;
            case CurveType.Trefoil:
                startColor = new Color(0.2f, 1f, 0.2f); // bright green
                endColor = new Color(0f, 0.5f, 0f); // dark green
                break;
        }
        
        SetupLineRenderer();
    }
}
