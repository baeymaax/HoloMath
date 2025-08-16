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
    [Header("Polygon �]�w")]
    public string polygonRpcUrl = "https://polygon-rpc.com";
    public string contractAddress = "0x742d35Cc6635C0532925a3b8D25316ba100E7";  // �ܨҦa�}
    public string walletAddress = "0x123...";  // HoloMath ���c���]�a�}

    [Header("Gas �]�w")]
    public int gasLimit = 21000;
    public int gasPrice = 20; // Gwei

    private const string API_KEY = "YOUR_POLYGON_API_KEY";

    /// <summary>
    /// �N�ҮѤW�Ǩ�϶���
    /// </summary>
    public IEnumerator UploadCertificateToBlockchain(Certificate certificate)
    {
        Debug.Log($"�}�l�W���ҮѨ� Polygon �϶���: {certificate.certificateId}");

        // 1. �ǳƥ���ƾ�
        string transactionData = PrepareTransactionData(certificate);

        // 2. �����϶����� (������Τ��|�եίu�ꪺWeb3 API)
        yield return StartCoroutine(SimulateBlockchainTransaction(certificate, transactionData));

        Debug.Log($"�ҮѤW�짹��: {certificate.blockchainTxHash}");
    }

    /// <summary>
    /// �ǳư϶������ƾ�
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
    /// �����϶����� (��ڶ}�o���������u��API�ե�)
    /// </summary>
    IEnumerator SimulateBlockchainTransaction(Certificate certificate, string data)
    {
        // ������������
        yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

        // �ͦ����������hash
        string txHash = GenerateTransactionHash(certificate.certificateId);

        // ��s�ҮѪ��϶����T
        certificate.blockchainTxHash = txHash;
        certificate.isOnChain = true;
        certificate.isVerified = true;
        certificate.polygonAddress = contractAddress;

        // �����϶��T�{
        yield return new WaitForSeconds(1f);

        Debug.Log($"�϶��������\ - TxHash: {txHash}");

        // �o�e������\�ƥ�
        OnTransactionSuccess?.Invoke(certificate);
    }

    /// <summary>
    /// �u�ꪺ�϶���API�ե� (�i���@)
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
            Debug.Log($"Polygon API �^��: {responseText}");

            try
            {
                BlockchainResponse response = JsonConvert.DeserializeObject<BlockchainResponse>(responseText);
                // �B�z�^��
            }
            catch (Exception e)
            {
                Debug.LogError($"�ѪR�϶���^������: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"�϶���API�եΥ���: {request.error}");
        }

        request.Dispose();
    }

    /// <summary>
    /// �q�϶��������Ү�
    /// </summary>
    public IEnumerator VerifyCertificateOnBlockchain(string certificateHash)
    {
        Debug.Log($"�q�϶��������Ү�: {certificateHash}");

        // �����d�ߩ���
        yield return new WaitForSeconds(2f);

        // �b������Τ��A�o�̷|�d�߰϶���W���O��
        bool isValid = !string.IsNullOrEmpty(certificateHash);

        Debug.Log($"�϶������ҵ��G: {(isValid ? "����" : "�L��")}");

        OnVerificationComplete?.Invoke(isValid);
    }

    /// <summary>
    /// �ͦ����hash (������)
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
    /// �����egas����
    /// </summary>
    public IEnumerator GetCurrentGasPrice()
    {
        // ������Τ��|�ե�gas price API
        yield return new WaitForSeconds(1f);

        gasPrice = UnityEngine.Random.Range(15, 30); // ����gas����
        Debug.Log($"��e Gas ����: {gasPrice} Gwei");
    }

    // �ƥ�t��
    public event Action<Certificate> OnTransactionSuccess;
    public event Action<bool> OnVerificationComplete;
    public event Action<string> OnTransactionFailed;
}