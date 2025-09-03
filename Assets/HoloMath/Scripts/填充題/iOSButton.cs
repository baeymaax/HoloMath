using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class iOSButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private Image image;
    private Vector3 originalScale;

    [Header("iOS Button Settings")]
    public Color normalColor = new Color(0.0f, 0.478f, 1.0f); // iOS 藍
    public Color pressedColor = new Color(0.0f, 0.4f, 0.9f);
    public float pressScale = 0.95f;
    public float animDuration = 0.1f;

    void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        originalScale = transform.localScale;

        // 設定按鈕顏色
        image.color = normalColor;

        // 設定圓角
        // 這裡需要圓角圖片或 Shader
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimatePress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateRelease());
    }

    IEnumerator AnimatePress()
    {
        float elapsed = 0;
        Vector3 startScale = transform.localScale;
        Color startColor = image.color;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;

            transform.localScale = Vector3.Lerp(startScale, originalScale * pressScale, t);
            image.color = Color.Lerp(startColor, pressedColor, t);

            yield return null;
        }
    }

    IEnumerator AnimateRelease()
    {
        float elapsed = 0;
        Vector3 startScale = transform.localScale;
        Color startColor = image.color;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;

            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            image.color = Color.Lerp(startColor, normalColor, t);

            yield return null;
        }
    }
}