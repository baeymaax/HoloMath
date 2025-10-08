// HapticFeedback.cs
using UnityEngine;

public class HapticFeedback : MonoBehaviour
{
    public void PlaySuccess()
    {
        // iOS 觸覺回饋
#if UNITY_IOS
        // 實際專案中使用 iOS Haptic API
#endif

        Debug.Log("Success Haptic");
    }

    public void PlayError()
    {
        Debug.Log("Error Haptic");
    }
}