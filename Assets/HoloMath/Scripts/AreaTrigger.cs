using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public AreaTextController areaText;

    public void ShowTriangleArea()
    {
        float baseLength = 4f;
        float height = 3f;
        bool isTriangle = true;

        areaText.UpdateArea(baseLength, height, isTriangle);
    }

    public void ShowRectangleArea()
    {
        float baseLength = 4f;
        float height = 3f;
        bool isTriangle = false;

        areaText.UpdateArea(baseLength, height, isTriangle);
    }
}