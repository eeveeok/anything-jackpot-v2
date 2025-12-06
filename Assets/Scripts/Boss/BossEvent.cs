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

    [Header("시간 효과")]
    public float timeSlowFactor = 0.3f; // 시간 느려짐 정도
    public float timeSlowDuration = 1.0f;

    [Header("이벤트 설정")]
    public bool eventTriggered = false;

    private CinemachineConfiner2D confiner;
    private CinemachineBasicMultiChannelPerlin noise;
    private float originalOrthoSize;
    private bool isEventActive = false;

    void Start()
    {
        // 컴포넌트 초기화
        if (virtualCamera != null)
        {
            originalOrthoSize = virtualCamera.m_Lens.OrthographicSize;
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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
            // 플레이어 컨트롤 잠시 중지
            player.enabled = false;

            // 플레이어 위치 이동
            player.transform.position = checkPoint.position;

            // 플레이어 물리 정지
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
            // 바운딩 설정
            CinemachineConfiner2D confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null && bossCameraConfiner != null)
            {
                confiner.m_BoundingShape2D = bossCameraConfiner;
            }

            // 보스월 활성화
            if (bossInvisibleWalls != null)
            {
                bossInvisibleWalls.SetActive(true);
            }
        }

        // 3. 시간 느려짐 효과
        yield return StartCoroutine(SlowTimeEffect());

        // 4. 카메라 줌 아웃 + 흔들림 효과
        yield return StartCoroutine(ZoomAndShakeEffect());

        // 5. 플레이어 컨트롤 복구
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }
            player.enabled = true;
        }

        // 6. 보스 등장 애니메이션
        if (bossObject != null)
        {
            yield return StartCoroutine(BossAppearanceAnimation());
        }

        isEventActive = false;

        // 7. 트리거 콜라이더 비활성화 (한 번만 실행)
        GetComponent<Collider2D>().enabled = false;
    }

    // 시간 느려짐 효과
    private IEnumerator SlowTimeEffect()
    {
        float originalTimeScale = Time.timeScale;
        float elapsed = 0f;

        // 시간 점점 느려지기
        while (elapsed < timeSlowDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (timeSlowDuration / 2);
            Time.timeScale = Mathf.Lerp(originalTimeScale, timeSlowFactor, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        // 일정 시간 유지
        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(0.5f);

        // 시간 점점 복구
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

        // 카메라 흔들림 활성화
        if (noise != null)
        {
            noise.m_AmplitudeGain = shakeIntensity;
            noise.m_FrequencyGain = shakeFrequency;
        }

        // 줌 아웃 효과
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / zoomDuration;
            float curveValue = zoomCurve.Evaluate(t);

            // 부드러운 줌 아웃
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentSize, targetOrthoSize, curveValue);

            // 흔들림 강도 감소
            if (noise != null)
            {
                float shakeT = Mathf.Clamp01(elapsed / shakeDuration);
                float shakeValue = shakeCurve.Evaluate(shakeT);
                noise.m_AmplitudeGain = shakeIntensity * shakeValue;
            }

            yield return null;
        }

        virtualCamera.m_Lens.OrthographicSize = targetOrthoSize;
        confiner.m_MaxWindowSize = 0f;

        // 흔들림 비활성화
        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    // 보스 등장 애니메이션
    private IEnumerator BossAppearanceAnimation()
    {
        // 보스 초기화 (비활성화 상태에서 시작)
        bool wasActive = bossObject.activeSelf;
        bossObject.SetActive(false);

        // 잠시 대기
        yield return new WaitForSecondsRealtime(0.5f);

        // 보스 활성화
        bossObject.SetActive(true);

        // 스케일 애니메이션 (커지면서 등장)
        Vector3 originalScale = bossObject.transform.localScale;
        bossObject.transform.localScale = Vector3.zero;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // 탄성 효과
            float scaleValue = ElasticOut(t);
            bossObject.transform.localScale = originalScale * scaleValue;

            yield return null;
        }

        bossObject.transform.localScale = originalScale;
    }

    // 탄성 효과 함수
    private float ElasticOut(float t)
    {
        float p = 0.3f;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
    }
    void OnDestroy()
    {
        // 씬 전환 시 시간 복구
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}