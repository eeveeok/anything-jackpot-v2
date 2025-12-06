using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossEvent : MonoBehaviour
{
    [Header("플레이어 및 체크포인트")]
    public BasePlayerController player;
    public Transform checkPoint;

    [Header("카메라 설정")]
    public CinemachineVirtualCamera virtualCamera;
    public Collider2D bossCameraConfiner;

    [Header("카메라 줌 설정")]
    public float targetOrthoSize = 12.5f;
    public float zoomDuration = 3.0f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("카메라 흔들림 효과")]
    public float shakeIntensity = 1.5f;
    public float shakeFrequency = 2.0f;
    public float shakeDuration = 1.5f;
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("게임오브젝트 설정")]
    public GameObject bossInvisibleWalls;
    public GameObject bossObject; // 보스 오브젝트

    [Header("보스 위치 설정")]
    public Transform bossSpawnPoint; // 보스 스폰 위치 (옵션)
    public bool autoAdjustToGround = true; // 자동으로 지면에 붙이기
    public float groundCheckDistance = 5f; // 지면 검사 거리
    public LayerMask groundLayer; // 지면 레이어

    [Header("시간 효과")]
    public float timeSlowFactor = 0.3f;
    public float timeSlowDuration = 1.0f;

    [Header("이벤트 설정")]
    public bool eventTriggered = false;

    private CinemachineConfiner2D confiner;
    private CinemachineBasicMultiChannelPerlin noise;
    private float originalOrthoSize;
    private bool isEventActive = false;

    // 보스 원래 위치 저장
    private Vector3 bossOriginalPosition;
    private Vector3 bossOriginalScale;
    private Collider2D bossCollider;

    void Start()
    {
        // 컴포넌트 초기화
        if (virtualCamera != null)
        {
            originalOrthoSize = virtualCamera.m_Lens.OrthographicSize;
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // 보스 오브젝트 초기화
        if (bossObject != null)
        {
            // 보스의 원래 위치와 스케일 저장
            bossOriginalPosition = bossObject.transform.position;
            bossOriginalScale = bossObject.transform.localScale;

            // 보스의 Collider 찾기
            bossCollider = bossObject.GetComponent<Collider2D>();

            // 보스 비활성화
            bossObject.SetActive(false);
        }

        // 지면 레이어가 설정되지 않았으면 자동 설정
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground", "Default");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !eventTriggered && !isEventActive)
        {
            StartCoroutine(BossEventSequence());
        }
    }

    private IEnumerator BossEventSequence()
    {
        eventTriggered = true;
        isEventActive = true;

        // 1. 플레이어 이동 및 컨트롤 중지
        if (player != null)
        {
            player.enabled = false;
            player.transform.position = checkPoint.position;

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.isKinematic = true;
            }
        }

        // 2. 카메라 설정 초기화
        if (virtualCamera != null)
        {
            CinemachineConfiner2D confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null && bossCameraConfiner != null)
            {
                confiner.m_BoundingShape2D = bossCameraConfiner;
            }

            if (bossInvisibleWalls != null)
            {
                bossInvisibleWalls.SetActive(true);
            }
        }

        // 3. 시간 느려짐 효과
        yield return StartCoroutine(SlowTimeEffect());

        // 4. 카메라 줌 아웃 + 흔들림 효과
        yield return StartCoroutine(ZoomAndShakeEffect());

        // 5. 보스 등장 애니메이션 (플레이어 컨트롤 복구 전에)
        if (bossObject != null)
        {
            yield return StartCoroutine(BossAppearanceAnimation());
        }

        // 6. 플레이어 컨트롤 복구
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }
            player.enabled = true;
        }

        isEventActive = false;

        // 7. 트리거 콜라이더 비활성화
        GetComponent<Collider2D>().enabled = false;
    }

    // 시간 느려짐 효과
    private IEnumerator SlowTimeEffect()
    {
        float originalTimeScale = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < timeSlowDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (timeSlowDuration / 2);
            Time.timeScale = Mathf.Lerp(originalTimeScale, timeSlowFactor, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(0.5f);

        elapsed = 0f;
        while (elapsed < timeSlowDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (timeSlowDuration / 2);
            Time.timeScale = Mathf.Lerp(timeSlowFactor, originalTimeScale, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    // 카메라 줌 + 흔들림 효과
    private IEnumerator ZoomAndShakeEffect()
    {
        float elapsed = 0f;
        float currentSize = virtualCamera.m_Lens.OrthographicSize;

        if (noise != null)
        {
            noise.m_AmplitudeGain = shakeIntensity;
            noise.m_FrequencyGain = shakeFrequency;
        }

        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / zoomDuration;
            float curveValue = zoomCurve.Evaluate(t);

            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentSize, targetOrthoSize, curveValue);

            if (noise != null)
            {
                float shakeT = Mathf.Clamp01(elapsed / shakeDuration);
                float shakeValue = shakeCurve.Evaluate(shakeT);
                noise.m_AmplitudeGain = shakeIntensity * shakeValue;
            }

            yield return null;
        }

        virtualCamera.m_Lens.OrthographicSize = targetOrthoSize;

        // Oversize Window 설정
        if (confiner != null)
        {
            confiner.m_MaxWindowSize = 0f; // 보스룸에서는 넓게
        }

        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    // 보스 등장 애니메이션 (수정됨)
    private IEnumerator BossAppearanceAnimation()
    {
        // 보스 초기 위치 설정
        Vector3 spawnPosition = bossOriginalPosition;

        // 스폰 포인트가 지정되었으면 사용
        if (bossSpawnPoint != null)
        {
            spawnPosition = bossSpawnPoint.position;
        }

        // 보스 위치 설정
        bossObject.transform.position = spawnPosition;

        // 자동으로 지면에 붙이기
        if (autoAdjustToGround && bossCollider != null)
        {
            AdjustBossToGround();
        }

        // 잠시 대기
        yield return new WaitForSecondsRealtime(0.5f);

        // 보스 활성화
        bossObject.SetActive(true);

        // 스케일 애니메이션
        bossObject.transform.localScale = Vector3.zero;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // 탄성 효과
            float scaleValue = ElasticOut(t);
            bossObject.transform.localScale = bossOriginalScale * scaleValue;

            yield return null;
        }

        bossObject.transform.localScale = bossOriginalScale;

        // 최종 위치 조정 (스케일 변경 후 다시)
        if (autoAdjustToGround && bossCollider != null)
        {
            AdjustBossToGround();
        }
    }

    // 보스를 지면에 붙이는 함수
    private void AdjustBossToGround()
    {
        if (bossCollider == null) return;

        // Collider의 바닥 위치 계산
        Bounds bounds = bossCollider.bounds;
        Vector2 bottomCenter = new Vector2(bounds.center.x, bounds.min.y);

        // 아래 방향으로 Raycast
        RaycastHit2D hit = Physics2D.Raycast(
            bottomCenter,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // Raycast 시각화 (디버그용)
        Debug.DrawRay(bottomCenter, Vector2.down * groundCheckDistance, Color.red, 2f);

        if (hit.collider != null)
        {
            // 지면까지의 거리 계산
            float distanceToGround = hit.distance;

            // 보스 위치 조정
            bossObject.transform.position -= new Vector3(0, distanceToGround, 0);
        }
    }

    // 간단한 이징 함수
    private float ElasticOut(float t)
    {
        float p = 0.3f;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
    }

    // 보스 위치 재조정 함수 (디버그용)
    public void DebugAdjustBossPosition()
    {
        if (bossObject != null && bossCollider != null)
        {
            AdjustBossToGround();
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}