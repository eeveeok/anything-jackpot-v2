using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class HealthUIManager : MonoBehaviour
{
    [Header("Mask Icons (Left to Right)")]
    public Image[] masks;                // 하트 5개
    public Sprite normalMaskSprite;
    public Sprite brokenMaskSprite;

    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Blink Settings")]
    public float blinkInterval = 0.15f;   // 깜빡임 속도 (빠르게/느리게)
    public int blinkCount = 3;

    GameOverManager gameOverManager;
    LaserShooter player;

    void Awake()
    {
        // 씬 로드 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        gameOverManager = FindObjectOfType<GameOverManager>();
        player = FindObjectOfType<LaserShooter>();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 다음 스테이지로 넘어올 때 체력 FULL 회복
        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        // 테스트용 E키로 데미지
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        // 전체 하트 깜빡임
        StartCoroutine(BlinkAllMasks());

        // 체력 0 → 게임오버 화면 출력
        if (currentHealth == 0)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        if (player != null)
            player.enabled = false;

        if (gameOverManager != null)
            gameOverManager.TriggerGameOver();
        else
            Debug.LogWarning("GameOverManager not found!");
    }

    void UpdateUI()
    {
        for (int i = 0; i < masks.Length; i++)
        {
            if (i < currentHealth)
                masks[i].sprite = normalMaskSprite;
            else
                masks[i].sprite = brokenMaskSprite;
        }
    }

    IEnumerator BlinkAllMasks()
    {
       
        for (int i = 0; i < blinkCount; i++)
        {
            // 하트 전부 꺼짐
            foreach (Image img in masks)
                img.enabled = false;

            yield return new WaitForSeconds(blinkInterval);

            // 하트 전부 켜짐
            foreach (Image img in masks)
                img.enabled = true;

            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
