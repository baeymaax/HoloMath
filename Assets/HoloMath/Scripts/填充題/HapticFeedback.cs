// HapticFeedback.cs
using UnityEngine;

public class HapticFeedback : MonoBehaviour
{
    public void PlaySuccess()
    {
        // iOS Ĳı�^�X
#if UNITY_IOS
        // ��ڱM�פ��ϥ� iOS Haptic API
#endif

        Debug.Log("Success Haptic");
    }

    public void PlayError()
    {
        Debug.Log("Error Haptic");
    }
}