using System.Collections;
using UnityEngine;

public abstract class BasePlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 7f;
    public float jumpForce = 10f;

    [Header("체크포인트")]
    public Transform checkpoint;
    public float respawnDelay = 2.0f;

    [Header("무적 상태")]
    public float invincibilityDuration = 2f;
    public float blinkInterval = 0.1f;

    [Header("물리 설정")]
    public PhysicsMaterial2D airMaterial;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    [Header("사운드 설정")]
    public AudioClip damageSound1;       // 피격 소리1
    public AudioClip damageSound2;       // 피격 소리2
    public AudioClip walkSound;        // 걷는 소리 (루프)
    public AudioClip respawnSound;        // 스폰 소리
    public AudioClip spikeSound;          // 스파이크 피격 소리
    public AudioClip tractorBeamSound;    // 트랙터빔 피격 소리

    private AudioSource walkSource;

    // 컴포넌트 참조
    protected Rigidbody2D rb;
    protected Camera mainCamera;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    // 상태 변수
    protected bool isGrounded;
    protected bool isFacingRight = true;
    protected bool isDead = false;
    protected bool isInvincible = false;
    protected float horizontalInput;

    // 위치 추적
    protected Vector3 lastGroundedPosition;
    protected bool wasGroundedLastFrame;

    // 매니저 참조
    protected DialogueManager dialogueManager;
    protected PauseManager pauseManager;
    protected HealthUIManager hpManager;

    // 애니메이터 파라미터 문자열
    protected const string JUMP_PARAM = "Jump";
    protected const string SPEED_PARAM = "Speed";
    protected const string FIRE_PARAM = "Fire";

    // 공개 프로퍼티
    public bool IsGrounded => isGrounded;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;

    protected virtual void Start()
    {
        InitializeComponents();
        InitializePosition();

        //스폰 사운드 재생
        SoundManager.Instance.PlaySFX(respawnSound, 0.1f);
    }

    protected virtual void Update()
    {
        if (IsGamePaused()) return;

        HandleBaseInput();
        UpdateFacingDirection();
        UpdateAnimation();
        HandleSpecialInput();

        // 걷는 소리 처리
        HandleWalkSound();
    }

    protected virtual void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        dialogueManager = FindObjectOfType<DialogueManager>();
        pauseManager = FindObjectOfType<PauseManager>();
        hpManager = FindObjectOfType<HealthUIManager>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (dialogueManager != null) dialogueManager.ActiveDialog();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && airMaterial != null) col.sharedMaterial = airMaterial;
    }

    private void InitializePosition()
    {
        lastGroundedPosition = transform.position;
        wasGroundedLastFrame = true;
    }

    protected bool IsGamePaused()
    {
        return (dialogueManager != null && dialogueManager.isDialogueActive) ||
               (pauseManager != null && pauseManager.isPaused) ||
               isDead;
    }

    private void HandleBaseInput()
    {
        if (IsGamePaused())
        {
            horizontalInput = 0f;
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    protected virtual void Jump()
    {
        rb.sharedMaterial = airMaterial;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    protected virtual void Move()
    {
        float currentMoveSpeed = GetCurrentMoveSpeed();
        Vector2 newVelocity = new Vector2(horizontalInput * currentMoveSpeed, rb.velocity.y);

        if (!IsBouncing()) rb.velocity = newVelocity;

        if (animator != null) animator.SetFloat(SPEED_PARAM, newVelocity.magnitude);
    }

    protected virtual float GetCurrentMoveSpeed()
    {
        return moveSpeed;
    }

    protected virtual bool IsBouncing()
    {
        return false;
    }

    protected virtual void CheckGround()
    {
        bool previouslyGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (animator != null) animator.SetBool(JUMP_PARAM, !isGrounded);

        if (isGrounded)
        {
            rb.sharedMaterial = null;
            UpdateGroundedPosition();
        }
        else
        {
            UpdateAirbornePosition();
        }

        wasGroundedLastFrame = isGrounded;
    }

    private void UpdateGroundedPosition()
    {
        if (!wasGroundedLastFrame)
        {
            lastGroundedPosition = transform.position;
        }
        else if (Mathf.Abs(rb.velocity.x) < 0.1f)
        {
            lastGroundedPosition = transform.position;
        }
    }

    private void UpdateAirbornePosition()
    {
        if (wasGroundedLastFrame && !isGrounded)
        {
            lastGroundedPosition = transform.position;
        }
    }

    protected virtual void UpdateFacingDirection()
    {
        if (mainCamera == null) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        bool shouldFaceRight = mousePosition.x > transform.position.x;

        if (shouldFaceRight != isFacingRight) Flip();
    }

    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // 추상 메서드 - 각 캐릭터별 구현
    protected abstract void UpdateAnimation();
    protected abstract void HandleSpecialInput();

    public virtual void PlayerDie()
    {
        if (isDead || isInvincible) return;

        isDead = true;
        rb.simulated = false;

        // 피격 소리 재생
        PlayRandomDamgeSound();

        if (walkSource != null)
        {
            walkSource.Stop();
            walkSource = null;
        }

        // 목숨 감소
        if (hpManager != null)
        {
            hpManager.TakeDamage(1);
        }

        // 애니메이션 중지
        ResetAllAnimations();

        // 특수 능력 중지
        StopSpecialAbility();

        // 보이지 않게
        SetPlayerVisible(false);

        // 부활 대기
        StartCoroutine(RespawnDelay());
    }

    protected virtual void ResetAllAnimations()
    {
        if (animator != null)
        {
            // 기본 애니메이션 파라미터 초기화
            animator.SetFloat(SPEED_PARAM, 0f);
            animator.SetBool(JUMP_PARAM, false);
            animator.SetBool(FIRE_PARAM, false);
        }
    }

    protected virtual void StopSpecialAbility() { }

    private IEnumerator RespawnDelay()
    {

        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    private void Respawn()
    {
        // 체크포인트 또는 마지막 착지 위치로 부활
        Vector3 respawnPos = checkpoint != null ? checkpoint.position : lastGroundedPosition;
        respawnPos.y += 2.5f;

        transform.position = respawnPos;
        rb.velocity = Vector2.zero;
        SetPlayerVisible(true);

        // 사운드 재생
        SoundManager.Instance.PlaySFX(respawnSound, 0.1f);

        isDead = false;
        isGrounded = false;
        rb.simulated = true;
        wasGroundedLastFrame = false;

        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        StartCoroutine(BlinkEffect());

        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;

        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    private IEnumerator BlinkEffect()
    {
        float endTime = Time.time + invincibilityDuration;

        while (Time.time < endTime && isInvincible)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    protected void SetPlayerVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (animator != null) animator.enabled = visible;
        rb.isKinematic = !visible;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvincible) return;

        if (collision.CompareTag("BossAttack") || collision.CompareTag("Spike") || collision.CompareTag("TractorBeam"))
        {
            if (collision.CompareTag("Spike") || collision.CompareTag("BossAttack"))
            {
                SoundManager.Instance.PlaySFX(spikeSound, 0.1f);
            }
            else if (collision.CompareTag("TractorBeam"))
            {
                SoundManager.Instance.PlaySFX(tractorBeamSound, 0.1f);
            }
            PlayerDie();
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 각 캐릭터별 충돌 처리
    }

    protected Vector2 GetMouseDirection()
    {
        if (mainCamera == null) return Vector2.right;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(mousePosition.x - transform.position.x,
                          mousePosition.y - transform.position.y).normalized;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    void PlayRandomDamgeSound()
    {
        if (damageSound1 == null && damageSound2 == null) return;

        AudioClip clipToPlay; // 실제로 틀 소리를 담을 임시 변수

        //50% 확률로 선택
        if (damageSound1 != null && damageSound2 != null)
            clipToPlay = (Random.value > 0.5f) ? damageSound1 : damageSound2;
        else
            // 하나만 있으면 있는 거 선택
            clipToPlay = (damageSound1 != null) ? damageSound1 : damageSound2;

        // 최종 선택된 소리 재생
        SoundManager.Instance.PlaySFXAt(clipToPlay, transform.position, 4.0f);
    }

    protected void HandleWalkSound()
    {
        // 1. 소리가 재생될 조건: 땅에 있고(isGrounded) && 속도가 있으며 && 죽지 않음
        bool isMoving = isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f && !isDead;

        if (isMoving)
        {
            // 걷고 있는데 소리가 안 나고 있다면 -> 켠다 (Loop)
            if (walkSource == null)
            {
                // SoundManager를 통해 루프 재생하고, 해당 스피커를 변수에 저장
                walkSource = SoundManager.Instance.PlaySFX(walkSound, 0.2f, true);
            }
        }
        else
        {
            // 멈췄거나 공중인데 소리가 나고 있다면 -> 끈다
            if (walkSource != null)
            {
                walkSource.Stop();
                walkSource = null;
            }
        }
    }
}