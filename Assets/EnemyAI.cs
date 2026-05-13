using UnityEngine;

/// <summary>
/// Enemy monkey AI for the top-down game.
/// Chases the player directly. Speed increases over time via GameManager.
/// Attach to each Monkey prefab.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 2f;

    // ── Private references ───────────────────────────────────────────────
    private Transform player;
    private SpriteRenderer sr;
    private bool isActive = true;

    // ─────────────────────────────────────────────────────────────────────
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Find the player automatically by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (!isActive || player == null) return;
        ChasePlayer();
    }

    /// <summary>Moves the enemy directly toward the player each frame.</summary>
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Move toward player
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );

        // Flip sprite to face player
        if (direction.x > 0) sr.flipX = false;
        if (direction.x < 0) sr.flipX = true;
    }

    /// <summary>Increases enemy speed — called by GameManager every difficulty interval.</summary>
    public void IncreaseSpeed(float amount)
    {
        chaseSpeed += amount;
    }

    /// <summary>Stops the enemy (called on Game Over).</summary>
    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>Damages the player on contact.</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.TakeDamage();
        }
    }
}
