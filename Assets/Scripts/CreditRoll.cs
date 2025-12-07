using UnityEngine;

public class CreditRoll : MonoBehaviour
{
    public RectTransform creditsText; 
    public float scrollSpeed = 50f;     
    public float endY = 1200f;         
    private Vector2 startPos;
    public GameObject mainMenuPanel;

    void OnEnable()
    {
        startPos = new Vector2(creditsText.anchoredPosition.x, -400f);
        creditsText.anchoredPosition = startPos;
    }

    void Update()
    {
        
        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        
        if (creditsText.anchoredPosition.y >= endY || Input.GetKeyDown(KeyCode.Escape))
        {
            transform.parent.gameObject.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }
}