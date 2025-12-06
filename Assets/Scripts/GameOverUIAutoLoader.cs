using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUIAutoLoader : MonoBehaviour
{
    
    public GameObject GameOverUI;

    void Awake()
    {
        string scene = SceneManager.GetActiveScene().name;

        // Title 같은 특정 씬에서는 UI 생성 안 함
        if (scene == "Title")
            return;


        if (FindObjectOfType<GameOverManager>() != null)
            return;


        GameObject ui = Instantiate(GameOverUI);
        DontDestroyOnLoad(ui);
    }
}
