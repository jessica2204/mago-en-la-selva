using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemy monkeys at random positions around the edges of the map.
/// Spawning frequency increases as the game progresses.
/// Attach to an empty GameObject called "EnemySpawner".
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float initialSpawnInterval = 5f;   // seconds between spawns
    [SerializeField] private float minimumSpawnInterval  = 1.5f; // fastest possible spawn rate
    [SerializeField] private float intervalReduction     = 0.3f; // how much faster each wave

    [Header("Spawn Area (match your map size)")]
    [SerializeField] private float spawnRangeX = 9f;
    [SerializeField] private float spawnRangeY = 5f;
    [SerializeField] private float edgeOffset  = 0.5f; // how close to the wall enemies spawn

    private float currentInterval;
    private bool isSpawning = true;

    // ── Keep track of all spawned enemies so we can speed them up ────────
    private System.Collections.Generic.List<EnemyAI> spawnedEnemies
        = new System.Collections.Generic.List<EnemyAI>();

    // ─────────────────────────────────────────────────────────────────────
    private void Start()
    {
        currentInterval = initialSpawnInterval;
        StartCoroutine(SpawnLoop());
    }

    /// <summary>Continuously spawns enemies at decreasing intervals.</summary>
    private IEnumerator SpawnLoop()
    {
        // Small delay before first spawn so the player can start moving
        yield return new WaitForSeconds(2f);

        while (isSpawning)
        {
            SpawnEnemy();

            // Speed up every enemy already in the scene
            IncreaseAllEnemySpeeds(0.15f);

            yield return new WaitForSeconds(currentInterval);

            // Reduce next interval (clamp to minimum)
            currentInterval = Mathf.Max(currentInterval - intervalReduction, minimumSpawnInterval);
        }
    }

    /// <summary>Instantiates one enemy at a random edge position.</summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector2 spawnPos = GetRandomEdgePosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
            spawnedEnemies.Add(ai);
    }

    /// <summary>Returns a random position near the edge of the play area.</summary>
    private Vector2 GetRandomEdgePosition()
    {
        // Pick one of the 4 edges randomly
        int edge = Random.Range(0, 4);
        float x, y;

        switch (edge)
        {
            case 0: // Top edge
                x = Random.Range(-spawnRangeX + edgeOffset, spawnRangeX - edgeOffset);
                y = spawnRangeY - edgeOffset;
                break;
            case 1: // Bottom edge
                x = Random.Range(-spawnRangeX + edgeOffset, spawnRangeX - edgeOffset);
                y = -spawnRangeY + edgeOffset;
                break;
            case 2: // Left edge
                x = -spawnRangeX + edgeOffset;
                y = Random.Range(-spawnRangeY + edgeOffset, spawnRangeY - edgeOffset);
                break;
            default: // Right edge
                x = spawnRangeX - edgeOffset;
                y = Random.Range(-spawnRangeY + edgeOffset, spawnRangeY - edgeOffset);
                break;
        }

        return new Vector2(x, y);
    }

    /// <summary>Increases speed of all spawned enemies.</summary>
    private void IncreaseAllEnemySpeeds(float amount)
    {
        // Remove null entries (destroyed enemies)
        spawnedEnemies.RemoveAll(e => e == null);

        foreach (EnemyAI enemy in spawnedEnemies)
            enemy.IncreaseSpeed(amount);
    }

    /// <summary>Stops spawning on Game Over.</summary>
    public void StopSpawning()
    {
        isSpawning = false;

        // Stop all existing enemies too
        spawnedEnemies.RemoveAll(e => e == null);
        foreach (EnemyAI enemy in spawnedEnemies)
            enemy.SetActive(false);
    }
}
