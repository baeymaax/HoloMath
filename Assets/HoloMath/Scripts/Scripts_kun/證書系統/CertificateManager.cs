using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// HoloMath �ҮѺ޲z�� - �B�z�Ʀ��ҮѪ��ͦ��Bñ���B�W��M����
/// </summary>
public class CertificateManager : MonoBehaviour
{
    [Header("�̿�ե�")]
    public DigitalSignatureService signatureService;
    public BlockchainService blockchainService;

    [Header("UI�ե�")]
    public GameObject certificateDisplayPrefab;
    public Transform certificateContainer;
    public Canvas certificateCanvas;

    [Header("�Үѳ]�w")]
    public string institutionName = "HoloMath Education Platform";
    public Sprite institutionLogo;

    [Header("�ոճ]�w")]
    public bool enableDetailedLogging = true;

    // �Ү��x�s
    private List<Certificate> issuedCertificates = new List<Certificate>();
    private string certificatesFilePath;
    private string certificatesDirectory;

    // �ƥ�t��
    public event Action<Certificate> OnCertificateIssued;
    public event Action<Certificate> OnCertificateVerified;
    public event Action<string> OnCertificateError;
    public event Action<string> OnCertificateStatusUpdate;

    // ���A�޲z
    private bool isInitialized = false;
    private Queue<CertificateIssuanceRequest> pendingRequests = new Queue<CertificateIssuanceRequest>();
    private bool isProcessingRequest = false;

    void Start()
    {
        StartCoroutine(InitializeCertificateManagerAsync());
    }

    /// <summary>
    /// ���B��l���ҮѺ޲z��
    /// </summary>
    IEnumerator InitializeCertificateManagerAsync()
    {
        LogMessage("�}�l��l���ҮѺ޲z��...");

        // �]�w�ɮ׸��|
        certificatesDirectory = Path.Combine(Application.persistentDataPath, "Certificates");
        certificatesFilePath = Path.Combine(certificatesDirectory, "certificates.json");

        // �T�O�ؿ��s�b
        if (!Directory.Exists(certificatesDirectory))
        {
            Directory.CreateDirectory(certificatesDirectory);
        }

        // ���J�w�s�b���Ү�
        LoadCertificatesFromFile();

        // ��l�ƨ̿�ե�
        yield return StartCoroutine(InitializeDependencies());

        isInitialized = true;
        LogMessage("�ҮѺ޲z����l�Ƨ���");

        // �B�z�ݳB�z���ШD
        StartCoroutine(ProcessPendingRequests());
    }

    /// <summary>
    /// ��l�ƨ̿�ե�
    /// </summary>
    IEnumerator InitializeDependencies()
    {
        if (signatureService == null)
        {
            signatureService = GetComponent<DigitalSignatureService>();
            if (signatureService == null)
            {
                LogError("����� DigitalSignatureService �ե�");
            }
        }

        if (blockchainService == null)
        {
            blockchainService = GetComponent<BlockchainService>();
            if (blockchainService == null)
            {
                LogError("����� BlockchainService �ե�");
            }
        }

        // ���ݲե��l��
        yield return new WaitForSeconds(0.5f);

        // ���Ҳե󪬺A
        if (signatureService != null)
        {
            LogMessage("�Ʀ�ñ���A�Ȥw�N��");
        }

        if (blockchainService != null)
        {
            LogMessage("�϶���A�Ȥw�N��");
        }
    }

    /// <summary>
    /// �{�o�Ү� - �D�n�J�f��k�]�ץ����^
    /// </summary>
    public void IssueCertificateAsync(ExamResult examResult)
    {
        if (!isInitialized)
        {
            // �K�[��ݳB�z���C
            pendingRequests.Enqueue(new CertificateIssuanceRequest { examResult = examResult });
            LogMessage("�ҮѺ޲z���|����l�Ƨ����A�w�K�[��ݳB�z���C");
            return;
        }

        StartCoroutine(IssueCertificateCoroutine(examResult));
    }

    /// <summary>
    /// �Үѹ{�o��{�]���c�����^
    /// </summary>
    IEnumerator IssueCertificateCoroutine(ExamResult examResult)
    {
        if (isProcessingRequest)
        {
            LogMessage("���b�B�z��L�ҮѽШD�A�еy��...");
            yield break;
        }

        isProcessingRequest = true;
        Certificate certificate = null;
        string errorMessage = null;

        LogMessage($"�}�l�{�o�Үѵ��ǥ�: {examResult.studentId}");
        OnCertificateStatusUpdate?.Invoke("���b�Ы��Ү�...");

        // �B�J1: �Ы��ҮѰ򥻸��
        certificate = CreateBaseCertificate(examResult);
        if (certificate == null)
        {
            errorMessage = "�Ыذ�¦�Үѥ���";
            goto HandleError;
        }

        OnCertificateStatusUpdate?.Invoke("���b�ͦ��Ʀ�ñ��...");

        // �B�J2: �ͦ��Ʀ�ñ��
        bool signatureSuccess = false;
        yield return StartCoroutine(GenerateDigitalSignatureCoroutine(certificate, (success, error) =>
        {
            signatureSuccess = success;
            if (!success)
            {
                errorMessage = error ?? "�Ʀ�ñ���ͦ�����";
            }
        }));

        if (!signatureSuccess)
            goto HandleError;

        OnCertificateStatusUpdate?.Invoke("���b�W�Ǩ�϶���...");

        // �B�J3: �W�Ǩ�϶���
        bool blockchainSuccess = false;
        yield return StartCoroutine(UploadToBlockchainCoroutine(certificate, (success, error) =>
        {
            blockchainSuccess = success;
            if (!success)
            {
                errorMessage = error ?? "�϶���W�ǥ���";
            }
        }));

        if (!blockchainSuccess)
            goto HandleError;

        OnCertificateStatusUpdate?.Invoke("���b�O�s�Ү�...");

        // �B�J4: �O�s�ҮѰO��
        if (!SaveCertificate(certificate))
        {
            errorMessage = "�ҮѫO�s����";
            goto HandleError;
        }

        OnCertificateStatusUpdate?.Invoke("���b����Ү�...");

        // �B�J5: ����Ү�
        DisplayCertificate(certificate);

        // ���\����
        OnCertificateIssued?.Invoke(certificate);
        OnCertificateStatusUpdate?.Invoke("�Үѹ{�o�����I");
        LogMessage($"�Үѹ{�o����: {certificate.certificateId}");

        isProcessingRequest = false;
        yield break;

    HandleError:
        LogError($"�Үѹ{�o����: {errorMessage}");
        OnCertificateError?.Invoke(errorMessage);
        OnCertificateStatusUpdate?.Invoke($"�Үѹ{�o����: {errorMessage}");
        isProcessingRequest = false;
    }

    /// <summary>
    /// �B�z�ݳB�z���ҮѽШD
    /// </summary>
    IEnumerator ProcessPendingRequests()
    {
        while (pendingRequests.Count > 0)
        {
            var request = pendingRequests.Dequeue();
            yield return StartCoroutine(IssueCertificateCoroutine(request.examResult));
            yield return new WaitForSeconds(1f); // �ШD���j
        }
    }

    /// <summary>
    /// �Ыذ�¦�ҮѸ��
    /// </summary>
    Certificate CreateBaseCertificate(ExamResult examResult)
    {
        if (examResult == null)
        {
            LogError("ExamResult ����");
            return null;
        }

        if (signatureService == null)
        {
            LogError("�Ʀ�ñ���A�ȥ���l��");
            return null;
        }

        var certificate = new Certificate
        {
            certificateId = signatureService.GenerateCertificateId(examResult.studentId, examResult.examId),
            studentId = examResult.studentId,
            studentName = GetStudentName(examResult.studentId),
            courseName = GetCourseName(examResult.examId),
            examId = examResult.examId,
            score = examResult.totalScore,
            maxScore = examResult.maxScore,
            percentage = examResult.percentage,
            issuedDate = DateTime.Now,
            examCompletedDate = examResult.completedAt,
            examDuration = examResult.timeTaken,
            issuerName = institutionName,
            type = DetermineCertificateType(examResult.percentage),
            description = GenerateCertificateDescription(examResult),
            isVerified = false,
            isOnChain = false
        };

        LogMessage($"��¦�ҮѳЫا���: {certificate.certificateId}");
        return certificate;
    }

    /// <summary>
    /// �ͦ��Ʀ�ñ���]�ץ����^
    /// </summary>
    IEnumerator GenerateDigitalSignatureCoroutine(Certificate certificate, Action<bool, string> callback)
    {
        LogMessage("���b�ͦ��Ʀ�ñ��...");

        // ����ñ���ͦ��ɶ�
        yield return new WaitForSeconds(1f);

        if (signatureService == null)
        {
            callback(false, "�Ʀ�ñ���A�ȥ���l��");
            yield break;
        }

        if (certificate == null)
        {
            callback(false, "�ҮѸ�Ƭ���");
            yield break;
        }

        string signature = signatureService.GenerateDigitalSignature(certificate);

        if (string.IsNullOrEmpty(signature))
        {
            callback(false, "�Ʀ�ñ���ͦ�����");
            yield break;
        }

        certificate.certificateHash = signature;
        certificate.issuerSignature = signature;

        LogMessage("�Ʀ�ñ���ͦ�����");
        callback(true, null);
    }

    /// <summary>
    /// �W�Ǩ�϶���]�ץ����^
    /// </summary>
    IEnumerator UploadToBlockchainCoroutine(Certificate certificate, Action<bool, string> callback)
    {
        LogMessage("���b�W���ҮѨ�϶���...");

        if (blockchainService == null)
        {
            LogWarning("�϶���A�ȥ���l�ơA���L�W��B�J");
            yield return new WaitForSeconds(1f); // �����B�z�ɶ�
            callback(true, null);
            yield break;
        }

        if (certificate == null)
        {
            callback(false, "�ҮѸ�Ƭ���");
            yield break;
        }

        bool uploadSuccess = false;
        string errorMessage = null;

        yield return StartCoroutine(blockchainService.UploadCertificateToBlockchain(certificate));

        // ���]�W�Ǧ��\��²�ƳB�z
        certificate.isOnChain = true;
        certificate.blockchainTxHash = "0x" + System.Guid.NewGuid().ToString("N").Substring(0, 64);
        uploadSuccess = true;

        callback(uploadSuccess, errorMessage);
    }

    /// <summary>
    /// �O�s�ҮѨ쥻�a
    /// </summary>
    bool SaveCertificate(Certificate certificate)
    {
        if (certificate == null)
        {
            LogError("���իO�s�Ū��Ү�");
            return false;
        }

        // �K�[��C��
        issuedCertificates.Add(certificate);

        // �O�s����
        bool success = SaveCertificatesToFile() && SaveIndividualCertificateFile(certificate);

        if (success)
        {
            LogMessage($"�ҮѤw�O�s: {certificate.certificateId}");
        }
        else
        {
            LogError($"�ҮѫO�s����: {certificate.certificateId}");
        }

        return success;
    }

    /// <summary>
    /// ����Ү�UI
    /// </summary>
    void DisplayCertificate(Certificate certificate)
    {
        if (certificate == null)
        {
            LogError("������ܪŪ��Ү�");
            return;
        }

        if (certificateDisplayPrefab == null)
        {
            LogError("�Ү���ܹw�s�饼�]�w");
            return;
        }

        if (certificateContainer == null)
        {
            LogError("�ҮѮe�����]�w");
            return;
        }

        GameObject certificateUI = Instantiate(certificateDisplayPrefab, certificateContainer);

        var displayComponent = certificateUI.GetComponent<CertificateDisplayUI>();
        if (displayComponent != null)
        {
            displayComponent.SetupCertificate(certificate);
        }
        else
        {
            LogError("�Ү���ܲե󥼧��");
        }

        // �T�OCanvas���
        if (certificateCanvas != null)
        {
            certificateCanvas.gameObject.SetActive(true);
        }

        LogMessage("�Ү�UI��ܧ���");
    }

    /// <summary>
    /// �����Үѡ]���B�����^
    /// </summary>
    public void VerifyCertificateAsync(Certificate certificate)
    {
        if (certificate == null)
        {
            OnCertificateError?.Invoke("���Ҫ��ҮѬ���");
            return;
        }

        StartCoroutine(VerifyCertificateCoroutine(certificate));
    }

    /// <summary>
    /// �����ҮѨ�{
    /// </summary>
    IEnumerator VerifyCertificateCoroutine(Certificate certificate)
    {
        LogMessage($"�}�l�����Ү�: {certificate.certificateId}");

        bool signatureValid = false;
        bool blockchainValid = false;

        // 1. ���ҼƦ�ñ��
        if (signatureService != null)
        {
            signatureValid = signatureService.VerifyDigitalSignature(certificate, certificate.certificateHash);
            LogMessage($"�Ʀ�ñ������: {(signatureValid ? "����" : "�L��")}");
        }
        else
        {
            LogError("�Ʀ�ñ���A�ȥ��i�ΡA���Lñ������");
        }

        yield return new WaitForSeconds(1f);

        // 2. �q�϶�������
        if (blockchainService != null && !string.IsNullOrEmpty(certificate.blockchainTxHash))
        {
            yield return StartCoroutine(blockchainService.VerifyCertificateOnBlockchain(certificate.certificateHash));
            blockchainValid = true; // ²�ƳB�z
            LogMessage($"�϶�������: {(blockchainValid ? "����" : "�L��")}");
        }
        else
        {
            LogMessage("���L�϶������ҡ]�A�ȥ��i�Ω��Үѥ��W��^");
            blockchainValid = !certificate.isOnChain; // �p�G���W��h���v�T����
        }

        // 3. ��s���Ҫ��A
        certificate.isVerified = signatureValid && (blockchainValid || !certificate.isOnChain);

        OnCertificateVerified?.Invoke(certificate);

        LogMessage($"�Ү����ҧ���: {(certificate.isVerified ? "����" : "�L��")}");
    }

    /// <summary>
    /// �ھ��Ү�ID�d���Ү�
    /// </summary>
    public Certificate FindCertificateById(string certificateId)
    {
        if (string.IsNullOrEmpty(certificateId))
        {
            LogError("�Ү�ID����");
            return null;
        }

        return issuedCertificates.Find(c => c.certificateId == certificateId);
    }

    /// <summary>
    /// ����ǥͪ��Ҧ��Ү�
    /// </summary>
    public List<Certificate> GetStudentCertificates(string studentId)
    {
        if (string.IsNullOrEmpty(studentId))
        {
            LogError("�ǥ�ID����");
            return new List<Certificate>();
        }

        return issuedCertificates.FindAll(c => c.studentId == studentId);
    }

    /// <summary>
    /// ����Ҧ��Үѡ]�޲z���\��^
    /// </summary>
    public List<Certificate> GetAllCertificates()
    {
        return new List<Certificate>(issuedCertificates);
    }

    /// <summary>
    /// �R���Үѡ]�޲z���\��^
    /// </summary>
    public bool DeleteCertificate(string certificateId)
    {
        var certificate = FindCertificateById(certificateId);
        if (certificate == null)
        {
            LogError($"�䤣��n�R�����Ү�: {certificateId}");
            return false;
        }

        if (issuedCertificates.Remove(certificate))
        {
            SaveCertificatesToFile();
            LogMessage($"�ҮѤw�R��: {certificateId}");
            return true;
        }

        return false;
    }

    // === ���U��k ===

    string GetStudentName(string studentId)
    {
        // TODO: ������Τ��q�Τ�޲z�t�����
        // �i�H�q PlayerPrefs �Ψ�L�t������Τ�W
        return PlayerPrefs.GetString($"StudentName_{studentId}", "�ǥͥΤ�");
    }

    string GetCourseName(string examId)
    {
        // �ھڦҸ�ID��^�ҵ{�W��
        return examId switch
        {
            "exam_2_1" => "�ĤG���Ĥ@�` - �T����ư�¦",
            "exam_2_2" => "�ĤG���ĤG�` - �T���������",
            "exam_3_1" => "�ĤT���Ĥ@�` - ����X���¦",
            "exam_3_2" => "�ĤT���ĤG�` - ����X������",
            "exam_4_1" => "�ĥ|���Ĥ@�` - �V�q�P�x�}",
            "exam_4_2" => "�ĥ|���ĤG�` - �u���ܴ�",
            _ => "HoloMath �ƾǽҵ{"
        };
    }

    CertificateType DetermineCertificateType(float percentage)
    {
        if (percentage >= 95) return CertificateType.Outstanding;
        if (percentage >= 90) return CertificateType.Excellence;
        if (percentage >= 80) return CertificateType.Merit;
        if (percentage >= 70) return CertificateType.ExamPass;
        if (percentage >= 60) return CertificateType.BasicPass;
        return CertificateType.Participation;
    }

    string GenerateCertificateDescription(ExamResult examResult)
    {
        string gradeText = examResult.percentage switch
        {
            >= 95 => "���V",
            >= 90 => "�u�q",
            >= 80 => "�}�n",
            >= 70 => "�q�L",
            >= 60 => "�ή�",
            _ => "�ѻP"
        };

        return $"�ǥͤw���\���� {GetCourseName(examResult.examId)} ���ǲߵ����A" +
               $"��o {examResult.percentage:F1}% ��{gradeText}���Z�C" +
               $"�Ҹեή� {examResult.timeTaken.Minutes} �� {examResult.timeTaken.Seconds} ��A" +
               $"�i�{�F�}�n���ƾǲz�ѯ�O�C";
    }

    // === ���ާ@ ===

    bool SaveCertificatesToFile()
    {
        if (string.IsNullOrEmpty(certificatesFilePath))
        {
            LogError("�ҮѤ����|���]�w");
            return false;
        }

        try
        {
            string json = JsonConvert.SerializeObject(issuedCertificates, Formatting.Indented);
            File.WriteAllText(certificatesFilePath, json);
            return true;
        }
        catch (Exception e)
        {
            LogError($"�O�s�ҮѦC����: {e.Message}");
            return false;
        }
    }

    void LoadCertificatesFromFile()
    {
        if (string.IsNullOrEmpty(certificatesFilePath))
        {
            LogError("�ҮѤ����|���]�w");
            return;
        }

        try
        {
            if (File.Exists(certificatesFilePath))
            {
                string json = File.ReadAllText(certificatesFilePath);
                issuedCertificates = JsonConvert.DeserializeObject<List<Certificate>>(json) ?? new List<Certificate>();
                LogMessage($"���J�F {issuedCertificates.Count} ���ҮѰO��");
            }
            else
            {
                LogMessage("�ҮѰO����󤣦s�b�A�Ыطs���O��");
                issuedCertificates = new List<Certificate>();
            }
        }
        catch (Exception e)
        {
            LogError($"���J�ҮѦC����: {e.Message}");
            issuedCertificates = new List<Certificate>();
        }
    }

    bool SaveIndividualCertificateFile(Certificate certificate)
    {
        if (certificate == null)
        {
            LogError("�ҮѬ��šA�L�k�O�s");
            return false;
        }

        try
        {
            string filename = $"certificate_{certificate.certificateId}.json";
            string filePath = Path.Combine(certificatesDirectory, filename);

            string json = JsonConvert.SerializeObject(certificate, Formatting.Indented);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception e)
        {
            LogError($"�O�s����ҮѤ�󥢱�: {e.Message}");
            return false;
        }
    }

    // === ��x��k ===

    void LogMessage(string message)
    {
        if (enableDetailedLogging)
        {
            Debug.Log($"[CertificateManager] {message}");
        }
    }

    void LogWarning(string message)
    {
        Debug.LogWarning($"[CertificateManager] {message}");
    }

    void LogError(string message)
    {
        Debug.LogError($"[CertificateManager] {message}");
    }

    // === Unity �ͩR�P�� ===

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // ���μȰ��ɫO�s�Ү�
            SaveCertificatesToFile();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // ���h�J�I�ɫO�s�Ү�
            SaveCertificatesToFile();
        }
    }

    void OnDestroy()
    {
        // �P���ɫO�s�Ү�
        SaveCertificatesToFile();
    }
}

// === �䴩���O ===
public class CertificateIssuanceRequest
{
    public ExamResult examResult;
    public DateTime requestTime = DateTime.Now;
}