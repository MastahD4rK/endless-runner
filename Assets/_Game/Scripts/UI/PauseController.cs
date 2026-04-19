using UnityEngine;
using UnityEngine.InputSystem;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Controlador modular para el sistema de Pausa.
    /// Funciona escuchando la tecla [ESC] a traves del nuevo Input System.
    /// Congela el Time.timeScale y los AudioListeners dinámicamente.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        [Header("Interfaz de Usuario")]
        [Tooltip("Arrastra aqui el Panel de Pausa (Canvas > Panel)")]
        public GameObject pausePanel;

        /// <summary>Indica si el juego se encuentra en pausa actualmente.</summary>
        public bool IsPaused { get; private set; }

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            Instance = this;
            
            // Nos aseguramos de que el panel arranque apagado
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        void Update()
        {
            // 1. Evitar pausar si el jugador ya murió y está viendo el Game Over
            if (GameOverController.Instance != null && 
                GameOverController.Instance.gameOverPanel != null && 
                GameOverController.Instance.gameOverPanel.activeSelf)
            {
                return;
            }

            // 2. Detectar tecla [ESC] de forma nativa vía Unity Input System
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            
            // Garantía de seguridad: Si la escena cambia mientras está pausado, restaurar
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Lógica Principal

        /// <summary>
        /// Alterna el estado de pausa del juego. Congela o reanuda físicas y sonido.
        /// </summary>
        public void TogglePause()
        {
            IsPaused = !IsPaused;
            
            if (pausePanel != null)
                pausePanel.SetActive(IsPaused);

            if (IsPaused)
            {
                Time.timeScale = 0f;          // Congela mecánicas/físicas/movimiento
                AudioListener.pause = true;   // Pausa todos los Unity AudioSources
            }
            else
            {
                Time.timeScale = 1f;          // Restaura físicas
                AudioListener.pause = false;  // Restaura sonidos
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Eventos de Botones UI (Para conectar en el Canvas)

        /// <summary>Continuar la partida: Cierra el menú y reanuda el juego.</summary>
        public void OnResumeButton()
        {
            if (IsPaused) TogglePause();
        }

        /// <summary>Reintentar nivel: Restaura el engine y recarga la escena activa.</summary>
        public void OnRetryButton()
        {
            RestoreEngineState();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReloadCurrentLevel();
            }
        }

        /// <summary>Salir al inicio: Restaura el engine y vuelve al Main Menu.</summary>
        public void OnMainMenuButton()
        {
            RestoreEngineState();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMainMenu();
            }
        }

        /// <summary>Helpers para asegurar que el tiempo/audio no sigan trabados en nuevas escenas</summary>
        private void RestoreEngineState()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        #endregion
    }
}
