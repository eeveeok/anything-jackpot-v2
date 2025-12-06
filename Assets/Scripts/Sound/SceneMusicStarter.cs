using UnityEngine;

public class SceneMusicStarter : MonoBehaviour
{
    [Header("이 씬의 배경음악 설정")]
    public AudioClip bgmClip;       // 인스펙터에서 드래그 & 드롭

    [Range(0f, 1f)]
    public float volume = 0.5f;     // 볼륨 조절 필요 시

    void Start()
    {
        // 씬이 시작될 때 사운드 매니저에게 재생 요청
        // (이미 같은 곡이면 SoundManager가 알아서 무시하고, 다른 곡이면 페이드 전환함)
        if (SoundManager.Instance != null && bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip, volume);
        }
    }
}