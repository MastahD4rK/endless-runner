using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Componente de obstáculo simple que se mueve con el WorldMover
    /// y mata al jugador al entrar en contacto.
    /// Similar al DeathZone pero sin necesitar ser Trigger.
    /// Se usa para pinchos, muros, o enemigos estáticos en el Endless Runner.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Obstacle : MonoBehaviour
    {
        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                Schedule<PlayerDeath>();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                Schedule<PlayerDeath>();
            }
        }
    }
}
