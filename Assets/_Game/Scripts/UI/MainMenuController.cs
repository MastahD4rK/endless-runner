using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Controla el menú principal con dos paneles:
    ///   - Panel_Main: botones de Jugar, Opciones y Salir
    ///   - Panel_Options: sliders de volumen
    ///
    /// Asignar este script a un GameObject en la escena MainMenu.
    /// Conectar los paneles en el Inspector.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ── Paneles ───────────────────────────────────────────────────
        [Header("Paneles del Menú")]
        public GameObject panelMain;
        public GameObject panelOptions;

        // ── Animación del título ──────────────────────────────────────
        [Header("Título Animado (opcional)")]
        [Tooltip("RectTransform del logo o título para animar al inicio")]
        public RectTransform titleTransform;
        public float titleAnimDuration = 0.8f;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Start()
        {
            ShowPanel(panelMain);
            AnimateTitle();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Navegación entre Paneles

        private void ShowPanel(GameObject target)
        {
            panelMain.SetActive(panelMain == target);
            panelOptions.SetActive(panelOptions == target);
        }

        // Botones para el panel principal
        public void ShowMainPanel()     => ShowPanel(panelMain);
        public void ShowOptionsPanel()  => ShowPanel(panelOptions);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Acciones de Botones Principales

        /// <summary>
        /// Botón "JUGAR": carga directamente el primer nivel.
        /// </summary>
        public void OnPlayButton()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuController] No se encontró el GameManager en la escena.");
                return;
            }

            GameManager.Instance.LoadLevel(0);
        }

        /// <summary>Botón "OPCIONES".</summary>
        public void OnOptionsButton() => ShowOptionsPanel();

        /// <summary>Botón "SALIR" — funciona tanto en Editor como en build.</summary>
        public void OnQuitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Animación

        private void AnimateTitle()
        {
            if (titleTransform == null) return;
            // Animación simple de descenso desde arriba
            StartCoroutine(AnimateTitleRoutine());
        }

        private System.Collections.IEnumerator AnimateTitleRoutine()
        {
            Vector2 startPos = titleTransform.anchoredPosition + Vector2.up * 80f;
            Vector2 endPos   = titleTransform.anchoredPosition;
            float elapsed = 0f;

            titleTransform.anchoredPosition = startPos;

            while (elapsed < titleAnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / titleAnimDuration;
                // Ease-out cubic
                t = 1f - Mathf.Pow(1f - t, 3f);
                titleTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            titleTransform.anchoredPosition = endPos;
        }

        #endregion
    }
}
