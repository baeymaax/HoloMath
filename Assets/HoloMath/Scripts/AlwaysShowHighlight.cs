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
        // ���ݤ@�V�A�T�O MRTK ��l�Ƨ���
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

            // �B�~�O�I�G�j�� GameObject ���鬰�ҥΪ��A
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
