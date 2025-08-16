using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class CertificateDisplayUI : MonoBehaviour
{
    [Header("證書UI元件")]
    public TextMeshProUGUI certificateIdText;
    public TextMeshProUGUI studentNameText;
    public TextMeshProUGUI courseNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI issueDateText;
    public TextMeshProUGUI issuerText;

    [Header("區塊鏈驗證UI")]
    public GameObject verificationPanel;
    public TextMeshProUGUI blockchainHashText;
    public Image verificationStatusIcon;
    public TextMeshProUGUI verificationStatusText;
    public Button verifyButton;
    public Button downloadButton;

    [Header("視覺效果")]
    public Image certificateBorder;
    public Image gradeIcon;
    public ParticleSystem celebrationEffect;
    public AudioSource certificateAudio;
    public AudioClip issuanceSound;

    [Header("顏色設定")]
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
    /// 設置證書顯示內容
    /// </summary>
    public void SetupCertificate(Certificate certificate)
    {
        currentCertificate = certificate;

        // 基本資訊顯示
        UpdateBasicInfo();

        // 成績資訊顯示
        UpdateScoreInfo();

        // 區塊鏈驗證資訊
        UpdateBlockchainInfo();

        // 視覺效果
        UpdateVisualEffects();

        // 播放頒發動畫
        StartCoroutine(PlayIssuanceAnimation());
    }

    void UpdateBasicInfo()
    {
        if (certificateIdText != null)
            certificateIdText.text = $"證書編號: {currentCertificate.certificateId}";

        if (studentNameText != null)
            studentNameText.text = currentCertificate.studentName;

        if (courseNameText != null)
            courseNameText.text = currentCertificate.courseName;

        if (issueDateText != null)
            issueDateText.text = $"頒發日期: {currentCertificate.issuedDate:yyyy年MM月dd日}";

        if (issuerText != null)
            issuerText.text = $"頒發機構: {currentCertificate.issuerName}";
    }

    void UpdateScoreInfo()
    {
        if (scoreText != null)
            scoreText.text = $"{currentCertificate.score} / {currentCertificate.maxScore} ({currentCertificate.percentage:F1}%)";

        // 計算等級
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
                blockchainHashText.text = $"區塊鏈Hash: {shortHash}";
            }
            else
            {
                blockchainHashText.text = "尚未上鏈";
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
                verificationStatusText.text = "已驗證";
                verificationStatusText.color = Color.green;
            }
            else if (currentCertificate.isOnChain)
            {
                verificationStatusIcon.color = Color.yellow;
                verificationStatusText.text = "等待驗證";
                verificationStatusText.color = Color.yellow;
            }
            else
            {
                verificationStatusIcon.color = unverifiedColor;
                verificationStatusText.text = "未驗證";
                verificationStatusText.color = unverifiedColor;
            }
        }
    }

    void UpdateVisualEffects()
    {
        // 根據成績設定邊框顏色
        if (certificateBorder != null)
        {
            certificateBorder.color = GetGradeColor(currentCertificate.percentage);
        }

        // 設定等級圖示
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
    /// 播放證書頒發動畫
    /// </summary>
    IEnumerator PlayIssuanceAnimation()
    {
        // 初始設定：縮放為0
        transform.localScale = Vector3.zero;

        // 播放音效
        if (certificateAudio != null && issuanceSound != null)
        {
            certificateAudio.PlayOneShot(issuanceSound);
        }

        // 彈性縮放動畫
        float animationTime = 1.0f;
        float time = 0;

        while (time < animationTime)
        {
            time += Time.deltaTime;
            float progress = time / animationTime;

            // 彈性曲線
            float scale = ElasticEaseOut(progress);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        transform.localScale = Vector3.one;

        // 播放慶祝特效
        if (celebrationEffect != null)
        {
            celebrationEffect.Play();
        }

        // 延遲顯示區塊鏈資訊
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
    /// 驗證證書
    /// </summary>
    public void VerifyCertificate()
    {
        if (certificateManager != null && currentCertificate != null)
        {
            Debug.Log("開始驗證證書...");
            StartCoroutine(certificateManager.VerifyCertificateAsync(currentCertificate));
        }
    }

    /// <summary>
    /// 下載證書
    /// </summary>
    public void DownloadCertificate()
    {
        if (currentCertificate != null)
        {
            Debug.Log("開始下載證書...");
            StartCoroutine(GenerateCertificatePDF());
        }
    }

    IEnumerator GenerateCertificatePDF()
    {
        // 模擬PDF生成過程
        Debug.Log("正在生成PDF...");
        yield return new WaitForSeconds(2f);

        string filename = $"Certificate_{currentCertificate.certificateId}.pdf";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);

        Debug.Log($"證書已保存至: {path}");

        // 顯示成功訊息
        if (verificationStatusText != null)
        {
            string originalText = verificationStatusText.text;
            verificationStatusText.text = "下載完成！";
            verificationStatusText.color = Color.green;

            yield return new WaitForSeconds(2f);

            verificationStatusText.text = originalText;
            UpdateVerificationStatus();
        }
    }

    /// <summary>
    /// 分享證書到社交媒體
    /// </summary>
    public void ShareCertificate()
    {
        string shareText = $"我在 HoloMath 平台完成了「{currentCertificate.courseName}」課程，" +
                          $"獲得了 {currentCertificate.percentage:F1}% 的成績！" +
                          $"證書ID: {currentCertificate.certificateId}";

        Debug.Log($"分享內容: {shareText}");

        // 實際應用中可以整合社交媒體API
        GUIUtility.systemCopyBuffer = shareText;
        Debug.Log("分享內容已複製到剪貼板");
    }
}