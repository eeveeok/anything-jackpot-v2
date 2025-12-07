using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static bool isGameOver = false;

    public GameObject gameOverPanel;

    public Text gameOverText;
    public Text continueText;

    public GameObject yesButtonObj;
    public GameObject noButtonObj;
    public GameObject navigationObject;

    [Header("사운드 설정")]
    public AudioClip selectSound;       // 선택 소리
    public AudioClip gameoverBGM;      // 게임오버 BGM

    void Start()
    {
        gameOverPanel.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        continueText.gameObject.SetActive(false);
        yesButtonObj.SetActive(false);
        noButtonObj.SetActive(false);
        navigationObject.SetActive(false);
    }

    void Update()
    {
        // 테스트용: T키를 누르면 즉시 게임오버 UI 출력
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        // 게임오버 BGM 재생
        SoundManager.Instance.PlayBGM(gameoverBGM, 0.2f);
        isGameOver = true;
        Time.timeScale = 0f;
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {

        gameOverPanel.SetActive(true);

    
        gameOverText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(1.0f);

        continueText.gameObject.SetActive(true);

        yesButtonObj.SetActive(true);
        noButtonObj.SetActive(true);
        navigationObject.SetActive(true);
    }

    public void OnClickYes()
    {
        // 선택 소리 재생
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);

        Time.timeScale = 1f;

        LaserShooter player = FindObjectOfType<LaserShooter>();
        if (player != null)
            player.enabled = true;

        // UI 숨기기
        gameOverPanel.SetActive(false);
        yesButtonObj.SetActive(false);
        noButtonObj.SetActive(false);
        navigationObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        continueText.gameObject.SetActive(false); ;

        GameOverManager.isGameOver = false;

        // 메인 메뉴 씬으로 이동
        SceneManager.LoadScene("MainMenuScene");
    }

    public void OnClickNo()
    {
        // 선택 소리 재생
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);

        Time.timeScale = 1f;

        LaserShooter player = FindObjectOfType<LaserShooter>();
        if (player != null)
            player.enabled = true;

        SceneManager.LoadScene("Title");
    }
}
