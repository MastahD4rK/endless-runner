using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Componente de obstáculo simple que se mueve con el WorldMover
    /// y mata al jugador al entrar en contacto.
    /// Usa CompareTag en vez de GetComponent para evitar alocaciones GC en colisiones.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Obstacle : MonoBehaviour
    {
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Schedule<PlayerDeath>();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Schedule<PlayerDeath>();
            }
        }
    }
}
