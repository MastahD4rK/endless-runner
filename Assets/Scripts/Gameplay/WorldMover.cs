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
        private GameSpeedManager _speedManager;

        void Start()
        {
            // Caché de la referencia al singleton para evitar acceder al Instance cada frame
            _speedManager = GameSpeedManager.Instance;
        }

        void FixedUpdate()
        {
            if (_speedManager == null)
            {
                _speedManager = GameSpeedManager.Instance;
                if (_speedManager == null) return;
            }

            if (_speedManager.isPlaying)
            {
                transform.position += Vector3.left * _speedManager.CurrentSpeed * Time.fixedDeltaTime;
            }
        }
    }
}
