using Platformer.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Core
{
    /// <summary>
    /// Singleton persistente entre escenas. Gestiona la progresión de niveles,
    /// el puntaje de sesión y la navegación entre escenas.
    /// Vive en la escena MainMenu y sobrevive con DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Configuración (asignar en el Inspector) ───────────────────
        [Header("Niveles del Juego (en orden de progresión)")]
        [Tooltip("Arrastrar aquí los LevelData ScriptableObjects en orden")]
        public LevelData[] levels;

        [Header("Escenas")]
        [Tooltip("Nombre exacto de la escena del menú principal")]
        public string mainMenuSceneName = "MainMenu";

        // ── Estado de la Sesión ───────────────────────────────────────
        /// <summary>Índice del nivel actualmente cargado o en juego.</summary>
        public int CurrentLevelIndex { get; private set; } = 0;

        /// <summary>Puntaje acumulado durante la sesión actual.</summary>
        public int TotalScore { get; private set; } = 0;

        // ── Progreso Guardado ─────────────────────────────────────────
        private const string UNLOCKED_KEY = "UnlockedLevelsCount";
        private const string HIGHSCORE_KEY = "HighScore";
        private int _unlockedCount = 1; // mínimo el primer nivel siempre desbloqueado

        /// <summary>Mejor puntaje registrado entre todas las sesiones.</summary>
        public int HighScore { get; private set; } = 0;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Navegación de Escenas

        /// <summary>
        /// Carga el nivel en el índice dado con transición de fade.
        /// </summary>
        public void LoadLevel(int index)
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogError("[GameManager] No hay niveles configurados en el Inspector.");
                return;
            }
            if (index < 0 || index >= levels.Length)
            {
                Debug.LogWarning($"[GameManager] Índice de nivel fuera de rango: {index}");
                return;
            }

            CurrentLevelIndex = index;
            SceneTransitionController.Instance?.FadeToScene(levels[index].sceneName);
        }

        /// <summary>
        /// Carga el siguiente nivel en la secuencia.
        /// Si no hay más, vuelve al menú principal.
        /// </summary>
        public void LoadNextLevel()
        {
            int next = CurrentLevelIndex + 1;
            if (levels != null && next < levels.Length)
            {
                UnlockLevel(next);
                LoadLevel(next);
            }
            else
            {
                Debug.Log("[GameManager] Fin de niveles — volviendo al menú.");
                GoToMainMenu();
            }
        }

        /// <summary>
        /// Recarga el nivel actual (útil desde la pantalla de Game Over).
        /// </summary>
        public void ReloadCurrentLevel()
        {
            if (levels != null && levels.Length > 0 && CurrentLevelIndex < levels.Length)
            {
                SceneTransitionController.Instance?.FadeToScene(levels[CurrentLevelIndex].sceneName);
            }
            else
            {
                // Fallback: recargar la escena activa directamente
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>
        /// Vuelve a la escena del menú principal.
        /// </summary>
        public void GoToMainMenu()
        {
            ResetSessionScore();
            SceneTransitionController.Instance?.FadeToScene(mainMenuSceneName);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Puntaje

        /// <summary>Agrega puntos al puntaje de la sesión actual.</summary>
        public void AddScore(int points) => TotalScore += points;

        /// <summary>Reinicia el puntaje de sesión a 0.</summary>
        public void ResetSessionScore() => TotalScore = 0;

        /// <summary>
        /// Intenta establecer un nuevo high score. Retorna true si fue superado.
        /// </summary>
        public bool TrySetHighScore(int score)
        {
            if (score > HighScore)
            {
                HighScore = score;
                SaveProgress();
                Debug.Log($"[GameManager] ¡Nuevo High Score! {HighScore}");
                return true;
            }
            return false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Progresión y Desbloqueo

        /// <summary>Retorna true si el nivel en el índice dado está desbloqueado.</summary>
        public bool IsLevelUnlocked(int index) => index < _unlockedCount;

        /// <summary>
        /// Desbloquea todos los niveles hasta el índice indicado y guarda el progreso.
        /// </summary>
        public void UnlockLevel(int index)
        {
            if (index + 1 > _unlockedCount)
            {
                _unlockedCount = index + 1;
                SaveProgress();
                Debug.Log($"[GameManager] Nivel {index} desbloqueado.");
            }
        }

        /// <summary>
        /// Desbloquea TODOS los niveles (útil para debug o cheat).
        /// </summary>
        [ContextMenu("Desbloquear todos los niveles")]
        public void UnlockAllLevels()
        {
            if (levels != null)
                _unlockedCount = levels.Length;
            SaveProgress();
        }

        /// <summary>Borra todo el progreso guardado (para debug).</summary>
        [ContextMenu("Borrar progreso guardado")]
        public void ResetProgress()
        {
            _unlockedCount = 1;
            SaveProgress();
            Debug.Log("[GameManager] Progreso borrado.");
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt(UNLOCKED_KEY, _unlockedCount);
            PlayerPrefs.SetInt(HIGHSCORE_KEY, HighScore);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _unlockedCount = PlayerPrefs.GetInt(UNLOCKED_KEY, 1);
            HighScore = PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
            Debug.Log($"[GameManager] Progreso cargado — Niveles desbloqueados: {_unlockedCount}, High Score: {HighScore}");
        }

        #endregion
    }
}
