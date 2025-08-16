using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public class DigitalSignatureService : MonoBehaviour
{
    [Header("簽章設定")]
    public string institutionName = "HoloMath Education Platform";
    public string institutionId = "HOLO_MATH_001";

    private const string PRIVATE_KEY = "HOLOMATH_PRIVATE_KEY_2025"; // 實際應用中應該更安全地存儲

    /// <summary>
    /// 為證書生成數位簽章
    /// </summary>
    public string GenerateDigitalSignature(Certificate certificate)
    {
        try
        {
            // 1. 創建證書的標準化JSON字符串
            string certificateData = CreateSignatureData(certificate);

            // 2. 生成SHA-256 hash
            string hash = GenerateSHA256Hash(certificateData);

            // 3. 加上機構簽名
            string digitalSignature = SignWithInstitutionKey(hash);

            Debug.Log($"數位簽章生成成功: {digitalSignature.Substring(0, 16)}...");
            return digitalSignature;
        }
        catch (Exception e)
        {
            Debug.LogError($"數位簽章生成失敗: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 創建用於簽章的標準化數據
    /// </summary>
    string CreateSignatureData(Certificate certificate)
    {
        var signatureData = new
        {
            certificateId = certificate.certificateId,
            studentId = certificate.studentId,
            courseName = certificate.courseName,
            examId = certificate.examId,
            score = certificate.score,
            maxScore = certificate.maxScore,
            percentage = certificate.percentage,
            issuedDate = certificate.issuedDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            issuerName = certificate.issuerName,
            institutionId = institutionId
        };

        return JsonConvert.SerializeObject(signatureData, Formatting.None);
    }

    /// <summary>
    /// 生成SHA-256 hash值
    /// </summary>
    string GenerateSHA256Hash(string input)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// 使用機構私鑰簽名
    /// </summary>
    string SignWithInstitutionKey(string hash)
    {
        // 簡化版本的簽名，實際應用中應使用RSA或ECDSA
        string combinedData = $"{PRIVATE_KEY}:{hash}:{institutionId}";
        return GenerateSHA256Hash(combinedData);
    }

    /// <summary>
    /// 驗證數位簽章
    /// </summary>
    public bool VerifyDigitalSignature(Certificate certificate, string signature)
    {
        try
        {
            string expectedSignature = GenerateDigitalSignature(certificate);
            bool isValid = signature == expectedSignature;

            Debug.Log($"簽章驗證結果: {(isValid ? "有效" : "無效")}");
            return isValid;
        }
        catch (Exception e)
        {
            Debug.LogError($"簽章驗證失敗: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 生成證書的唯一ID
    /// </summary>
    public string GenerateCertificateId(string studentId, string examId)
    {
        string data = $"{studentId}:{examId}:{DateTime.Now.Ticks}";
        string hash = GenerateSHA256Hash(data);
        return $"CERT_{hash.Substring(0, 12).ToUpper()}";
    }
}