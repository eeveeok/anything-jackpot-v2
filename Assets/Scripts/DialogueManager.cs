using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public GameObject nextIndicator;
    public GameObject portraitImage;
    public GameObject portraitImage2;

    [Header("UI Panels")]
    public GameObject heartPanel;

    [Header("Typing Settings")]
    public float typingSpeed = 0.04f;

    [Header("Dialogue Data (7 NPCs)")]
    public DialogueData npc1Dialogue;
    public DialogueData npc2Dialogue;
    public DialogueData npc3Dialogue;
    public DialogueData npc4Dialogue;
    public DialogueData npc5Dialogue;
    public DialogueData npc6Dialogue;
    public DialogueData npc7Dialogue;
    public DialogueData npc8Dialogue;

    [Header("사운드 설정")]
    public AudioClip chatSound1;       // 채팅 소리1
    public AudioClip chatSound2;       // 채팅 소리2

    private DialogueData currentDialogue;
    private int currentIndex = 0;
    private bool isTyping = false;

    [HideInInspector]
    public bool isDialogueActive = false;

    public void ActiveDialog()
    {
        dialoguePanel.SetActive(false);
        nextIndicator.SetActive(false);
        portraitImage.SetActive(false);   // 처음에 초상화 OFF
        portraitImage2.SetActive(false);

        string scene = SceneManager.GetActiveScene().name;

        //if (scene == "EndingScene" && heartPanel != null)
        //    heartPanel.SetActive(false);

        if (scene == "FirstScene")
        {
            StartDialogue(npc1Dialogue);
        }

        if (scene == "Level 1-1")
        {
            StartDialogue(npc2Dialogue);
        }

        if (scene == "Level 1-5")
        {
            StartDialogue(npc3Dialogue);
        }

        if (scene == "Level 2-1")
        {
            StartDialogue(npc4Dialogue);
        }

        if (scene == "Level 2-20")
        {
            StartDialogue(npc5Dialogue);
        }

        if (scene == "Level 3-1")
        {
            StartDialogue(npc6Dialogue);
        }

        if (scene == "Level 3-10")
        {
            StartDialogue(npc7Dialogue);
        }

        if (scene == "EndingScene")
        {
            StartDialogue(npc8Dialogue);
        }
    }

    void Update()
    {
        // 테스트용: 숫자 키로 대화 시작
        if (!isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                StartDialogue(npc1Dialogue);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                StartDialogue(npc2Dialogue);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                StartDialogue(npc3Dialogue);

            if (Input.GetKeyDown(KeyCode.Alpha4))
                StartDialogue(npc4Dialogue);

            if (Input.GetKeyDown(KeyCode.Alpha5))
                StartDialogue(npc5Dialogue);

            if (Input.GetKeyDown(KeyCode.Alpha6))
                StartDialogue(npc6Dialogue);

            if (Input.GetKeyDown(KeyCode.Alpha7))
                StartDialogue(npc7Dialogue);


        }

        // 대화 진행 중 스페이스바로 넘기기
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentDialogue.lines[currentIndex].text;
                isTyping = false;
                nextIndicator.SetActive(true);
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueData dialogueData)
    {
        currentDialogue = dialogueData;
        currentIndex = 0;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        nextIndicator.SetActive(false);

        ShowLine();
    }

    void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentIndex];

        // 주인공일 때만 초상화 ON
        if (line.speaker == "player")
        {
            SoundManager.Instance.PlaySFX(chatSound1, 0.2f);
            portraitImage.SetActive(true);
            portraitImage2.SetActive(false);
        }
        else if(line.speaker == "Nemo")
        { 
            SoundManager.Instance.PlaySFX(chatSound2, 0.2f);
            portraitImage2.SetActive(true);
            portraitImage.SetActive(false);
        }

        dialogueText.text = "";
        StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        nextIndicator.SetActive(false);

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        nextIndicator.SetActive(true);
    }

    void NextLine()
    {
        currentIndex++;

        if (currentIndex < currentDialogue.lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        portraitImage.SetActive(false);

        if (currentDialogue == npc8Dialogue)
        {
            SceneManager.LoadScene("Title");
        }
    }
}
