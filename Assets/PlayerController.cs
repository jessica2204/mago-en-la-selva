using UnityEngine;

/// <summary>
/// Top-down player controller for Mario en la Selva.
/// Handles movement in 4 directions, coin collection feedback,
/// damage with invincibility frames and sprite blink effect.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Invincibility after damage")]
    [SerializeField] private float invincibilityDuration = 1.5f;

    // ── Private references ───────────────────────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    private bool isInvincible = false;
    private bool isAlive = true;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isAlive) return;

        // Read input from keyboard (WASD or arrow keys)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;

        // Flip sprite to face movement direction
        if (horizontal > 0) sr.flipX = false;
        if (horizontal < 0) sr.flipX = true;
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;
        // Move player using physics
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
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

    public float GetMoveSpeed() => moveSpeed;

    /// <summary>Increases move speed — called by GameManager for difficulty scaling.</summary>
    public void SetMoveSpeed(float newSpeed) => moveSpeed = newSpeed;
}
