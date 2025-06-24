using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysShowHighlight : MonoBehaviour
{
    public GameObject highlightPlate;

    void Start()
    {
        StartCoroutine(ForceEnableAfterDelay());
    }

    IEnumerator ForceEnableAfterDelay()
    {
        // 等待一幀，確保 MRTK 初始化完成
        yield return null;

        if (highlightPlate != null)
        {
            var renderer = highlightPlate.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }

            var meshRenderer = highlightPlate.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            // 額外保險：強制 GameObject 整體為啟用狀態
            highlightPlate.SetActive(true);
        }
    }
    void Update()
    {
        if (highlightPlate != null)
        {
            var renderer = highlightPlate.GetComponent<MeshRenderer>();
            if (renderer != null && !renderer.enabled)
            {
                renderer.enabled = true;
            }
        }
    }

}
