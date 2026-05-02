using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Adjuntar este script al suelo, enemigos o decoración 
    /// para que se desplacen constantemente hacia la vista del jugador (izquierda).
    /// Usa FixedUpdate para sincronizar con el motor de físicas y evitar jitter.
    /// </summary>
    public class WorldMover : MonoBehaviour
    {
        void OnEnable()
        {
            if (WorldMoverManager.Instance != null)
            {
                WorldMoverManager.Instance.Register(this);
            }
        }

        void OnDisable()
        {
            if (WorldMoverManager.HasInstance)
            {
                WorldMoverManager.Instance.Unregister(this);
            }
        }
    }
}
