using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class CertificateDisplayUI : MonoBehaviour
{
    [Header("�Ү�UI����")]
    public TextMeshProUGUI certificateIdText;
    public TextMeshProUGUI studentNameText;
    public TextMeshProUGUI courseNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI issueDateText;
    public TextMeshProUGUI issuerText;

    [Header("�϶�������UI")]
    public GameObject verificationPanel;
    public TextMeshProUGUI blockchainHashText;
    public Image verificationStatusIcon;
    public TextMeshProUGUI verificationStatusText;
    public Button verifyButton;
    public Button downloadButton;

    [Header("��ı�ĪG")]
    public Image certificateBorder;
    public Image gradeIcon;
    public ParticleSystem celebrationEffect;
    public AudioSource certificateAudio;
    public AudioClip issuanceSound;

    [Header("�C��]�w")]
    public Color excellentColor = Color.yellow;
    public Color goodColor = Color.green;
    public Color passColor = Color.blue;
    public Color unverifiedColor = Color.gray;

    private Certificate currentCertificate;
    private CertificateManager certificateManager;

    void Start()
    {
        certificateManager = FindAnyObjectByType<CertificateManager>();

        if (verifyButton != null)
            verifyButton.onClick.AddListener(VerifyCertificate);

        if (downloadButton != null)
            downloadButton.onClick.AddListener(DownloadCertificate);
    }

    /// <summary>
    /// �]�m�Ү���ܤ��e
    /// </summary>
    public void SetupCertificate(Certificate certificate)
    {
        currentCertificate = certificate;

        // �򥻸�T���
        UpdateBasicInfo();

        // ���Z��T���
        UpdateScoreInfo();

        // �϶������Ҹ�T
        UpdateBlockchainInfo();

        // ��ı�ĪG
        UpdateVisualEffects();

        // ����{�o�ʵe
        StartCoroutine(PlayIssuanceAnimation());
    }

    void UpdateBasicInfo()
    {
        if (certificateIdText != null)
            certificateIdText.text = $"�Үѽs��: {currentCertificate.certificateId}";

        if (studentNameText != null)
            studentNameText.text = currentCertificate.studentName;

        if (courseNameText != null)
            courseNameText.text = currentCertificate.courseName;

        if (issueDateText != null)
            issueDateText.text = $"�{�o���: {currentCertificate.issuedDate:yyyy�~MM��dd��}";

        if (issuerText != null)
            issuerText.text = $"�{�o���c: {currentCertificate.issuerName}";
    }

    void UpdateScoreInfo()
    {
        if (scoreText != null)
            scoreText.text = $"{currentCertificate.score} / {currentCertificate.maxScore} ({currentCertificate.percentage:F1}%)";

        // �p�ⵥ��
        string grade = CalculateGrade(currentCertificate.percentage);
        currentCertificate.grade = grade;

        if (gradeText != null)
        {
            gradeText.text = grade;
            gradeText.color = GetGradeColor(currentCertificate.percentage);
        }
    }

    void UpdateBlockchainInfo()
    {
        if (verificationPanel != null)
            verificationPanel.SetActive(true);

        if (blockchainHashText != null)
        {
            if (!string.IsNullOrEmpty(currentCertificate.blockchainTxHash))
            {
                string shortHash = currentCertificate.blockchainTxHash.Substring(0, 10) + "...";
                blockchainHashText.text = $"�϶���Hash: {shortHash}";
            }
            else
            {
                blockchainHashText.text = "�|���W��";
            }
        }

        UpdateVerificationStatus();
    }

    void UpdateVerificationStatus()
    {
        if (verificationStatusIcon != null && verificationStatusText != null)
        {
            if (currentCertificate.isVerified && currentCertificate.isOnChain)
            {
                verificationStatusIcon.color = Color.green;
                verificationStatusText.text = "�w����";
                verificationStatusText.color = Color.green;
            }
            else if (currentCertificate.isOnChain)
            {
                verificationStatusIcon.color = Color.yellow;
                verificationStatusText.text = "��������";
                verificationStatusText.color = Color.yellow;
            }
            else
            {
                verificationStatusIcon.color = unverifiedColor;
                verificationStatusText.text = "������";
                verificationStatusText.color = unverifiedColor;
            }
        }
    }

    void UpdateVisualEffects()
    {
        // �ھڦ��Z�]�w����C��
        if (certificateBorder != null)
        {
            certificateBorder.color = GetGradeColor(currentCertificate.percentage);
        }

        // �]�w���Źϥ�
        if (gradeIcon != null)
        {
            gradeIcon.color = GetGradeColor(currentCertificate.percentage);
        }
    }

    string CalculateGrade(float percentage)
    {
        if (percentage >= 95) return "A+";
        if (percentage >= 90) return "A";
        if (percentage >= 85) return "A-";
        if (percentage >= 80) return "B+";
        if (percentage >= 75) return "B";
        if (percentage >= 70) return "B-";
        if (percentage >= 65) return "C+";
        if (percentage >= 60) return "C";
        return "F";
    }

    Color GetGradeColor(float percentage)
    {
        if (percentage >= 90) return excellentColor;
        if (percentage >= 80) return goodColor;
        if (percentage >= 70) return passColor;
        return unverifiedColor;
    }

    /// <summary>
    /// �����Үѹ{�o�ʵe
    /// </summary>
    IEnumerator PlayIssuanceAnimation()
    {
        // ��l�]�w�G�Y��0
        transform.localScale = Vector3.zero;

        // ���񭵮�
        if (certificateAudio != null && issuanceSound != null)
        {
            certificateAudio.PlayOneShot(issuanceSound);
        }

        // �u���Y��ʵe
        float animationTime = 1.0f;
        float time = 0;

        while (time < animationTime)
        {
            time += Time.deltaTime;
            float progress = time / animationTime;

            // �u�ʦ��u
            float scale = ElasticEaseOut(progress);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        transform.localScale = Vector3.one;

        // ����y���S��
        if (celebrationEffect != null)
        {
            celebrationEffect.Play();
        }

        // ������ܰ϶����T
        yield return new WaitForSeconds(0.5f);
        UpdateBlockchainInfo();
    }

    float ElasticEaseOut(float t)
    {
        if (t == 0) return 0;
        if (t == 1) return 1;

        float p = 0.3f;
        float a = 1f;
        float s = p / 4f;

        return a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
    }

    /// <summary>
    /// �����Ү�
    /// </summary>
    public void VerifyCertificate()
    {
        if (certificateManager != null && currentCertificate != null)
        {
            Debug.Log("�}�l�����Ү�...");
            StartCoroutine(certificateManager.VerifyCertificateAsync(currentCertificate));
        }
    }

    /// <summary>
    /// �U���Ү�
    /// </summary>
    public void DownloadCertificate()
    {
        if (currentCertificate != null)
        {
            Debug.Log("�}�l�U���Ү�...");
            StartCoroutine(GenerateCertificatePDF());
        }
    }

    IEnumerator GenerateCertificatePDF()
    {
        // ����PDF�ͦ��L�{
        Debug.Log("���b�ͦ�PDF...");
        yield return new WaitForSeconds(2f);

        string filename = $"Certificate_{currentCertificate.certificateId}.pdf";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);

        Debug.Log($"�ҮѤw�O�s��: {path}");

        // ��ܦ��\�T��
        if (verificationStatusText != null)
        {
            string originalText = verificationStatusText.text;
            verificationStatusText.text = "�U�������I";
            verificationStatusText.color = Color.green;

            yield return new WaitForSeconds(2f);

            verificationStatusText.text = originalText;
            UpdateVerificationStatus();
        }
    }

    /// <summary>
    /// �����ҮѨ����C��
    /// </summary>
    public void ShareCertificate()
    {
        string shareText = $"�ڦb HoloMath ���x�����F�u{currentCertificate.courseName}�v�ҵ{�A" +
                          $"��o�F {currentCertificate.percentage:F1}% �����Z�I" +
                          $"�Ү�ID: {currentCertificate.certificateId}";

        Debug.Log($"���ɤ��e: {shareText}");

        // ������Τ��i�H��X����C��API
        GUIUtility.systemCopyBuffer = shareText;
        Debug.Log("���ɤ��e�w�ƻs��ŶK�O");
    }
}