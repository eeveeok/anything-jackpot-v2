using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAutoLoader : MonoBehaviour
{
   
    public GameObject dialogueUI;

    void Awake()
    {
        string scene = SceneManager.GetActiveScene().name;

        // Title 같은 특정 씬에서는 UI 생성 안 함
        if (scene == "Title")
            return;

       
        if (FindObjectOfType<DialogueManager>() != null)
            return;

        
        GameObject ui = Instantiate(dialogueUI);
        DontDestroyOnLoad(ui);
    }
}

