using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseNavigation : MonoBehaviour
{
    public Button[] pauseButtons; // 계속하기, 나가기
    private int index = 0;

    [Header("사운드 설정")]
    public AudioClip selectSound;       // 선택 소리

    void Awake()
    {
        // 씬에 EventSystem이 없으면 생성
        if (EventSystem.current == null)
        {
            GameObject esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // [핵심] 생성된 EventSystem을 이 오브젝트(PauseUI)의 자식으로 설정
            esObj.transform.SetParent(this.transform);
        }
    }


    private void OnEnable()
    {
        index = 0;
        EventSystem.current.SetSelectedGameObject(pauseButtons[index].gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            //소리 재생
            SoundManager.Instance.PlaySFX(selectSound, 0.2f);
            index++;
            if (index >= pauseButtons.Length)
                index = 0;

            EventSystem.current.SetSelectedGameObject(pauseButtons[index].gameObject);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            //소리 재생
            SoundManager.Instance.PlaySFX(selectSound, 0.2f);
            index--;
            if (index < 0)
                index = pauseButtons.Length - 1;

            EventSystem.current.SetSelectedGameObject(pauseButtons[index].gameObject);
        }
    }
}
