using UnityEngine;

public class BtnPop : MonoBehaviour
{
    private Animator anim;
    private const string clipName = "Pop";

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayPop()
    {
        if (anim) anim.Play(clipName, 0, 0f);
    }
}
