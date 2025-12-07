using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;   // Panel_MainMenu
    public GameObject creditPanel;     // Panel_Credit

    [Header("사운드 설정")]
    public AudioClip selectSound;       // 선택 소리


    

    // Start 버튼 → 게임 시작
    public void OnClickStart()
    {
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);
        SceneManager.LoadScene("FirstScene");   // 메인 게임 씬 이름
    }

    // Exit 버튼 → 게임 종료
    public void OnClickExit()
    {
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서는 이것도 필요
#endif
    }

    // Credit 버튼 → 크레딧 패널 활성화
    public void OnClickCredit()
    {
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);
        creditPanel.SetActive(true);
       mainMenuPanel.SetActive(false);
    }

    // Close 버튼 → 크레딧 패널 닫기
    public void OnClickCloseCredit()
    {
        SoundManager.Instance.PlaySFX(selectSound, 0.2f);
         creditPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    void Update()
    {
        if (creditPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            creditPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

}
