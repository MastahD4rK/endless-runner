using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Adjuntar este script al suelo, enemigos o decoración 
    /// para que se desplacen constantemente hacia la vista del jugador (izquierda).
    /// </summary>
    public class WorldMover : MonoBehaviour
    {
        void Update()
        {
            if (GameSpeedManager.Instance != null && GameSpeedManager.Instance.isPlaying)
            {
                // Mover automáticamente todo lo que tiene este componente hacia la izquierda (eje X negativo)
                transform.position += Vector3.left * GameSpeedManager.Instance.CurrentSpeed * Time.deltaTime;
            }
        }
    }
}
