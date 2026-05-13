using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 8f;          // Velocidad del proyectil
    public float lifetime = 3f;       // Segundos antes de destruirse solo
    public int damage = 1;            // Daño que hace al enemigo

    private Vector2 direction;
    private Rigidbody2D rb;

    // Llama este método justo después de Instantiate para darle dirección
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Si nadie llamó SetDirection, apuntar hacia arriba por defecto
        if (direction == Vector2.zero)
            direction = Vector2.up;

        // Destruirse después de "lifetime" segundos para no llenar la memoria
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // IMPORTANTE: usar MovePosition en Rigidbody2D es más confiable
        // que cambiar transform.position directamente
        if (rb != null)
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
        else
        {
            // Fallback si no hay Rigidbody2D
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // PROBLEMA 3 resuelto aquí: detecta el tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            // Intentar hacer daño al enemigo
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Destruir el proyectil al impactar
            Destroy(gameObject);
        }

        // También destruirse si choca con un obstáculo o pared
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
