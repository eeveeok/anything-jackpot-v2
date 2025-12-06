using UnityEngine;

public static class StageProgressManager
{
    // 키 상수 정의
    private const string HIGHEST_STAGE_KEY = "HighestStage";
    private const string CURRENT_STAGE_KEY = "CurrentStage";
    private const string STAGE_PREFIX = "Stage_";

    /// <summary>
    /// 최대 스테이지 수
    /// </summary>
    public static int MaxStages { get; set; } = 10;

    /// <summary>
    /// 최고 달성 스테이지 가져오기
    /// </summary>
    public static int GetHighestStage()
    {
        return PlayerPrefs.GetInt(HIGHEST_STAGE_KEY, 1);
    }

    /// <summary>
    /// 최고 달성 스테이지 설정하기
    /// </summary>
    public static void SetHighestStage(int stage)
    {
        int currentHighest = GetHighestStage();
        if (stage > currentHighest)
        {
            PlayerPrefs.SetInt(HIGHEST_STAGE_KEY, stage);
            PlayerPrefs.Save();
            Debug.Log($"최고 스테이지 갱신: {stage}단계");
        }
    }

    /// <summary>
    /// 현재 플레이 중인 스테이지 가져오기
    /// </summary>
    public static int GetCurrentStage()
    {
        return PlayerPrefs.GetInt(CURRENT_STAGE_KEY, 1);
    }

    /// <summary>
    /// 현재 플레이 중인 스테이지 설정하기
    /// </summary>
    public static void SetCurrentStage(int stage)
    {
        stage = Mathf.Clamp(stage, 1, MaxStages);
        PlayerPrefs.SetInt(CURRENT_STAGE_KEY, stage);

        // 최고 스테이지도 업데이트
        if (stage > GetHighestStage())
        {
            SetHighestStage(stage);
        }

        PlayerPrefs.Save();
        Debug.Log($"현재 스테이지 설정: {stage}단계");
    }

    /// <summary>
    /// 특정 스테이지 도달 여부 확인
    /// </summary>
    public static bool IsStageReached(int stage)
    {
        return PlayerPrefs.GetInt(STAGE_PREFIX + stage, 0) == 1;
    }

    /// <summary>
    /// 스테이지 도달 처리
    /// </summary>
    public static void ReachStage(int stage)
    {
        stage = Mathf.Clamp(stage, 1, MaxStages);

        // 스테이지 도달 표시
        PlayerPrefs.SetInt(STAGE_PREFIX + stage, 1);

        // 다음 스테이지 잠금 해제 (최고 스테이지 갱신)
        if (stage <= MaxStages)
        {
            SetHighestStage(stage);
        }

        PlayerPrefs.Save();
        Debug.Log($"스테이지 {stage} 도달!");
    }

    /// <summary>
    /// 모든 스테이지 도달 여부 확인
    /// </summary>
    public static bool IsAllStagesReached()
    {
        for (int i = 1; i <= MaxStages; i++)
        {
            if (!IsStageReached(i))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 도달한 스테이지 개수 가져오기
    /// </summary>
    public static int GetReachedStageCount()
    {
        int count = 0;
        for (int i = 1; i <= MaxStages; i++)
        {
            if (IsStageReached(i))
                count++;
        }
        return count;
    }

    /// <summary>
    /// 도달 퍼센트 계산
    /// </summary>
    public static float GetReachPercentage()
    {
        if (MaxStages <= 0) return 0f;
        return (float)GetReachedStageCount() / MaxStages * 100f;
    }

    /// <summary>
    /// 모든 스테이지 데이터 초기화 (새 게임)
    /// </summary>
    public static void ResetAllStages()
    {
        for (int i = 1; i <= MaxStages; i++)
        {
            PlayerPrefs.SetInt(STAGE_PREFIX + i, 0);
        }
        PlayerPrefs.SetInt(HIGHEST_STAGE_KEY, 1);
        PlayerPrefs.SetInt(CURRENT_STAGE_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("모든 스테이지 데이터 초기화 완료");
    }

    /// <summary>
    /// 특정 스테이지 데이터만 초기화
    /// </summary>
    public static void ResetStage(int stage)
    {
        stage = Mathf.Clamp(stage, 1, MaxStages);

        PlayerPrefs.SetInt(STAGE_PREFIX + stage, 0);

        // 최고 스테이지 조정
        if (GetHighestStage() > stage)
        {
            PlayerPrefs.SetInt(HIGHEST_STAGE_KEY, stage);
        }

        PlayerPrefs.Save();
        Debug.Log($"스테이지 {stage} 데이터 초기화 완료");
    }

    /// <summary>
    /// 스테이지 진행 상태 출력 (디버그용)
    /// </summary>
    public static void PrintStageStatus()
    {
        Debug.Log("=== 스테이지 진행 현황 ===");
        Debug.Log($"최고 스테이지: {GetHighestStage()}");
        Debug.Log($"현재 스테이지: {GetCurrentStage()}");
        Debug.Log($"도달한 스테이지: {GetReachedStageCount()}/{MaxStages}");
        Debug.Log($"도달율: {GetReachPercentage():F1}%");

        for (int i = 1; i <= MaxStages; i++)
        {
            string status = IsStageReached(i) ? "도달 ✓" : "미도달 ✗";
            string unlocked = (i <= GetHighestStage()) ? "잠금해제" : "잠김";
            Debug.Log($"스테이지 {i}: {status} ({unlocked})");
        }
    }

    /// <summary>
    /// 저장된 모든 데이터 삭제
    /// </summary>
    public static void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("모든 저장 데이터 삭제 완료");
    }
}