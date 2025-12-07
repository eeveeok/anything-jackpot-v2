using System.Collections;
using UnityEngine;

public class CannonShooter : BasePlayerController
{
    [Header("대포 발사 설정")]
    public GameObject cannonEffectPrefab;
    public float recoilForce = 30f;
    public float recoilDuration = 1f;
    public float maxRecoilVelocity = 50f;
    public float shootCooldown = 1f;
    public float verticalRecoilMultiplier = 0.6f;
    public float effectSpawnDistance = 0.5f;
    public float effectDuration = 0.5f;

    [Header("사운드 설정")]
    public AudioClip cannonSound;

    // 발사 관련
    private bool canShoot = true;
    private float lastShootTime = 0f;

    // 애니메이터 파라미터
    private const string IS_SHOOTING_PARAM = "IsShooting";

    protected override void Start()
    {
        base.Start();
        canShoot = true;
    }

    // 특수 입력 처리
    protected override void HandleSpecialInput()
    {
        if (IsGamePaused()) return;

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            //사운드 재생
            SoundManager.Instance.PlaySFX(cannonSound, 0.05f);
            ShootCannon();
        }
    }

    // 이동 처리 (반동 중 x축 속도 보존)
    protected override void Move()
    {
        bool isRecoiling = Time.time - lastShootTime < recoilDuration * 0.5f;

        if (!isRecoiling && isGrounded)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

        if (animator != null) animator.SetFloat(SPEED_PARAM, Mathf.Abs(rb.velocity.x));
    }

    // 애니메이션 업데이트
    protected override void UpdateAnimation()
    {
        if (animator != null)
        {
            bool isShooting = Time.time - lastShootTime < recoilDuration * 0.5f;
            animator.SetBool(IS_SHOOTING_PARAM, isShooting);
        }
    }

    // 대포 발사
    private void ShootCannon()
    {
        Vector2 direction = GetMouseDirection();

        // 발사 효과 생성
        if (cannonEffectPrefab != null)
        {
            Vector2 effectPos = (Vector2)transform.position + direction * effectSpawnDistance;
            GameObject effect = Instantiate(cannonEffectPrefab, effectPos, Quaternion.identity);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            effect.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Destroy(effect, effectDuration);
        }

        ApplyCannonRecoil(direction);
        lastShootTime = Time.time;
        StartCoroutine(ShootCooldownRoutine());
    }

    // 대포 반동
    private void ApplyCannonRecoil(Vector2 direction)
    {
        Vector2 recoilDirection = -direction;
        Vector2 recoil = recoilDirection * recoilForce;
        recoil.y *= verticalRecoilMultiplier;

        // 아래 방향에서 반동 강화
        float angle = Vector2.SignedAngle(Vector2.down, direction);
        if (Mathf.Abs(angle) <= 15f) recoil *= 1.3f;

        rb.AddForce(recoil, ForceMode2D.Impulse);

        if (rb.velocity.magnitude > maxRecoilVelocity)
            rb.velocity = rb.velocity.normalized * maxRecoilVelocity;

        StartCoroutine(ContinuousRecoilRoutine(recoilDirection));
    }

    // 지속적 반동 코루틴
    private IEnumerator ContinuousRecoilRoutine(Vector2 recoilDirection)
    {
        float elapsed = 0f;
        float half = recoilDuration * 0.5f;

        while (elapsed < half)
        {
            float t = elapsed / half;
            float curve = 1f - Mathf.Pow(1f - t, 3f);

            Vector2 add = recoilDirection * (recoilForce * 0.5f * curve * Time.fixedDeltaTime);
            add.y = 0f; // y축 반동 제거

            rb.AddForce(add, ForceMode2D.Force);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, 1f));
    }

    // 발사 쿨다운
    private IEnumerator ShootCooldownRoutine()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    // 공개 프로퍼티
    public bool CanShoot => canShoot;
}