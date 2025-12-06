using UnityEngine;

public class UIResetter : MonoBehaviour
{
    void Awake()
    {
        // DontDestroyOnLoad로 살아있는 모든 UI 검색
        var allUIManagers = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var obj in allUIManagers)
        {
            if (obj.name.Contains("DialogueUI") ||
                obj.name.Contains("HealthUI") ||
                obj.name.Contains("PauseUI") ||
                obj.name.Contains("GameOverUI") ||
                obj.name.Contains("AutoLoader"))
            {
                Destroy(obj.gameObject);
            }
        }

        // TimeScale 초기화
        Time.timeScale = 1f;
    }
}
