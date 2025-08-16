using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

[Serializable]
public class BlockchainTransaction
{
    public string transactionHash;
    public string fromAddress;
    public string toAddress;
    public string data;
    public DateTime timestamp;
    public bool isConfirmed;
    public int blockNumber;
}

[Serializable]
public class BlockchainResponse
{
    public bool success;
    public string transactionHash;
    public string message;
    public string blockNumber;
}

public class BlockchainService : MonoBehaviour
{
    [Header("Polygon 設定")]
    public string polygonRpcUrl = "https://polygon-rpc.com";
    public string contractAddress = "0x742d35Cc6635C0532925a3b8D25316ba100E7";  // 示例地址
    public string walletAddress = "0x123...";  // HoloMath 機構錢包地址

    [Header("Gas 設定")]
    public int gasLimit = 21000;
    public int gasPrice = 20; // Gwei

    private const string API_KEY = "YOUR_POLYGON_API_KEY";

    /// <summary>
    /// 將證書上傳到區塊鏈
    /// </summary>
    public IEnumerator UploadCertificateToBlockchain(Certificate certificate)
    {
        Debug.Log($"開始上傳證書到 Polygon 區塊鏈: {certificate.certificateId}");

        // 1. 準備交易數據
        string transactionData = PrepareTransactionData(certificate);

        // 2. 模擬區塊鏈交易 (實際應用中會調用真實的Web3 API)
        yield return StartCoroutine(SimulateBlockchainTransaction(certificate, transactionData));

        Debug.Log($"證書上鏈完成: {certificate.blockchainTxHash}");
    }

    /// <summary>
    /// 準備區塊鏈交易數據
    /// </summary>
    string PrepareTransactionData(Certificate certificate)
    {
        var blockchainData = new
        {
            certificateId = certificate.certificateId,
            studentId = certificate.studentId,
            courseName = certificate.courseName,
            examId = certificate.examId,
            hash = certificate.certificateHash,
            timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            issuer = certificate.issuerName
        };

        return JsonConvert.SerializeObject(blockchainData);
    }

    /// <summary>
    /// 模擬區塊鏈交易 (實際開發中替換為真實API調用)
    /// </summary>
    IEnumerator SimulateBlockchainTransaction(Certificate certificate, string data)
    {
        // 模擬網路延遲
        yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

        // 生成模擬的交易hash
        string txHash = GenerateTransactionHash(certificate.certificateId);

        // 更新證書的區塊鏈資訊
        certificate.blockchainTxHash = txHash;
        certificate.isOnChain = true;
        certificate.isVerified = true;
        certificate.polygonAddress = contractAddress;

        // 模擬區塊確認
        yield return new WaitForSeconds(1f);

        Debug.Log($"區塊鏈交易成功 - TxHash: {txHash}");

        // 發送交易成功事件
        OnTransactionSuccess?.Invoke(certificate);
    }

    /// <summary>
    /// 真實的區塊鏈API調用 (可選實作)
    /// </summary>
    IEnumerator CallPolygonAPI(string endpoint, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(polygonRpcUrl + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"Polygon API 回應: {responseText}");

            try
            {
                BlockchainResponse response = JsonConvert.DeserializeObject<BlockchainResponse>(responseText);
                // 處理回應
            }
            catch (Exception e)
            {
                Debug.LogError($"解析區塊鏈回應失敗: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"區塊鏈API調用失敗: {request.error}");
        }

        request.Dispose();
    }

    /// <summary>
    /// 從區塊鏈驗證證書
    /// </summary>
    public IEnumerator VerifyCertificateOnBlockchain(string certificateHash)
    {
        Debug.Log($"從區塊鏈驗證證書: {certificateHash}");

        // 模擬查詢延遲
        yield return new WaitForSeconds(2f);

        // 在實際應用中，這裡會查詢區塊鏈上的記錄
        bool isValid = !string.IsNullOrEmpty(certificateHash);

        Debug.Log($"區塊鏈驗證結果: {(isValid ? "有效" : "無效")}");

        OnVerificationComplete?.Invoke(isValid);
    }

    /// <summary>
    /// 生成交易hash (模擬用)
    /// </summary>
    string GenerateTransactionHash(string certificateId)
    {
        string data = $"{certificateId}:{DateTime.Now.Ticks}:{UnityEngine.Random.Range(1000, 9999)}";
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            return "0x" + BitConverter.ToString(hashedBytes).Replace("-", "").ToLower().Substring(0, 40);
        }
    }

    /// <summary>
    /// 獲取當前gas價格
    /// </summary>
    public IEnumerator GetCurrentGasPrice()
    {
        // 實際應用中會調用gas price API
        yield return new WaitForSeconds(1f);

        gasPrice = UnityEngine.Random.Range(15, 30); // 模擬gas價格
        Debug.Log($"當前 Gas 價格: {gasPrice} Gwei");
    }

    // 事件系統
    public event Action<Certificate> OnTransactionSuccess;
    public event Action<bool> OnVerificationComplete;
    public event Action<string> OnTransactionFailed;
}