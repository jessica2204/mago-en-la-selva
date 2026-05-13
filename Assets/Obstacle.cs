using UnityEngine;

/// <summary>
/// Static obstacle (tree, rock, bush) that blocks player movement.
/// Just needs a Collider2D (not trigger) — no logic required.
/// This script is optional; it exists only for documentation purposes.
/// Attach to any obstacle prefab in the scene.
/// </summary>
public class Obstacle : MonoBehaviour
{
    // This object is purely physical — the Collider2D blocks movement.
    // No additional logic needed beyond the collider component.
}
