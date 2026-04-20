using Platformer.Gameplay;
using UnityEngine;

namespace Platformer.View
{
    /// <summary>
    /// Implementa efecto Parallax para fondos en un Endless Runner con cámara estática.
    /// En vez de seguir la posición de la cámara (que ahora está fija),
    /// se desplaza usando la velocidad global del mundo con un factor de escala
    /// para crear la ilusión de profundidad.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        /// <summary>
        /// Factor de velocidad del parallax relativo a la velocidad del mundo.
        /// Valores más bajos = más lejos (se mueve más lento).
        /// Ejemplo: 0.2 para montañas lejanas, 0.5 para edificios cercanos.
        /// </summary>
        [Range(0f, 1f)]
        public float parallaxFactor = 0.5f;

        private GameSpeedManager _speedManager;

        void Start()
        {
            _speedManager = GameSpeedManager.Instance;
        }

        void Update()
        {
            if (_speedManager == null)
            {
                _speedManager = GameSpeedManager.Instance;
                if (_speedManager == null) return;
            }

            if (_speedManager.isPlaying)
            {
                // Desplazar el fondo hacia la izquierda proporcionalmente a la velocidad del mundo
                float speed = _speedManager.CurrentSpeed * parallaxFactor;
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
        }
    }
}