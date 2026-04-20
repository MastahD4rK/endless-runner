using Platformer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.UI
{
    /// <summary>
    /// Controla la pantalla de Game Over en la escena de gameplay.
    /// Se activa automáticamente cuando el jugador muere (llamado desde PlayerSpawn).
    ///
    /// Cómo usar:
    /// 1. Agregar este script a un GameObject en la escena de juego.
    /// 2. Crear un Panel de Game Over en el UI Canvas y asignarlo a gameOverPanel.
    /// 3. Conectar los botones de Reintentar y Menú Principal.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        public static GameOverController Instance { get; private set; }

        [Header("Panel de Game Over")]
        [Tooltip("El panel de Game Over (inicialmente desactivado en la escena)")]
        public GameObject gameOverPanel;

        [Tooltip("Texto que muestra el puntaje final — opcional")]
        public TextMeshProUGUI finalScoreText;

        [Tooltip("Texto que muestra por cuánto tiempo sobrevivió el jugador — opcional")]
        public TextMeshProUGUI survivalTimeText;

        private float _levelStartTime;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            Instance = this;

            // Asegurar que el panel comienza oculto
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        void Start()
        {
            _levelStartTime = Time.time;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region API Pública

        /// <summary>
        /// Muestra la pantalla de Game Over con el puntaje y tiempo actuales.
        /// Llamado desde PlayerSpawn.cs cuando el jugador muere.
        /// </summary>
        public void ShowGameOver()
        {
            if (gameOverPanel == null)
            {
                Debug.LogWarning("[GameOverController] No hay un gameOverPanel asignado.");
                return;
            }

            // Mostrar puntaje del contador en vivo
            if (finalScoreText != null)
            {
                int score = ScoreCounter.Instance != null
                    ? ScoreCounter.Instance.GetCurrentScore()
                    : (GameManager.Instance != null ? GameManager.Instance.TotalScore : 0);
                finalScoreText.text = $"PUNTAJE: {score}";
            }

            // Mostrar tiempo de supervivencia
            if (survivalTimeText != null)
            {
                float survived = Time.time - _levelStartTime;
                int minutes = Mathf.FloorToInt(survived / 60f);
                int seconds = Mathf.FloorToInt(survived % 60f);
                survivalTimeText.text = $"TIEMPO: {minutes:00}:{seconds:00}";
            }

            // Pausar el mundo
            if (Gameplay.GameSpeedManager.Instance != null)
                Gameplay.GameSpeedManager.Instance.StopWorld();

            // ── FIX: Activar el Canvas padre ─────────────────────────
            // El MetaGameController desactiva el UI Canvas al inicio del juego.
            // Necesitamos reactivarlo para que el panel sea visible (activeInHierarchy = true).
            Transform parent = gameOverPanel.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }

            // Activar el panel de Game Over
            gameOverPanel.SetActive(true);

            // Desactivar otros paneles hermanos (PausePanel, etc.) para evitar conflictos
            Transform panelParent = gameOverPanel.transform.parent;
            if (panelParent != null)
            {
                foreach (Transform sibling in panelParent)
                {
                    if (sibling.gameObject != gameOverPanel && sibling.gameObject.activeSelf)
                    {
                        sibling.gameObject.SetActive(false);
                    }
                }
            }

            Debug.Log("[GameOverController] Panel de Game Over mostrado correctamente.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Botones del Panel

        /// <summary>Botón "REINTENTAR" — recarga el nivel actual.</summary>
        public void OnRetryButton()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReloadCurrentLevel();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>Botón "MENÚ PRINCIPAL" — vuelve al menú.</summary>
        public void OnMainMenuButton()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GoToMainMenu();
            else
                SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}
