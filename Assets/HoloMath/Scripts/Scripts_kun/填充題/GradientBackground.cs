using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class GradientBackground : MonoBehaviour
{
    public Color topColor = new Color(0.98f, 0.98f, 0.99f, 1f);    // ´X¥G¥Õ¦â
    public Color bottomColor = new Color(0.92f, 0.92f, 0.94f, 1f); // ²L¦Ç

    void Start()
    {
        GenerateGradient();
    }

    void GenerateGradient()
    {
        Texture2D texture = new Texture2D(1, 256);
        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            Color color = Color.Lerp(bottomColor, topColor, t);
            texture.SetPixel(0, i, color);
        }
        texture.Apply();

        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            rawImage = gameObject.AddComponent<RawImage>();
        }
        rawImage.texture = texture;
    }
}