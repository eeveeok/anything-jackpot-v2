using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public LaserShooter player;   // 플레이어 스크립트 연결

    [HideInInspector]
    public static bool isPaused = false;

    [Header("사운드 설정")]
    public AudioClip pauseSound;       // 정지 소리
    public AudioClip selectSound;     // 선택 소리

    void Start()
    {
        pausePanel.SetActive(false);  // 시작할 때 항상 숨김
        Time.timeScale = 1f;          // 게임 속도 정상화
    }

    void Update()
    {
        // ESC 키로 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        // 소리 재생
        SoundManager.Instance.PlaySFX(pauseSound, 0.2f);

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // 게임 멈춤

        if (player != null)
            player.enabled = false;   // 일시정지 동안 조작 금지
    }

    public void Resume()
    {
        // 소리 재생
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);

        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개

        if (player != null)
            player.enabled = true;    // 게임 재개 시 조작 가능
    }

    public void ExitToMenu()
    {
        Resume();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}
