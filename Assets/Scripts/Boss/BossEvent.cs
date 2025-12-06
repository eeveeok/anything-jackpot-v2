using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossEvent : MonoBehaviour
{
    [Header("�÷��̾� �� üũ����Ʈ")]
    public BasePlayerController player;
    public Transform checkPoint;

    [Header("ī�޶� ����")]
    public CinemachineVirtualCamera virtualCamera;
    public Collider2D bossCameraConfiner;

    [Header("ī�޶� �� ����")]
    public float targetOrthoSize = 12.5f;
    public float zoomDuration = 3.0f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("ī�޶� ��鸲 ȿ��")]
    public float shakeIntensity = 1.5f;
    public float shakeFrequency = 2.0f;
    public float shakeDuration = 1.5f;
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("���ӿ�����Ʈ ����")]
    public GameObject bossInvisibleWalls;
    public GameObject bossObject; // ���� ������Ʈ

    [Header("�ð� ȿ��")]
    public float timeSlowFactor = 0.3f; // �ð� ������ ����
    public float timeSlowDuration = 1.0f;

    [Header("�̺�Ʈ ����")]
    public bool eventTriggered = false;

    private CinemachineConfiner2D confiner;
    private CinemachineBasicMultiChannelPerlin noise;
    private float originalOrthoSize;
    private bool isEventActive = false;

    void Start()
    {
        // ������Ʈ �ʱ�ȭ
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

        // 1. �÷��̾� �̵� �� ��Ʈ�� ����
        if (player != null)
        {
            // �÷��̾� ��Ʈ�� ��� ����
            player.enabled = false;

            // �÷��̾� ��ġ �̵�
            player.transform.position = checkPoint.position;

            // �÷��̾� ���� ����
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.isKinematic = true;
            }
        }

        // 2. ī�޶� ���� �ʱ�ȭ
        if (virtualCamera != null)
        {
            // �ٿ�� ����
            CinemachineConfiner2D confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null && bossCameraConfiner != null)
            {
                confiner.m_BoundingShape2D = bossCameraConfiner;
            }

            // ������ Ȱ��ȭ
            if (bossInvisibleWalls != null)
            {
                bossInvisibleWalls.SetActive(true);
            }
        }

        // 3. �ð� ������ ȿ��
        yield return StartCoroutine(SlowTimeEffect());

        // 4. ī�޶� �� �ƿ� + ��鸲 ȿ��
        yield return StartCoroutine(ZoomAndShakeEffect());

        // 5. �÷��̾� ��Ʈ�� ����
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }
            player.enabled = true;
        }

        // 6. ���� ���� �ִϸ��̼�
        if (bossObject != null)
        {
            yield return StartCoroutine(BossAppearanceAnimation());
        }

        isEventActive = false;

        // 7. Ʈ���� �ݶ��̴� ��Ȱ��ȭ (�� ���� ����)
        GetComponent<Collider2D>().enabled = false;
    }

    // �ð� ������ ȿ��
    private IEnumerator SlowTimeEffect()
    {
        float originalTimeScale = Time.timeScale;
        float elapsed = 0f;

        // �ð� ���� ��������
        while (elapsed < timeSlowDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (timeSlowDuration / 2);
            Time.timeScale = Mathf.Lerp(originalTimeScale, timeSlowFactor, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        // ���� �ð� ����
        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        yield return new WaitForSecondsRealtime(0.5f);

        // �ð� ���� ����
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

    // ī�޶� �� + ��鸲 ȿ��
    private IEnumerator ZoomAndShakeEffect()
    {
        float elapsed = 0f;
        float currentSize = virtualCamera.m_Lens.OrthographicSize;

        // ī�޶� ��鸲 Ȱ��ȭ
        if (noise != null)
        {
            noise.m_AmplitudeGain = shakeIntensity;
            noise.m_FrequencyGain = shakeFrequency;
        }

        // �� �ƿ� ȿ��
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / zoomDuration;
            float curveValue = zoomCurve.Evaluate(t);

            // �ε巯�� �� �ƿ�
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentSize, targetOrthoSize, curveValue);

            // ��鸲 ���� ����
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

        // ��鸲 ��Ȱ��ȭ
        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    // ���� ���� �ִϸ��̼�
    private IEnumerator BossAppearanceAnimation()
    {
        // ���� �ʱ�ȭ (��Ȱ��ȭ ���¿��� ����)
        bool wasActive = bossObject.activeSelf;
        bossObject.SetActive(false);

        // ��� ���
        yield return new WaitForSecondsRealtime(0.5f);

        // ���� Ȱ��ȭ
        bossObject.SetActive(true);

        // ������ �ִϸ��̼� (Ŀ���鼭 ����)
        Vector3 originalScale = bossObject.transform.localScale;
        bossObject.transform.localScale = Vector3.zero;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // ź�� ȿ��
            float scaleValue = ElasticOut(t);
            bossObject.transform.localScale = originalScale * scaleValue;

            yield return null;
        }

        bossObject.transform.localScale = originalScale;
    }

    // ź�� ȿ�� �Լ�
    private float ElasticOut(float t)
    {
        float p = 0.3f;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
    }
    void OnDestroy()
    {
        // �� ��ȯ �� �ð� ����
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}