using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Administra la velocidad global a la que se mueve el mundo (hacia la izquierda).
    /// </summary>
    public class GameSpeedManager : MonoBehaviour
    {
        public static GameSpeedManager Instance { get; private set; }

        [Header("Configuración de Velocidad")]
        public float baseSpeed = 5f;
        public float maxSpeedMultiplier = 3f;
        public float accelerationRate = 0.05f;

        public float speedMultiplier { get; private set; } = 1f;
        public bool isPlaying = true;
        public bool isBossPhase = false;

        public float CurrentSpeed => baseSpeed * speedMultiplier;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (isPlaying && speedMultiplier < maxSpeedMultiplier)
            {
                speedMultiplier = Mathf.Min(
                    speedMultiplier + accelerationRate * Time.deltaTime,
                    maxSpeedMultiplier
                );
            }
        }

        public void ResetSpeed()
        {
            speedMultiplier = 1f;
            isPlaying = true;
            isBossPhase = false;
        }

        public void StopWorld()
        {
            isPlaying = false;
        }
    }
}
