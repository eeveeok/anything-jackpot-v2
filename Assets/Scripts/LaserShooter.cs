using System.Collections;
using UnityEngine;

public class LaserShooter : BasePlayerController
{
    [Header("레이저 설정")]
    public GameObject laserBeamPrefab;
    public float laserSpawnDistance = 0.1f;

    [Header("반동 세부 설정")]
    public float initialRecoilForce = 17f;
    public float continuousRecoilForce = 1000f;
    public float maxRecoilVelocity = 1000f;
    public float verticalRecoilMultiplier = 0.5f;
    public float recoilSmoothing = 0.1f;

    [Header("물리 설정")]
    public float normalDrag = 0.5f;
    public float bounceDrag = 3f;

    // 레이저 관련
    private bool isLaserActive = false;
    private GameObject currentLaser;

    // 반동 관련
    private Vector2 recoilVelocity;

    // 바운스 상태
    [HideInInspector] public bool isBounced = false;

    // 애니메이터 파라미터
    private const string IS_LASER_PARAM = "IsLaser";

    protected override void FixedUpdate()
    {
        Move();
        CheckGround();

        if (isLaserActive) ApplyContinuousRecoil();
    }

    // 이동 속도 계산 (레이저 발사 중 감속)
    protected override float GetCurrentMoveSpeed()
    {

        return isLaserActive ? moveSpeed * 0.6f : moveSpeed;
    }


    // 바운스 상태 확인
    protected override bool IsBouncing()
    {
        return isBounced;
    }

    // 특수 입력 처리 (레이저 발사)
    protected override void HandleSpecialInput()
    {
        if (IsGamePaused()) return;

        if (Input.GetMouseButtonDown(0) && !isLaserActive)
        {
            StartLaser();
            if (animator != null) animator.SetBool(FIRE_PARAM, true);
        }

        if ((Input.GetKeyUp(KeyCode.X) || Input.GetMouseButtonUp(0)) && isLaserActive)
        {
            StopLaser();
            if (animator != null) animator.SetBool(FIRE_PARAM, false);
        }
    }

    // 애니메이션 업데이트
    protected override void UpdateAnimation()
    {
    }

    // 레이저 발사
    private void StartLaser()
    {
        if (laserBeamPrefab == null) return;

        isLaserActive = true;
        Vector2 direction = GetMouseDirection();

        Vector2 spawnPos = (Vector2)transform.position + direction * laserSpawnDistance;
        currentLaser = Instantiate(laserBeamPrefab, spawnPos, Quaternion.identity);

        LaserBeam laserScript = currentLaser.GetComponent<LaserBeam>();
        if (laserScript != null)
        {
            laserScript.direction = direction;
            laserScript.characterCenter = transform;
            laserScript.mainCamera = mainCamera;
            laserScript.spawnDistance = laserSpawnDistance;
        }

        ApplyInitialRecoil(direction);
    }

    // 레이저 중지
    private void StopLaser()
    {
        isLaserActive = false;

        if (currentLaser != null)
        {
            LaserBeam laserScript = currentLaser.GetComponent<LaserBeam>();
            if (laserScript != null) laserScript.SetActive(false);

            Destroy(currentLaser, 0.1f);
            currentLaser = null;
        }
    }

    // 특수 능력 중지
    protected override void StopSpecialAbility()
    {
        StopLaser();
    }

    // 초기 반동
    private void ApplyInitialRecoil(Vector2 laserDirection)
    {
        float currentRecoilForce = isBounced ? initialRecoilForce * 0.3f : initialRecoilForce;
        float currentVerticalMultiplier = isBounced ? verticalRecoilMultiplier * 0.3f : verticalRecoilMultiplier;

        Vector2 recoilDirection = -laserDirection.normalized;
        Vector2 recoil = recoilDirection * currentRecoilForce;
        recoil.y *= currentVerticalMultiplier;

        rb.AddForce(recoil, ForceMode2D.Impulse);
        ClampVelocity();
    }

    // 지속적 반동
    private void ApplyContinuousRecoil()
    {
        float currentRecoilForce = isBounced ? continuousRecoilForce * 0.3f : continuousRecoilForce;
        float currentVerticalMultiplier = isBounced ? verticalRecoilMultiplier * 0.3f : verticalRecoilMultiplier * 0.3f;

        Vector2 recoilDirection = -GetMouseDirection().normalized;
        Vector2 targetRecoil = recoilDirection * currentRecoilForce;
        targetRecoil.y *= currentVerticalMultiplier;

        Vector2 smoothRecoil = Vector2.SmoothDamp(
            Vector2.zero, targetRecoil, ref recoilVelocity, recoilSmoothing
        );

        rb.AddForce(smoothRecoil, ForceMode2D.Force);
        ClampVelocity();
    }

    // 속도 제한
    private void ClampVelocity()
    {
        if (rb.velocity.magnitude > maxRecoilVelocity)
            rb.velocity = rb.velocity.normalized * maxRecoilVelocity;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (isBounced && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Spike")))
        {
            isBounced = false;
        }
    }

    // 공개 프로퍼티
    public bool IsLaserActive => isLaserActive;
}