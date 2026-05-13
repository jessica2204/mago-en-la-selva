using UnityEngine;

/// <summary>
/// Handles all player movement in 4 directions (top-down),
/// damage with invincibility frames, sprite blink effect,
/// and projectile shooting mechanic.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;  // Assign Projectile prefab in Inspector
    [SerializeField] private float shootCooldown = 0.5f;   // Seconds between shots

    [Header("Invincibility after damage")]
    [SerializeField] private float invincibilityDuration = 1.5f;

    // ── Private references ────────────────────────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right; // Default shoot direction

    private bool isInvincible  = false;
    private bool isAlive       = true;
    private float shootTimer   = 0f;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isAlive) return;

        HandleMovement();
        HandleShooting();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    // ── Movement ──────────────────────────────────────────────────────────

    /// <summary>Reads directional input and updates last known direction.</summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;

        // Store last direction for shooting aim
        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput;

        // Flip sprite based on horizontal direction
        if (horizontal > 0) sr.flipX = false;
        if (horizontal < 0) sr.flipX = true;
    }

    // ── Shooting ──────────────────────────────────────────────────────────

    /// <summary>
    /// Fires a projectile in the last movement direction when Space is pressed.
    /// A cooldown prevents spamming.
    /// </summary>
    private void HandleShooting()
    {
        // Decrease cooldown timer
        if (shootTimer > 0)
            shootTimer -= Time.deltaTime;

        // Fire when Space is pressed and cooldown is ready
        if (Input.GetKeyDown(KeyCode.Space) && shootTimer <= 0f)
        {
            FireProjectile();
            shootTimer = shootCooldown;
        }
    }

    /// <summary>Instantiates a projectile at the player's position.</summary>
    private void FireProjectile()
    {
        if (projectilePrefab == null) return;

        // Create the projectile at player position
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // Set its travel direction
        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.SetDirection(lastMoveDirection);
    }

    // ── Damage ────────────────────────────────────────────────────────────

    /// <summary>Called by EnemyAI when the enemy touches the player.</summary>
    public void TakeDamage()
    {
        if (isInvincible || !isAlive) return;
        GameManager.Instance.LoseLife();
        StartCoroutine(InvincibilityFrames());
    }

    /// <summary>Blinks the sprite during the invincibility window.</summary>
    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        sr.enabled   = true;
        isInvincible = false;
    }

    /// <summary>Stops the player when the game ends.</summary>
    public void SetAlive(bool alive)
    {
        isAlive = alive;
        if (!alive) rb.velocity = Vector2.zero;
    }

    public float GetMoveSpeed()              => moveSpeed;
    public void  SetMoveSpeed(float speed)   => moveSpeed = speed;
}
