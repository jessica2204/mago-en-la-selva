using UnityEngine;

// Adjunta este script al GameObject del jugador (mago)
// También necesitas crear un Particle System — instrucciones abajo
public class DamageEffect : MonoBehaviour
{
    [Header("Partículas de daño")]
    public ParticleSystem damageParticles; // Arrastra aquí tu Particle System

    // Llama este método desde PlayerController cuando el jugador recibe daño
    public void PlayDamageEffect()
    {
        if (damageParticles != null)
        {
            // Reproducir en la posición actual del jugador
            damageParticles.transform.position = transform.position;
            damageParticles.Play();
        }
    }
}
