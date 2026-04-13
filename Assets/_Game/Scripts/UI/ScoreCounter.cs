using Platformer.Core;
using Platformer.Gameplay;
using TMPro;
using UnityEngine;

namespace Platformer.UI
{
    /// <summary>
    /// Contador de puntaje en tiempo real estilo Chrome Dino.
    /// El puntaje sube automáticamente mientras el jugador esté vivo.
    /// Cada 100 puntos hace un efecto visual de parpadeo.
    /// </summary>
    public class ScoreCounter : MonoBehaviour
    {
        public static ScoreCounter Instance { get; private set; }

        [Header("Referencia UI")]
        [Tooltip("Texto TMP donde se muestra el puntaje (ej: 00000)")]
        public TextMeshProUGUI scoreText;

        [Header("Configuración del Contador")]
        [Tooltip("Puntos que se suman por segundo base")]
        public float pointsPerSecond = 10f;

        [Tooltip("Si es true, la velocidad del juego afecta cuántos puntos ganas")]
        public bool scaleWithSpeed = true;

        [Header("Efecto cada 100 puntos")]
        [Tooltip("Duración del parpadeo cuando llegas a un múltiplo de 100")]
        public float blinkDuration = 0.5f;
        [Tooltip("Velocidad del parpadeo")]
        public float blinkSpeed = 15f;

        // ── Estado interno ──────────────────────────────────────────
        private float _scoreAccumulator = 0f;
        private int _displayedScore = 0;
        private int _lastMilestone = 0;
        private float _blinkTimer = 0f;
        private bool _isRunning = true;

        // ─────────────────────────────────────────────────────────────
        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            _scoreAccumulator = 0f;
            _displayedScore = 0;
            _lastMilestone = 0;
            _isRunning = true;
            UpdateText();
        }

        void Update()
        {
            if (!_isRunning) return;

            // Verificar si el juego sigue corriendo
            if (GameSpeedManager.Instance != null && !GameSpeedManager.Instance.isPlaying)
            {
                _isRunning = false;
                // Enviar puntaje final al GameManager
                if (GameManager.Instance != null)
                    GameManager.Instance.AddScore(_displayedScore);
                return;
            }

            // ── Acumular puntos ──────────────────────────────────
            float multiplier = 1f;
            if (scaleWithSpeed && GameSpeedManager.Instance != null)
                multiplier = GameSpeedManager.Instance.speedMultiplier;

            _scoreAccumulator += pointsPerSecond * multiplier * Time.deltaTime;

            int newScore = Mathf.FloorToInt(_scoreAccumulator);
            if (newScore != _displayedScore)
            {
                _displayedScore = newScore;
                UpdateText();

                // Verificar milestone cada 100 puntos
                int currentMilestone = _displayedScore / 100;
                if (currentMilestone > _lastMilestone)
                {
                    _lastMilestone = currentMilestone;
                    _blinkTimer = blinkDuration;
                }
            }

            // ── Efecto de parpadeo ───────────────────────────────
            if (_blinkTimer > 0f)
            {
                _blinkTimer -= Time.deltaTime;
                float alpha = Mathf.Abs(Mathf.Sin(_blinkTimer * blinkSpeed));
                if (scoreText != null)
                    scoreText.alpha = alpha;
            }
            else if (scoreText != null && scoreText.alpha < 1f)
            {
                scoreText.alpha = 1f;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─────────────────────────────────────────────────────────────
        private void UpdateText()
        {
            if (scoreText != null)
                scoreText.text = _displayedScore.ToString("D5"); // Formato 00000
        }

        /// <summary>Devuelve el puntaje actual de esta ronda.</summary>
        public int GetCurrentScore() => _displayedScore;
    }
}
