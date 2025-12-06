using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageController : MonoBehaviour
{
    [Header("스테이지 정보")]
    public int stageNumber = 1;

    [Header("스테이지 선택")]
    public bool isMainScene = false;
    public List<GameObject> portals;

    [Header("포탈 설정")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    public Color unlockedColor = Color.white;

    void Start()
    {
        // 스테이지 시작 처리
        InitializeStage();

        // 메인 씬이 아닐 경우에만 도달 처리
        if (!isMainScene)
        {
            ReachStage();
        }
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            StageProgressManager.ResetAllStages();
        }
    }

    void InitializeStage()
    {
        if (isMainScene)
        {
            SetupPortals();
        }
        else
        {
            // 현재 스테이지 설정
            StageProgressManager.SetCurrentStage(stageNumber);

            Debug.Log($"스테이지 시작: ({stageNumber}스테이지)");
        }
    }

    /// <summary>
    /// 포탈 설정 및 잠금 처리
    /// </summary>
    void SetupPortals()
    {
        if (portals == null || portals.Count == 0)
        {
            Debug.LogWarning("포탈이 설정되지 않았습니다.");
            return;
        }

        int highestStage = StageProgressManager.GetHighestStage();

        for (int i = 0; i < portals.Count; i++)
        {
            GameObject portal = portals[i];
            if (portal == null) continue;

            int portalStage = i + 1; // 포탈 순서 = 스테이지 번호

            // 포탈이 잠겼는지 확인
            bool isLocked = portalStage > highestStage;

            // 포탈 설정
            SetupPortal(portal, portalStage, isLocked);
        }
    }

    /// <summary>
    /// 개별 포탈 설정
    /// </summary>
    void SetupPortal(GameObject portal, int portalStage, bool isLocked)
    {
        // 1. 콜라이더 활성/비활성화
        Collider2D collider = portal.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = !isLocked;
        }

        // 2. 스프라이트 색상 변경
        SpriteRenderer spriteRenderer = portal.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isLocked ? lockedColor : unlockedColor;
        }

        if (!isLocked)
            portal.transform.localScale *= 1.7f;
    }

    /// <summary>
    /// 스테이지 도달 처리
    /// </summary>
    public void ReachStage()
    {
        // 이미 도달했으면 중복 처리 방지
        if (StageProgressManager.IsStageReached(stageNumber))
        {
            Debug.Log($"이미 도달한 스테이지입니다: {stageNumber}");
            return;
        }

        // 스테이지 도달 저장
        StageProgressManager.ReachStage(stageNumber);

        Debug.Log($"스테이지 {stageNumber} 도달");
    }
}