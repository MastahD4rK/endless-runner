using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Controla el menú principal con tres paneles:
    ///   - Panel_Main: botones de Jugar, Opciones y Salir
    ///   - Panel_LevelSelect: grilla de niveles desbloqueados
    ///   - Panel_Options: sliders de volumen
    ///
    /// Asignar este script a un GameObject en la escena MainMenu.
    /// Conectar los tres paneles y el prefab de tarjeta en el Inspector.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ── Paneles ───────────────────────────────────────────────────
        [Header("Paneles del Menú")]
        public GameObject panelMain;
        public GameObject panelLevelSelect;
        public GameObject panelOptions;

        // ── Level Select ──────────────────────────────────────────────
        [Header("Selector de Niveles")]
        [Tooltip("Transform padre donde se instancian las tarjetas de nivel (debe tener un Layout Group)")]
        public Transform levelCardsContainer;

        [Tooltip("Prefab de tarjeta de nivel con el componente LevelCardController")]
        public GameObject levelCardPrefab;

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
            PopulateLevelSelect();
            AnimateTitle();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Navegación entre Paneles

        private void ShowPanel(GameObject target)
        {
            panelMain.SetActive(panelMain == target);
            panelLevelSelect.SetActive(panelLevelSelect == target);
            panelOptions.SetActive(panelOptions == target);
        }

        // Botones para el panel principal
        public void ShowMainPanel()     => ShowPanel(panelMain);
        public void ShowLevelSelect()   => ShowPanel(panelLevelSelect);
        public void ShowOptionsPanel()  => ShowPanel(panelOptions);

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Acciones de Botones Principales

        /// <summary>
        /// Botón "JUGAR":
        /// - Si hay más de un nivel, muestra el selector.
        /// - Si solo hay un nivel, lo carga directamente.
        /// </summary>
        public void OnPlayButton()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuController] No se encontró el GameManager en la escena.");
                return;
            }

            if (GameManager.Instance.levels != null && GameManager.Instance.levels.Length > 1)
                ShowLevelSelect();
            else
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
        #region Selector de Niveles

        /// <summary>
        /// Genera las tarjetas de nivel dinámicamente a partir de los
        /// datos registrados en el GameManager.
        /// </summary>
        private void PopulateLevelSelect()
        {
            if (GameManager.Instance == null) return;
            if (levelCardPrefab == null || levelCardsContainer == null)
            {
                Debug.LogWarning("[MainMenuController] Falta asignar levelCardPrefab o levelCardsContainer.");
                return;
            }

            // Limpiar tarjetas antiguas
            foreach (Transform child in levelCardsContainer)
                Destroy(child.gameObject);

            // Crear una tarjeta por cada nivel
            for (int i = 0; i < GameManager.Instance.levels.Length; i++)
            {
                int capturedIndex = i; // capturar para el closure del listener
                bool unlocked = GameManager.Instance.IsLevelUnlocked(i);

                GameObject card = Instantiate(levelCardPrefab, levelCardsContainer);
                LevelCardController cardCtrl = card.GetComponent<LevelCardController>();

                if (cardCtrl != null)
                {
                    cardCtrl.Setup(
                        GameManager.Instance.levels[i],
                        unlocked,
                        () => GameManager.Instance.LoadLevel(capturedIndex)
                    );
                }
            }
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
