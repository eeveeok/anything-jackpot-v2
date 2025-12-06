using UnityEngine;

public class SoundAutoLoader : MonoBehaviour
{
    public GameObject soundManagerPrefab;

    void Awake()
    {
        // 이미 SoundManager가 존재한다면 생성하지 않음
        if (FindObjectOfType<SoundManager>() != null)
            return;

        // SoundManager 생성
        GameObject manager = Instantiate(soundManagerPrefab);
        // SoundManager 내부에서 DontDestroyOnLoad를 처리하므로 여기선 Instantiate만 하면 됨
        manager.name = "SoundManager";
    }
}