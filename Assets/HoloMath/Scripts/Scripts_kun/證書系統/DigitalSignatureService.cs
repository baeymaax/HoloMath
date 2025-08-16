using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public class DigitalSignatureService : MonoBehaviour
{
    [Header("ñ���]�w")]
    public string institutionName = "HoloMath Education Platform";
    public string institutionId = "HOLO_MATH_001";

    private const string PRIVATE_KEY = "HOLOMATH_PRIVATE_KEY_2025"; // ������Τ����ӧ�w���a�s�x

    /// <summary>
    /// ���Үѥͦ��Ʀ�ñ��
    /// </summary>
    public string GenerateDigitalSignature(Certificate certificate)
    {
        try
        {
            // 1. �Ы��ҮѪ��зǤ�JSON�r�Ŧ�
            string certificateData = CreateSignatureData(certificate);

            // 2. �ͦ�SHA-256 hash
            string hash = GenerateSHA256Hash(certificateData);

            // 3. �[�W���cñ�W
            string digitalSignature = SignWithInstitutionKey(hash);

            Debug.Log($"�Ʀ�ñ���ͦ����\: {digitalSignature.Substring(0, 16)}...");
            return digitalSignature;
        }
        catch (Exception e)
        {
            Debug.LogError($"�Ʀ�ñ���ͦ�����: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// �ЫإΩ�ñ�����зǤƼƾ�
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
    /// �ͦ�SHA-256 hash��
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
    /// �ϥξ��c�p�_ñ�W
    /// </summary>
    string SignWithInstitutionKey(string hash)
    {
        // ²�ƪ�����ñ�W�A������Τ����ϥ�RSA��ECDSA
        string combinedData = $"{PRIVATE_KEY}:{hash}:{institutionId}";
        return GenerateSHA256Hash(combinedData);
    }

    /// <summary>
    /// ���ҼƦ�ñ��
    /// </summary>
    public bool VerifyDigitalSignature(Certificate certificate, string signature)
    {
        try
        {
            string expectedSignature = GenerateDigitalSignature(certificate);
            bool isValid = signature == expectedSignature;

            Debug.Log($"ñ�����ҵ��G: {(isValid ? "����" : "�L��")}");
            return isValid;
        }
        catch (Exception e)
        {
            Debug.LogError($"ñ�����ҥ���: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// �ͦ��ҮѪ��ߤ@ID
    /// </summary>
    public string GenerateCertificateId(string studentId, string examId)
    {
        string data = $"{studentId}:{examId}:{DateTime.Now.Ticks}";
        string hash = GenerateSHA256Hash(data);
        return $"CERT_{hash.Substring(0, 12).ToUpper()}";
    }
}