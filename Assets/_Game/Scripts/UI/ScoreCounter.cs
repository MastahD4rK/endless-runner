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

        [Tooltip("Texto TMP donde se muestra el puntaje (ej: 00000)")]
        public TextMeshProUGUI scoreText;

        [Tooltip("Texto TMP flotante para mostrar los bonus (ej: +50). Se anima automáticamente.")]
        public TextMeshProUGUI bonusText;

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

        // ── Eventos ─────────────────────────────────────────────────
        /// <summary>
        /// Se invoca cada vez que el puntaje mostrado cambia.
        /// Paso el score actual como parámetro.
        /// Usado por MapManager para saber cuándo cambiar de mapa.
        /// </summary>
        public System.Action<int> OnScoreChanged;

        // ── Estado interno ──────────────────────────────────────────
        private float _scoreAccumulator = 0f;
        private int _displayedScore = 0;
        private int _lastMilestone = 0;
        private float _blinkTimer = 0f;
        private bool _isRunning = true;
        private GameSpeedManager _speedManager;

        // ── Estado del Bonus Popup ───────────────────────────────────
        private float _bonusTimer = 0f;
        private float _bonusDuration = 1f;
        private Vector3 _bonusOriginalPos;

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
            _speedManager = GameSpeedManager.Instance;
            
            // Auto-generar el texto flotante si no fue asignado en el Inspector
            if (bonusText == null && scoreText != null)
            {
                CreateBonusTextAutomatically();
            }

            if (bonusText != null)
            {
                _bonusOriginalPos = bonusText.rectTransform.anchoredPosition;
                SetBonusAlpha(0f);
            }

            UpdateText();
        }

        void Update()
        {
            if (!_isRunning) return;

            // Lazy init por si el singleton se creó después
            if (_speedManager == null)
            {
                _speedManager = GameSpeedManager.Instance;
                if (_speedManager == null) return;
            }

            // Verificar si el juego sigue corriendo
            if (!_speedManager.isPlaying)
            {
                _isRunning = false;
                // Enviar puntaje final al GameManager
                if (GameManager.Instance != null)
                    GameManager.Instance.AddScore(_displayedScore);
                return;
            }

            // ── Acumular puntos ──────────────────────────────────
            float multiplier = 1f;
            if (scaleWithSpeed)
                multiplier = _speedManager.speedMultiplier;

            _scoreAccumulator += pointsPerSecond * multiplier * Time.deltaTime;

            int newScore = Mathf.FloorToInt(_scoreAccumulator);
            if (newScore != _displayedScore)
            {
                _displayedScore = newScore;
                UpdateText();

                // Notificar a los suscriptores (MapManager, etc.)
                OnScoreChanged?.Invoke(_displayedScore);

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

            // ── Efecto del Bonus Text (+50) ──────────────────────────
            if (_bonusTimer > 0f && bonusText != null)
            {
                _bonusTimer -= Time.deltaTime;
                float t = 1f - (_bonusTimer / _bonusDuration); // 0 a 1

                // Animación: flota hacia arriba 50 pixeles
                bonusText.rectTransform.anchoredPosition = _bonusOriginalPos + new Vector3(0f, Mathf.Lerp(0, 50f, t), 0f);
                
                // Fade out en la segunda mitad de la animación
                float alpha = t > 0.5f ? Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f) : 1f;
                SetBonusAlpha(alpha);
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

        private void SetBonusAlpha(float alpha)
        {
            if (bonusText != null)
            {
                Color c = bonusText.color;
                c.a = alpha;
                bonusText.color = c;
            }
        }

        private void CreateBonusTextAutomatically()
        {
            // Duplicar el texto de score existente
            GameObject bonusObj = Instantiate(scoreText.gameObject, scoreText.transform.parent);
            bonusObj.name = "BonusText_Auto";
            bonusText = bonusObj.GetComponent<TextMeshProUGUI>();
            
            // Ajustar el diseño del texto duplicado
            bonusText.text = "+50";
            bonusText.color = new Color(1f, 0.8f, 0f, 1f); // Dorado
            bonusText.fontSize = scoreText.fontSize * 0.7f; // Un poco más pequeño
            bonusText.alignment = TextAlignmentOptions.TopRight;
            
            // Moverlo ligeramente abajo y a la derecha del texto principal
            bonusText.rectTransform.anchoredPosition = scoreText.rectTransform.anchoredPosition + new Vector2(0, -30f);
            
            // Limpiar si tiene algún otro script copiado accidentalmente
            // (no debría, ya que clonamos el objeto del texto que normalmente solo tiene el TMP)
        }

        /// <summary>Suma puntos extra al puntaje actual y muestra el popup visual.</summary>
        public void AddBonusScore(int amount)
        {
            if (!_isRunning) return;

            _scoreAccumulator += amount;
            
            // Forzar actualización visual inmediata
            _displayedScore = Mathf.FloorToInt(_scoreAccumulator);
            UpdateText();

            // Activar efecto de UI popup
            if (bonusText != null)
            {
                bonusText.text = $"+{amount}";
                bonusText.rectTransform.anchoredPosition = _bonusOriginalPos;
                SetBonusAlpha(1f);
                _bonusTimer = _bonusDuration;
            }
        }

        /// <summary>Devuelve el puntaje actual de esta ronda.</summary>
        public int GetCurrentScore() => _displayedScore;
    }
}
