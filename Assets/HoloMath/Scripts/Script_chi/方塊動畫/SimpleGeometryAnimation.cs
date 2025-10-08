using UnityEngine;

public class SimpleGeometryAnimation : MonoBehaviour
{
    [Header("要動畫的物件")]
    public Transform targetObject;

    [Header("動畫設定")]
    public float rotationSpeed = 50f;
    public float scaleSpeed = 1f;
    public float moveDistance = 2f;

    private bool isAnimating = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    void Start()
    {
        if (targetObject != null)
        {
            originalPosition = targetObject.position;
            originalScale = targetObject.localScale;
        }
    }

    void Update()
    {
        if (isAnimating && targetObject != null)
        {
            targetObject.Rotate(0, rotationSpeed * Time.deltaTime, 0);

            float scale = 1 + Mathf.Sin(Time.time * scaleSpeed) * 0.3f;
            targetObject.localScale = originalScale * scale;

            float yOffset = Mathf.Sin(Time.time * 2f) * moveDistance;
            targetObject.position = originalPosition + Vector3.up * yOffset;
        }
    }

    public void ToggleAnimation()
    {
        isAnimating = !isAnimating;

        if (!isAnimating)
        {
            targetObject.position = originalPosition;
            targetObject.localScale = originalScale;
        }
    }
}