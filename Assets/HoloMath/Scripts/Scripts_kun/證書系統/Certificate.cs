using System;
using UnityEngine;

[Serializable]
public class Certificate
{
    [Header("�򥻸�T")]
    public string certificateId;        // �ҮѰߤ@ID
    public string studentId;            // �ǥ�ID
    public string studentName;          // �ǥͩm�W
    public string courseName;           // �ҵ{�W��
    public string examId;               // �Ҹ�ID

    [Header("���Z��T")]
    public int score;                   // �o��
    public int maxScore;                // ����
    public float percentage;            // �ʤ���
    public string grade;                // ���� (A+, A, B+, etc.)

    [Header("�ɶ���T")]
    public DateTime issuedDate;         // �{�o���
    public DateTime examCompletedDate;  // �Ҹէ������
    public TimeSpan examDuration;       // �Ҹեή�

    [Header("�o�Ҿ��c")]
    public string issuerName;           // �o�Ҿ��c�W��
    public string issuerSignature;      // ���cñ�W

    [Header("�϶����T")]
    public string certificateHash;      // �Ү�hash��
    public string blockchainTxHash;     // �϶�����hash
    public string polygonAddress;       // Polygon��a�}
    public bool isVerified;             // �O�_�w����
    public bool isOnChain;              // �O�_�w�W��

    [Header("���[��T")]
    public string description;          // �ҮѴy�z
    public string imageUrl;             // �ҮѹϤ�URL
    public CertificateType type;        // �Ү�����
}

[Serializable]
public enum CertificateType
{
    CourseCompletion,    // �ҵ{�����Ү�
    ExamPass,           // �Ҹդή��Ү�
    Excellence,         // �u�q�Ү� (90���H�W)
    Participation       // �ѻP�Ү�
}

[Serializable]
public class CertificateMetadata
{
    public string version = "1.0";
    public string platform = "HoloMath";
    public string standard = "ERC-721";  // NFT�з�
    public string[] skills;              // �ޯ����
    public string[] achievements;        // ���N����
}