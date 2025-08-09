using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnhancedMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("��ı�ĪG")]
    public GameObject hoverEffect;
    public AudioClip hoverSound;

    private AudioSource audioSource;
    private Vector3 originalScale;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���s��j�ĪG
        StartCoroutine(ScaleAnimation(originalScale * 1.1f));

        // ����a���ĪG
        if (hoverEffect != null)
            hoverEffect.SetActive(true);

        // ���񭵮�
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // �٭�j�p
        StartCoroutine(ScaleAnimation(originalScale));

        // �����a���ĪG
        if (hoverEffect != null)
            hoverEffect.SetActive(false);
    }

    System.Collections.IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        Vector3 currentScale = transform.localScale;
        float time = 0;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}