using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D coll;
    private LayerMask ground;
    private LayerMask enemyLayer;

    [SerializeField] private AudioSource footstep;
    [SerializeField] private AudioSource coinAudio;
    [SerializeField] private AudioSource jumpAudio;

    private enum State { idle, running, jumping, falling, hurt };
    private State state = State.idle;

    private int speed = 9;
    private float jumpForce = 25f;
    private float airControl = 0.8f;
    private float hurtForce = 10f;
    private bool canTakeDamage = true;

    // Respawn support
    private Vector2 lastGroundedPosition;
    private Vector2 checkpointPosition;
    private bool hasGroundedOnce = false;
    private bool hasCheckpoint = false;
    private float groundedTimer = 0f;
    private float minGroundedTime = 0.2f; 

    // Dash
    private float dashSpeed = 20f;
    private float dashDuration = 0.3f;
    private float dashCooldown = 1f;
    private float lastDashTime = -1f;
    private bool isDashing = false;

    // Double jump & wall
    private bool canDoubleJump = false;
    private bool isWallSliding = false;
    private float wallSlideSpeed = 2f;
    private float wallJumpForceX = 12f;
    private float wallJumpForceY = 20f;
    private int wallDirection = 0; // -1 left, 1 right, 0 none

    // Powerups Timers & State
    private bool isMagnetActive = false;
    private bool isDoubleJumpPowerupActive = false; // New timer based logic
    private bool isDashPowerupActive = false; // New timer based logic
    private bool hasPermanentDoubleJump = false;
    private bool hasPermanentDash = false;
    
    private float magnetRange = 10f;
    private float magnetSpeed = 10f;
    private bool warnedMissingPermanentUI = false;

    // Stamina / Sprint (Green Bar)
    [Header("Stamina (Green Bar / Sprint)")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainPerSec = 65f;
    [SerializeField] private float staminaRegenPerSec = 10f;
    [SerializeField] private float sprintMultiplier = 2.5f;
    [SerializeField] private float sprintRampRate = 5f; 
    [SerializeField] private float minStaminaToSprint = 5f;
    [SerializeField] private Image manaBarFill; 
    [SerializeField] private Text manaText;

    private float stamina;
    private bool isSprinting;
    private float sprintFactor = 1f; 

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        coll = GetComponent<CircleCollider2D>();

        ground = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("EnemyLayer");

        stamina = maxStamina;
        UpdateStaminaUI();

        // Initialize last grounded position to spawn
        lastGroundedPosition = transform.position;
        checkpointPosition = transform.position;

        ApplyPersistentUnlocks();
    }

    private void Update()
    {
        if (state != State.hurt && !isDashing)
        {
            Movement();
        }

        HandleSprintAndStamina();

        // Dash only if Powerup is active
        if (isDashPowerupActive && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDash());
        }

        animator.SetInteger("state", (int)state);
        AnimationState();

        if (isMagnetActive)
        {
            HandleMagnet();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectible")
        {
            if (coinAudio != null) coinAudio.Play();

            Destroy(collision.gameObject);

            if (EnsurePermanentUI())
            {
                PermanentUI.perm.coins++;
                if (PermanentUI.perm.coinText != null)
                {
                    PermanentUI.perm.coinText.text = PermanentUI.perm.coins.ToString();
                }
            }
        }

        if (collision.tag == "Cherry")
        {
            Destroy(collision.gameObject);
            if (HealthSystem.Instance != null)
            {
                HealthSystem.Instance.HealDamage(5f);
            }
        }

        if (collision.tag == "Enemy")
        {
            if (canTakeDamage)
            {
                TakeDamage();
                if (collision.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-hurtForce, hurtForce);
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, hurtForce);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            // Check specifically for Bat first
            Bat bat = other.gameObject.GetComponent<Bat>();
            if (bat != null)
            {
                bool isBelow = transform.position.y < other.transform.position.y;
                if (isBelow) 
                {
                    bat.Die(); 
                    rb.velocity = new Vector2(rb.velocity.x, -2f); 
                }
                return; 
            }

            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            
            if (isDashing)
            {
                enemy.JumpedOn();
                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(coll.bounds.center, Vector2.down, 1.3f, enemyLayer);
                if (hit.collider != null || state == State.falling)
                {
                    state = State.falling;
                    enemy.JumpedOn();
                    Jump();
                    // Reset double jump IF powerup is active
                    canDoubleJump = isDoubleJumpPowerupActive;
                }
                else
                {
                    if (canTakeDamage)
                    {
                        TakeDamage();
                        if (other.gameObject.transform.position.x > transform.position.x)
                        {
                            rb.velocity = new Vector2(-hurtForce, hurtForce);
                        }
                        else
                        {
                            rb.velocity = new Vector2(hurtForce, hurtForce);
                        }
                    }
                }
            }
        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxis("Horizontal");
        bool jumping = Input.GetButtonDown("Jump");
        
        // Dedicated downward grounded check
        bool isGroundedDown = Physics2D.Raycast(coll.bounds.center, Vector2.down, 0.9f, ground);
        
        if (isGroundedDown)
        {
            groundedTimer += Time.deltaTime;
            if (groundedTimer >= minGroundedTime)
            {
                Vector2 safePos = transform.position;
                if (lastGroundedPosition != safePos) 
                {
                    lastGroundedPosition = safePos;
                    hasGroundedOnce = true;
                }
            }
        }
        else
        {
            groundedTimer = 0f;
        }

        float currentSpeed = speed * sprintFactor;

        // Check for breakable tiles underneath
        if (isGroundedDown)
        {
            RaycastHit2D tileHit = Physics2D.Raycast(coll.bounds.center, Vector2.down, 0.8f, ground);
            if (tileHit.collider != null)
            {
                Tilemap tilemap = tileHit.collider.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    BreakableTile breakable = tilemap.GetComponent<BreakableTile>();
                    if (breakable != null)
                    {
                        breakable.BreakTileAtPosition(tileHit.point);
                    }
                }
            }
        }

        // Wall detection
        bool isTouchingWallLeft = Physics2D.Raycast(coll.bounds.center, Vector2.left, 0.5f, ground);
        bool isTouchingWallRight = Physics2D.Raycast(coll.bounds.center, Vector2.right, 0.5f, ground);

        // Wall slide
        if (!isGroundedDown && (isTouchingWallLeft || isTouchingWallRight) && rb.velocity.y < 0)
        {
            isWallSliding = true;
            wallDirection = isTouchingWallLeft ? -1 : 1;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            // Allow double jump IF powerup is active
            canDoubleJump = isDoubleJumpPowerupActive;
        }
        else
        {
            isWallSliding = false;
            wallDirection = 0;
        }

        if (state != State.hurt)
        {
            if (hDirection < 0 && !isGroundedDown)
            {
                rb.velocity = new Vector2(-currentSpeed * airControl, rb.velocity.y);
            }
            else if (hDirection > 0 && !isGroundedDown)
            {
                rb.velocity = new Vector2(currentSpeed * airControl, rb.velocity.y);
            }
            else if (hDirection < 0 && isGroundedDown)
            {
                rb.velocity = new Vector2(-currentSpeed, rb.velocity.y);
            }
            else if (hDirection > 0 && isGroundedDown)
            {
                rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
                transform.localScale = new Vector2(1, 1);
            }
            else if (hDirection == 0 && isGroundedDown)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if (jumping)
        {
            RaycastHit2D hit = Physics2D.Raycast(coll.bounds.center, Vector2.down, 0.8f, ground);
            if (hit.collider != null)
            {
                Jump();
                // Reset double jump IF powerup is active
                canDoubleJump = isDoubleJumpPowerupActive;
            }
            else if (isWallSliding)
            {
                rb.velocity = new Vector2(-wallDirection * wallJumpForceX, wallJumpForceY);
                state = State.jumping;
                if (jumpAudio != null) jumpAudio.Play();
                isWallSliding = false;
                canDoubleJump = isDoubleJumpPowerupActive;
            }
            else if (canDoubleJump && (state == State.jumping || state == State.falling))
            {
                Jump();
                canDoubleJump = false; // Used double jump
            }
        }

        if (hDirection < 0)
        {
            transform.localScale = new Vector2(-1, 1);
        }
        else if (hDirection > 0)
        {
            transform.localScale = new Vector2(1, 1);
        }
    }

    private void Jump()
    {
        if (jumpAudio != null) jumpAudio.Play();
        rb.velocity = new Vector2(rb.velocity.x / 2, jumpForce);
        state = State.jumping;
    }

    private void AnimationState()
    {
        if (state == State.jumping)
        {
            if (rb.velocity.y < 0.1f) state = State.falling;
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
                canDoubleJump = false;
            }
        }
        else if (state == State.hurt)
        {
            if (Mathf.Abs(rb.velocity.x) < 2f) state = State.idle;
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f)
        {
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        sprintFactor = 1f; // Reset sprint factor
        UpdateStaminaUI();
        
        lastDashTime = Time.time;
        canTakeDamage = false;

        float hDirection = Input.GetAxis("Horizontal");
        float dashDir = (Mathf.Abs(hDirection) > 0.01f) ? Mathf.Sign(hDirection) : Mathf.Sign(transform.localScale.x == 0 ? 1 : transform.localScale.x);

        float dashEndTime = Time.time + dashDuration;
        while (Time.time < dashEndTime)
        {
            rb.velocity = new Vector2(dashDir * dashSpeed, rb.velocity.y);
            yield return null;
        }

        isDashing = false;
        canTakeDamage = true;
    }

    private void HandleSprintAndStamina()
    {
        if (isDashing)
        {
            isSprinting = false;
            sprintFactor = 1f;
            UpdateStaminaUI();
            return;
        }

        // Changed Sprint to LeftShift since Dash is on Ctrl
        bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float h = Input.GetAxis("Horizontal");
        bool isMoving = Mathf.Abs(h) > 0.01f;
        bool isGrounded = coll.IsTouchingLayers(ground);

        if (sprintKey && isMoving && isGrounded && stamina > minStaminaToSprint)
            isSprinting = true;
        else
            isSprinting = false;

        // Smooth sprint
        float targetSprint = isSprinting ? sprintMultiplier : 1f;
        sprintFactor = Mathf.MoveTowards(sprintFactor, targetSprint, sprintRampRate * Time.deltaTime);

        if (isSprinting)
        {
            stamina -= staminaDrainPerSec * Time.deltaTime;
            if (stamina <= 0f)
            {
                stamina = 0f;
                isSprinting = false;
            }
        }
        else
        {
            stamina += staminaRegenPerSec * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }

        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        float ratio = stamina / maxStamina;

        if (manaBarFill != null)
            manaBarFill.fillAmount = ratio;

        if (manaText != null)
            manaText.text = Mathf.CeilToInt(ratio * 100f).ToString();
    }

    private void Footstep()
    {
        if (footstep != null) footstep.Play();
    }

    private void TakeDamage()
    {
        state = State.hurt;

        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.TakeDamage(10f);
            if (HealthSystem.Instance.hitPoint > 0)
            {
                StartCoroutine(DamageCooldown());
            }
        }
        else
        {
            Die();
        }
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(1.5f);
        canTakeDamage = true;
    }

    private void Die()
    {
        if (PermanentUI.perm != null)
        {
            PermanentUI.perm.RestoreCoinsToLevelStart();
        }
        // Reset health (HealthSystem may persist via DontDestroyOnLoad Canvas)
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.ResetHealth();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // Checkpoint & Respawn Functions
    public void SetCheckpoint(Vector2 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
        Debug.Log($"Checkpoint set at {position}");
    }

    public void RespawnToLastPlatform()
    {
        Debug.Log($"RespawnToLastPlatform called! hasCheckpoint={hasCheckpoint}, hasGroundedOnce={hasGroundedOnce}");
        
        Vector2 target;
        if (hasCheckpoint)
        {
            target = checkpointPosition;
        }
        else if (hasGroundedOnce)
        {
            target = lastGroundedPosition;
        }
        else
        {
            target = transform.position;
        }
        
        Vector2 safePos = target + Vector2.up * 1.0f;
        transform.position = safePos;
        rb.velocity = Vector2.zero;
        
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        canTakeDamage = false;
        isDashing = false;
        state = State.idle;
        groundedTimer = 0f;

        yield return new WaitForSeconds(1.0f);
        canTakeDamage = true;
    }

    // Powerup Activations
    public void ActivateMagnet(float duration)
    {
        if (_magnetRoutine != null) StopCoroutine(_magnetRoutine);
        _magnetRoutine = StartCoroutine(MagnetRoutine(duration));
    }

    public void ActivateDoubleJump(float duration)
    {
        if (_doubleJumpRoutine != null) StopCoroutine(_doubleJumpRoutine);
        _doubleJumpRoutine = StartCoroutine(DoubleJumpRoutine(duration));
    }

    public void ActivateDashPowerup(float duration)
    {
        if (_dashRoutine != null) StopCoroutine(_dashRoutine);
        _dashRoutine = StartCoroutine(DashRoutine(duration));
    }

    private Coroutine _magnetRoutine;
    private Coroutine _doubleJumpRoutine;
    private Coroutine _dashRoutine;

    private IEnumerator MagnetRoutine(float duration)
    {
        isMagnetActive = true;
        Debug.Log("Magnet activated for " + duration + "s");
        yield return new WaitForSeconds(duration);
        isMagnetActive = false;
        Debug.Log("Magnet deactivated");
    }

    private IEnumerator DoubleJumpRoutine(float duration)
    {
        isDoubleJumpPowerupActive = true;
        // Make double jump available immediately (even if activated mid-air)
        canDoubleJump = true;
        Debug.Log("DoubleJump activated for " + duration + "s");
        yield return new WaitForSeconds(duration);
        if (!hasPermanentDoubleJump)
        {
            isDoubleJumpPowerupActive = false;
            // After buff ends, remove remaining double jump if any
            canDoubleJump = false;
            Debug.Log("DoubleJump deactivated");
        }
    }

    private IEnumerator DashRoutine(float duration)
    {
        isDashPowerupActive = true;
        Debug.Log("Dash activated for " + duration + "s");
        yield return new WaitForSeconds(duration);
        if (!hasPermanentDash)
        {
            isDashPowerupActive = false;
            Debug.Log("Dash deactivated");
        }
    }

    private void HandleMagnet()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, magnetRange);
        foreach (Collider2D col in colliders)
        {
            if (col.tag == "Collectible")
            {
                col.transform.position = Vector2.MoveTowards(col.transform.position, transform.position, magnetSpeed * Time.deltaTime);
            }
        }
    }

    private bool EnsurePermanentUI()
    {
        if (PermanentUI.perm != null) return true;

        var ui = FindObjectOfType<PermanentUI>();
        if (ui != null)
        {
            PermanentUI.perm = ui;
            return true;
        }

        if (!warnedMissingPermanentUI)
        {
            warnedMissingPermanentUI = true;
            Debug.LogWarning("PermanentUI not found; coin not counted.");
        }
        return false;
    }

    private void ApplyPersistentUnlocks()
    {
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.DoubleJumpUnlocked)
        {
            EnablePermanentDoubleJump();
        }
        if (InventoryManager.Instance.DashUnlocked)
        {
            EnablePermanentDash();
        }
    }

    public void EnablePermanentDoubleJump()
    {
        hasPermanentDoubleJump = true;
        isDoubleJumpPowerupActive = true;
        canDoubleJump = true;
    }

    public void EnablePermanentDash()
    {
        hasPermanentDash = true;
        isDashPowerupActive = true;
    }
}
