using UnityEngine;

public class CreditRoll : MonoBehaviour
{
    public RectTransform creditsText;  // 올라갈 텍스트
    public float scrollSpeed = 50f;     // 속도
    public float endY = 1200f;          // 도착할 위치
    private Vector2 startPos;
    public GameObject mainMenuPanel;

    void OnEnable()
    {
        // 텍스트를 다시 시작 위치로 되돌리기
        startPos = new Vector2(creditsText.anchoredPosition.x, -400f);
        creditsText.anchoredPosition = startPos;
    }

    void Update()
    {
        // 위로 이동
        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // 끝까지 올라가면 자동 종료
        if (creditsText.anchoredPosition.y >= endY || Input.GetKeyDown(KeyCode.Escape))
        {
            this.gameObject.SetActive(false); // Panel_Credit 비활성화
            mainMenuPanel.SetActive(true);
        }
    }
}
