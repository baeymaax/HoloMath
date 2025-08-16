using System;
using UnityEngine;

[Serializable]
public class Certificate
{
    [Header("基本資訊")]
    public string certificateId;        // 證書唯一ID
    public string studentId;            // 學生ID
    public string studentName;          // 學生姓名
    public string courseName;           // 課程名稱
    public string examId;               // 考試ID

    [Header("成績資訊")]
    public int score;                   // 得分
    public int maxScore;                // 滿分
    public float percentage;            // 百分比
    public string grade;                // 等級 (A+, A, B+, etc.)

    [Header("時間資訊")]
    public DateTime issuedDate;         // 頒發日期
    public DateTime examCompletedDate;  // 考試完成日期
    public TimeSpan examDuration;       // 考試用時

    [Header("發證機構")]
    public string issuerName;           // 發證機構名稱
    public string issuerSignature;      // 機構簽名

    [Header("區塊鏈資訊")]
    public string certificateHash;      // 證書hash值
    public string blockchainTxHash;     // 區塊鏈交易hash
    public string polygonAddress;       // Polygon鏈地址
    public bool isVerified;             // 是否已驗證
    public bool isOnChain;              // 是否已上鏈

    [Header("附加資訊")]
    public string description;          // 證書描述
    public string imageUrl;             // 證書圖片URL
    public CertificateType type;        // 證書類型
}

[Serializable]
public enum CertificateType
{
    CourseCompletion,    // 課程完成證書
    ExamPass,           // 考試及格證書
    Excellence,         // 優秀證書 (90分以上)
    Participation       // 參與證書
}

[Serializable]
public class CertificateMetadata
{
    public string version = "1.0";
    public string platform = "HoloMath";
    public string standard = "ERC-721";  // NFT標準
    public string[] skills;              // 技能標籤
    public string[] achievements;        // 成就標籤
}