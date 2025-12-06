using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM 설정")]
    public AudioSource bgmSource;
    [Range(0.1f, 5f)]
    public float bgmFadeDuration = 1.0f; // 페이드 전환 시간 (초)

    [Header("SFX 풀링 설정")]
    public GameObject sfxSourcePrefab;
    public int poolSize = 20;

    private List<AudioSource> sfxPool;
    private int poolCursor = 0;
    private Coroutine bgmFadeCoroutine; // 현재 실행 중인 페이드 코루틴 저장

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        sfxPool = new List<AudioSource>();
        GameObject poolParent = new GameObject("SFX_Pool_Container");
        poolParent.transform.SetParent(transform);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(sfxSourcePrefab, poolParent.transform);
            AudioSource source = obj.GetComponent<AudioSource>();
            obj.SetActive(false);
            sfxPool.Add(source);
        }
    }

    private AudioSource GetSFXSource()
    {
        AudioSource source = sfxPool[poolCursor];
        poolCursor = (poolCursor + 1) % poolSize;
        return source;
    }

    // =========================================================
    // [BGM 페이드 기능]
    // =========================================================

    public void PlayBGM(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null) return;

        // 같은 곡이 재생 중이라면 페이드 없이 볼륨만 조절하고 종료
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            bgmSource.volume = volume;
            return;
        }

        // 이전에 실행 중이던 페이드 코루틴이 있다면 중지 (꼬임 방지)
        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        // 페이드 코루틴 시작
        bgmFadeCoroutine = StartCoroutine(FadeBGMProcess(clip, volume));
    }

    IEnumerator FadeBGMProcess(AudioClip nextClip, float targetVolume)
    {
        float halfDuration = bgmFadeDuration * 0.5f;

        // 1. 페이드 아웃 (현재 재생 중인 곡이 있다면)
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float timer = 0f;

            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                // 현재 볼륨에서 0까지 서서히 줄임
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / halfDuration);
                yield return null;
            }
            bgmSource.volume = 0f;
            bgmSource.Stop();
        }

        // 2. 클립 교체
        bgmSource.clip = nextClip;
        bgmSource.Play();

        // 3. 페이드 인
        float fadeInTimer = 0f;
        while (fadeInTimer < halfDuration)
        {
            fadeInTimer += Time.deltaTime;
            // 0에서 목표 볼륨까지 서서히 키움
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, fadeInTimer / halfDuration);
            yield return null;
        }

        bgmSource.volume = targetVolume; // 최종 볼륨 확정
    }

    // =========================================================
    // [SFX 기능]
    // =========================================================

    // 2. 2D 효과음 (반환값 변경: void -> AudioSource)
    public AudioSource PlaySFX(AudioClip clip, float volume = 1.0f, bool loop = false)
    {
        if (clip == null) return null;

        AudioSource source = GetSFXSource();
        source.gameObject.SetActive(true);

        // 2D 설정
        source.spatialBlend = 0f;
        source.transform.position = Vector3.zero;

        // ★ PlayOneShot 대신 Clip을 교체하고 Play() 사용
        source.clip = clip;
        source.volume = volume;
        source.loop = loop; // 루프 여부 설정
        source.Play();

        return source; // ★ 소리를 내고 있는 스피커를 리턴해줌!
    }

    // 3. 3D 효과음 (반환값 변경: void -> AudioSource)
    public AudioSource PlaySFXAt(AudioClip clip, Vector3 position, float volume = 1.0f, bool loop = false)
    {
        if (clip == null) return null;

        AudioSource source = GetSFXSource();
        source.gameObject.SetActive(true);
        source.transform.position = position;

        // 3D 설정
        source.spatialBlend = 1f;
        source.spread = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 2f;
        source.maxDistance = 30f;

        // ★ Play() 사용
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        source.Play();

        return source; // ★ 스피커 리턴
    }
}