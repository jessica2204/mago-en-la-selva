using UnityEngine;

/// <summary>
/// Collectible coin for the top-down game.
/// Rotates in place and awards points when the player touches it.
/// After collection, respawns at a new random position after a delay.
/// Attach to the Coin prefab. Set Collider2D as Trigger.
/// </summary>
public class CoinCollectible : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int    coinValue      = 1;
    [SerializeField] private float  rotationSpeed  = 90f;
    [SerializeField] private float  respawnDelay   = 3f;
    [SerializeField] private float  spawnRangeX    = 7f;
    [SerializeField] private float  spawnRangeY    = 4f;

    private SpriteRenderer sr;
    private Collider2D     col;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Spin the coin for visual feedback
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(coinValue);
            GameManager.Instance.PlayCoinSound();
            StartCoroutine(HideAndRespawn());
        }
    }

    /// <summary>Hides the coin then moves it to a new random spot.</summary>
    private System.Collections.IEnumerator HideAndRespawn()
    {
        // Hide immediately
        sr.enabled  = false;
        col.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // Move to a new random position and reappear
        float x = Random.Range(-spawnRangeX, spawnRangeX);
        float y = Random.Range(-spawnRangeY, spawnRangeY);
        transform.position = new Vector3(x, y, 0f);

        sr.enabled  = true;
        col.enabled = true;
    }
}
