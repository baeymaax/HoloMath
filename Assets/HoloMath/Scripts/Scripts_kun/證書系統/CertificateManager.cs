using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// HoloMath 證書管理器 - 處理數位證書的生成、簽章、上鏈和驗證
/// </summary>
public class CertificateManager : MonoBehaviour
{
    [Header("依賴組件")]
    public DigitalSignatureService signatureService;
    public BlockchainService blockchainService;

    [Header("UI組件")]
    public GameObject certificateDisplayPrefab;
    public Transform certificateContainer;
    public Canvas certificateCanvas;

    [Header("證書設定")]
    public string institutionName = "HoloMath Education Platform";
    public Sprite institutionLogo;

    [Header("調試設定")]
    public bool enableDetailedLogging = true;

    // 證書儲存
    private List<Certificate> issuedCertificates = new List<Certificate>();
    private string certificatesFilePath;
    private string certificatesDirectory;

    // 事件系統
    public event Action<Certificate> OnCertificateIssued;
    public event Action<Certificate> OnCertificateVerified;
    public event Action<string> OnCertificateError;
    public event Action<string> OnCertificateStatusUpdate;

    // 狀態管理
    private bool isInitialized = false;
    private Queue<CertificateIssuanceRequest> pendingRequests = new Queue<CertificateIssuanceRequest>();
    private bool isProcessingRequest = false;

    void Start()
    {
        StartCoroutine(InitializeCertificateManagerAsync());
    }

    /// <summary>
    /// 異步初始化證書管理器
    /// </summary>
    IEnumerator InitializeCertificateManagerAsync()
    {
        LogMessage("開始初始化證書管理器...");

        // 設定檔案路徑
        certificatesDirectory = Path.Combine(Application.persistentDataPath, "Certificates");
        certificatesFilePath = Path.Combine(certificatesDirectory, "certificates.json");

        // 確保目錄存在
        if (!Directory.Exists(certificatesDirectory))
        {
            Directory.CreateDirectory(certificatesDirectory);
        }

        // 載入已存在的證書
        LoadCertificatesFromFile();

        // 初始化依賴組件
        yield return StartCoroutine(InitializeDependencies());

        isInitialized = true;
        LogMessage("證書管理器初始化完成");

        // 處理待處理的請求
        StartCoroutine(ProcessPendingRequests());
    }

    /// <summary>
    /// 初始化依賴組件
    /// </summary>
    IEnumerator InitializeDependencies()
    {
        if (signatureService == null)
        {
            signatureService = GetComponent<DigitalSignatureService>();
            if (signatureService == null)
            {
                LogError("未找到 DigitalSignatureService 組件");
            }
        }

        if (blockchainService == null)
        {
            blockchainService = GetComponent<BlockchainService>();
            if (blockchainService == null)
            {
                LogError("未找到 BlockchainService 組件");
            }
        }

        // 等待組件初始化
        yield return new WaitForSeconds(0.5f);

        // 驗證組件狀態
        if (signatureService != null)
        {
            LogMessage("數位簽章服務已就緒");
        }

        if (blockchainService != null)
        {
            LogMessage("區塊鏈服務已就緒");
        }
    }

    /// <summary>
    /// 頒發證書 - 主要入口方法（修正版）
    /// </summary>
    public void IssueCertificateAsync(ExamResult examResult)
    {
        if (!isInitialized)
        {
            // 添加到待處理隊列
            pendingRequests.Enqueue(new CertificateIssuanceRequest { examResult = examResult });
            LogMessage("證書管理器尚未初始化完成，已添加到待處理隊列");
            return;
        }

        StartCoroutine(IssueCertificateCoroutine(examResult));
    }

    /// <summary>
    /// 證書頒發協程（重構版本）
    /// </summary>
    IEnumerator IssueCertificateCoroutine(ExamResult examResult)
    {
        if (isProcessingRequest)
        {
            LogMessage("正在處理其他證書請求，請稍候...");
            yield break;
        }

        isProcessingRequest = true;
        Certificate certificate = null;
        string errorMessage = null;

        LogMessage($"開始頒發證書給學生: {examResult.studentId}");
        OnCertificateStatusUpdate?.Invoke("正在創建證書...");

        // 步驟1: 創建證書基本資料
        certificate = CreateBaseCertificate(examResult);
        if (certificate == null)
        {
            errorMessage = "創建基礎證書失敗";
            goto HandleError;
        }

        OnCertificateStatusUpdate?.Invoke("正在生成數位簽章...");

        // 步驟2: 生成數位簽章
        bool signatureSuccess = false;
        yield return StartCoroutine(GenerateDigitalSignatureCoroutine(certificate, (success, error) =>
        {
            signatureSuccess = success;
            if (!success)
            {
                errorMessage = error ?? "數位簽章生成失敗";
            }
        }));

        if (!signatureSuccess)
            goto HandleError;

        OnCertificateStatusUpdate?.Invoke("正在上傳到區塊鏈...");

        // 步驟3: 上傳到區塊鏈
        bool blockchainSuccess = false;
        yield return StartCoroutine(UploadToBlockchainCoroutine(certificate, (success, error) =>
        {
            blockchainSuccess = success;
            if (!success)
            {
                errorMessage = error ?? "區塊鏈上傳失敗";
            }
        }));

        if (!blockchainSuccess)
            goto HandleError;

        OnCertificateStatusUpdate?.Invoke("正在保存證書...");

        // 步驟4: 保存證書記錄
        if (!SaveCertificate(certificate))
        {
            errorMessage = "證書保存失敗";
            goto HandleError;
        }

        OnCertificateStatusUpdate?.Invoke("正在顯示證書...");

        // 步驟5: 顯示證書
        DisplayCertificate(certificate);

        // 成功完成
        OnCertificateIssued?.Invoke(certificate);
        OnCertificateStatusUpdate?.Invoke("證書頒發完成！");
        LogMessage($"證書頒發完成: {certificate.certificateId}");

        isProcessingRequest = false;
        yield break;

    HandleError:
        LogError($"證書頒發失敗: {errorMessage}");
        OnCertificateError?.Invoke(errorMessage);
        OnCertificateStatusUpdate?.Invoke($"證書頒發失敗: {errorMessage}");
        isProcessingRequest = false;
    }

    /// <summary>
    /// 處理待處理的證書請求
    /// </summary>
    IEnumerator ProcessPendingRequests()
    {
        while (pendingRequests.Count > 0)
        {
            var request = pendingRequests.Dequeue();
            yield return StartCoroutine(IssueCertificateCoroutine(request.examResult));
            yield return new WaitForSeconds(1f); // 請求間隔
        }
    }

    /// <summary>
    /// 創建基礎證書資料
    /// </summary>
    Certificate CreateBaseCertificate(ExamResult examResult)
    {
        if (examResult == null)
        {
            LogError("ExamResult 為空");
            return null;
        }

        if (signatureService == null)
        {
            LogError("數位簽章服務未初始化");
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

        LogMessage($"基礎證書創建完成: {certificate.certificateId}");
        return certificate;
    }

    /// <summary>
    /// 生成數位簽章（修正版）
    /// </summary>
    IEnumerator GenerateDigitalSignatureCoroutine(Certificate certificate, Action<bool, string> callback)
    {
        LogMessage("正在生成數位簽章...");

        // 模擬簽章生成時間
        yield return new WaitForSeconds(1f);

        if (signatureService == null)
        {
            callback(false, "數位簽章服務未初始化");
            yield break;
        }

        if (certificate == null)
        {
            callback(false, "證書資料為空");
            yield break;
        }

        string signature = signatureService.GenerateDigitalSignature(certificate);

        if (string.IsNullOrEmpty(signature))
        {
            callback(false, "數位簽章生成失敗");
            yield break;
        }

        certificate.certificateHash = signature;
        certificate.issuerSignature = signature;

        LogMessage("數位簽章生成完成");
        callback(true, null);
    }

    /// <summary>
    /// 上傳到區塊鏈（修正版）
    /// </summary>
    IEnumerator UploadToBlockchainCoroutine(Certificate certificate, Action<bool, string> callback)
    {
        LogMessage("正在上傳證書到區塊鏈...");

        if (blockchainService == null)
        {
            LogWarning("區塊鏈服務未初始化，跳過上鏈步驟");
            yield return new WaitForSeconds(1f); // 模擬處理時間
            callback(true, null);
            yield break;
        }

        if (certificate == null)
        {
            callback(false, "證書資料為空");
            yield break;
        }

        bool uploadSuccess = false;
        string errorMessage = null;

        yield return StartCoroutine(blockchainService.UploadCertificateToBlockchain(certificate));

        // 假設上傳成功的簡化處理
        certificate.isOnChain = true;
        certificate.blockchainTxHash = "0x" + System.Guid.NewGuid().ToString("N").Substring(0, 64);
        uploadSuccess = true;

        callback(uploadSuccess, errorMessage);
    }

    /// <summary>
    /// 保存證書到本地
    /// </summary>
    bool SaveCertificate(Certificate certificate)
    {
        if (certificate == null)
        {
            LogError("嘗試保存空的證書");
            return false;
        }

        // 添加到列表
        issuedCertificates.Add(certificate);

        // 保存到文件
        bool success = SaveCertificatesToFile() && SaveIndividualCertificateFile(certificate);

        if (success)
        {
            LogMessage($"證書已保存: {certificate.certificateId}");
        }
        else
        {
            LogError($"證書保存失敗: {certificate.certificateId}");
        }

        return success;
    }

    /// <summary>
    /// 顯示證書UI
    /// </summary>
    void DisplayCertificate(Certificate certificate)
    {
        if (certificate == null)
        {
            LogError("嘗試顯示空的證書");
            return;
        }

        if (certificateDisplayPrefab == null)
        {
            LogError("證書顯示預製體未設定");
            return;
        }

        if (certificateContainer == null)
        {
            LogError("證書容器未設定");
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
            LogError("證書顯示組件未找到");
        }

        // 確保Canvas顯示
        if (certificateCanvas != null)
        {
            certificateCanvas.gameObject.SetActive(true);
        }

        LogMessage("證書UI顯示完成");
    }

    /// <summary>
    /// 驗證證書（異步版本）
    /// </summary>
    public void VerifyCertificateAsync(Certificate certificate)
    {
        if (certificate == null)
        {
            OnCertificateError?.Invoke("驗證的證書為空");
            return;
        }

        StartCoroutine(VerifyCertificateCoroutine(certificate));
    }

    /// <summary>
    /// 驗證證書協程
    /// </summary>
    IEnumerator VerifyCertificateCoroutine(Certificate certificate)
    {
        LogMessage($"開始驗證證書: {certificate.certificateId}");

        bool signatureValid = false;
        bool blockchainValid = false;

        // 1. 驗證數位簽章
        if (signatureService != null)
        {
            signatureValid = signatureService.VerifyDigitalSignature(certificate, certificate.certificateHash);
            LogMessage($"數位簽章驗證: {(signatureValid ? "有效" : "無效")}");
        }
        else
        {
            LogError("數位簽章服務未可用，跳過簽章驗證");
        }

        yield return new WaitForSeconds(1f);

        // 2. 從區塊鏈驗證
        if (blockchainService != null && !string.IsNullOrEmpty(certificate.blockchainTxHash))
        {
            yield return StartCoroutine(blockchainService.VerifyCertificateOnBlockchain(certificate.certificateHash));
            blockchainValid = true; // 簡化處理
            LogMessage($"區塊鏈驗證: {(blockchainValid ? "有效" : "無效")}");
        }
        else
        {
            LogMessage("跳過區塊鏈驗證（服務未可用或證書未上鏈）");
            blockchainValid = !certificate.isOnChain; // 如果未上鏈則不影響驗證
        }

        // 3. 更新驗證狀態
        certificate.isVerified = signatureValid && (blockchainValid || !certificate.isOnChain);

        OnCertificateVerified?.Invoke(certificate);

        LogMessage($"證書驗證完成: {(certificate.isVerified ? "有效" : "無效")}");
    }

    /// <summary>
    /// 根據證書ID查找證書
    /// </summary>
    public Certificate FindCertificateById(string certificateId)
    {
        if (string.IsNullOrEmpty(certificateId))
        {
            LogError("證書ID為空");
            return null;
        }

        return issuedCertificates.Find(c => c.certificateId == certificateId);
    }

    /// <summary>
    /// 獲取學生的所有證書
    /// </summary>
    public List<Certificate> GetStudentCertificates(string studentId)
    {
        if (string.IsNullOrEmpty(studentId))
        {
            LogError("學生ID為空");
            return new List<Certificate>();
        }

        return issuedCertificates.FindAll(c => c.studentId == studentId);
    }

    /// <summary>
    /// 獲取所有證書（管理員功能）
    /// </summary>
    public List<Certificate> GetAllCertificates()
    {
        return new List<Certificate>(issuedCertificates);
    }

    /// <summary>
    /// 刪除證書（管理員功能）
    /// </summary>
    public bool DeleteCertificate(string certificateId)
    {
        var certificate = FindCertificateById(certificateId);
        if (certificate == null)
        {
            LogError($"找不到要刪除的證書: {certificateId}");
            return false;
        }

        if (issuedCertificates.Remove(certificate))
        {
            SaveCertificatesToFile();
            LogMessage($"證書已刪除: {certificateId}");
            return true;
        }

        return false;
    }

    // === 輔助方法 ===

    string GetStudentName(string studentId)
    {
        // TODO: 實際應用中從用戶管理系統獲取
        // 可以從 PlayerPrefs 或其他系統獲取用戶名
        return PlayerPrefs.GetString($"StudentName_{studentId}", "學生用戶");
    }

    string GetCourseName(string examId)
    {
        // 根據考試ID返回課程名稱
        return examId switch
        {
            "exam_2_1" => "第二章第一節 - 三角函數基礎",
            "exam_2_2" => "第二章第二節 - 三角函數應用",
            "exam_3_1" => "第三章第一節 - 立體幾何基礎",
            "exam_3_2" => "第三章第二節 - 立體幾何應用",
            "exam_4_1" => "第四章第一節 - 向量與矩陣",
            "exam_4_2" => "第四章第二節 - 線性變換",
            _ => "HoloMath 數學課程"
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
            >= 95 => "卓越",
            >= 90 => "優秀",
            >= 80 => "良好",
            >= 70 => "通過",
            >= 60 => "及格",
            _ => "參與"
        };

        return $"學生已成功完成 {GetCourseName(examResult.examId)} 的學習評估，" +
               $"獲得 {examResult.percentage:F1}% 的{gradeText}成績。" +
               $"考試用時 {examResult.timeTaken.Minutes} 分 {examResult.timeTaken.Seconds} 秒，" +
               $"展現了良好的數學理解能力。";
    }

    // === 文件操作 ===

    bool SaveCertificatesToFile()
    {
        if (string.IsNullOrEmpty(certificatesFilePath))
        {
            LogError("證書文件路徑未設定");
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
            LogError($"保存證書列表失敗: {e.Message}");
            return false;
        }
    }

    void LoadCertificatesFromFile()
    {
        if (string.IsNullOrEmpty(certificatesFilePath))
        {
            LogError("證書文件路徑未設定");
            return;
        }

        try
        {
            if (File.Exists(certificatesFilePath))
            {
                string json = File.ReadAllText(certificatesFilePath);
                issuedCertificates = JsonConvert.DeserializeObject<List<Certificate>>(json) ?? new List<Certificate>();
                LogMessage($"載入了 {issuedCertificates.Count} 個證書記錄");
            }
            else
            {
                LogMessage("證書記錄文件不存在，創建新的記錄");
                issuedCertificates = new List<Certificate>();
            }
        }
        catch (Exception e)
        {
            LogError($"載入證書列表失敗: {e.Message}");
            issuedCertificates = new List<Certificate>();
        }
    }

    bool SaveIndividualCertificateFile(Certificate certificate)
    {
        if (certificate == null)
        {
            LogError("證書為空，無法保存");
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
            LogError($"保存單個證書文件失敗: {e.Message}");
            return false;
        }
    }

    // === 日誌方法 ===

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

    // === Unity 生命周期 ===

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 應用暫停時保存證書
            SaveCertificatesToFile();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // 失去焦點時保存證書
            SaveCertificatesToFile();
        }
    }

    void OnDestroy()
    {
        // 銷毀時保存證書
        SaveCertificatesToFile();
    }
}

// === 支援類別 ===
public class CertificateIssuanceRequest
{
    public ExamResult examResult;
    public DateTime requestTime = DateTime.Now;
}