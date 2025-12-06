using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    private static bool isInitialized = false;

    void Awake()
    {
        if (!isInitialized)
        {
            DontDestroyOnLoad(gameObject);   // 씬 이동해도 유지

            ApplyCursor();                   // 첫 실행에서만 커서 적용
            isInitialized = true;
        }
        else
        {
            Destroy(gameObject);             // 중복 생성 방지
        }
    }

    void ApplyCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cursorTexture != null)
        {
            Vector2 hotSpot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("Cursor texture not assigned!");
        }
    }
}

