using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Controlador auto-contenido para el sistema de Pausa.
    /// Se construye su propia UI por código (no requiere setup manual en el Editor).
    /// Escucha [ESC] vía Input System y congela Time.timeScale + AudioListener.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        /// <summary>Indica si el juego se encuentra en pausa actualmente.</summary>
        public bool IsPaused { get; private set; }

        // ── Configuración visual (modificable desde Inspector) ────────
        [Header("Colores del Menú")]
        [Tooltip("Color del fondo semi-transparente")]
        public Color overlayColor = new Color(0f, 0f, 0f, 0.75f);

        [Tooltip("Color normal de los botones")]
        public Color buttonColor = new Color(0.2f, 0.2f, 0.25f, 1f);

        [Tooltip("Color del texto de los botones")]
        public Color buttonTextColor = Color.white;

        [Tooltip("Color del título 'PAUSA'")]
        public Color titleColor = Color.white;

        // ── Referencias internas (construidas por código) ─────────────
        private GameObject _pauseCanvas;
        private GameObject _pausePanel;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            Instance = this;
            BuildPauseUI();
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

            // 2. Detectar tecla [ESC] vía Unity Input System
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Garantía de seguridad: restaurar si la escena cambia estando pausado
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

            if (_pausePanel != null)
                _pausePanel.SetActive(IsPaused);

            if (IsPaused)
            {
                Time.timeScale = 0f;
                AudioListener.pause = true;
            }
            else
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Eventos de Botones

        public void OnResumeButton()
        {
            if (IsPaused) TogglePause();
        }

        public void OnRetryButton()
        {
            HidePauseUI();
            RestoreEngineState();
            if (GameManager.Instance != null)
                GameManager.Instance.ReloadCurrentLevel();
        }

        public void OnMainMenuButton()
        {
            HidePauseUI();
            RestoreEngineState();
            if (GameManager.Instance != null)
                GameManager.Instance.GoToMainMenu();
        }

        private void RestoreEngineState()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        /// <summary>Oculta el panel de pausa antes de cambiar de escena.</summary>
        private void HidePauseUI()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Construcción automática de la UI

        private void BuildPauseUI()
        {
            // ── Canvas de overlay ────────────────────────────────────
            _pauseCanvas = new GameObject("PauseCanvas");
            _pauseCanvas.transform.SetParent(this.transform);

            Canvas canvas = _pauseCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900; // Por debajo del fade (999) pero por encima de todo lo demás

            CanvasScaler scaler = _pauseCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _pauseCanvas.AddComponent<GraphicRaycaster>();

            // ── Panel de fondo oscuro (cubre toda la pantalla) ───────
            _pausePanel = CreatePanel(_pauseCanvas.transform, "PausePanel", overlayColor);
            _pausePanel.SetActive(false); // Empieza oculto

            // ── Contenedor centrado para los elementos ───────────────
            GameObject container = CreatePanel(_pausePanel.transform, "Container", Color.clear);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.3f, 0.2f);
            containerRT.anchorMax = new Vector2(0.7f, 0.8f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            // Fondo del contenedor (panel con bordes redondeados simulados)
            Image containerBG = container.GetComponent<Image>();
            containerBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // ── Layout Vertical ──────────────────────────────────────
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // ── Título "PAUSA" ───────────────────────────────────────
            CreateText(container.transform, "TitleText", "PAUSA", 48, titleColor, FontStyles.Bold, 80f);

            // ── Separador ────────────────────────────────────────────
            CreateSeparator(container.transform);

            // ── Botón: Continuar ─────────────────────────────────────
            CreateButton(container.transform, "BtnResume", "CONTINUAR", OnResumeButton);

            // ── Botón: Reintentar ────────────────────────────────────
            CreateButton(container.transform, "BtnRetry", "REINTENTAR", OnRetryButton);

            // ── Botón: Menú Principal ────────────────────────────────
            CreateButton(container.transform, "BtnMainMenu", "MENU PRINCIPAL", OnMainMenuButton);
        }

        // ── Helpers de construcción ──────────────────────────────────

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string content, 
            float fontSize, Color color, FontStyles style, float height)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, height);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            tmp.raycastTarget = false; // CLAVE: dejar que los clicks pasen al Button de abajo

            // Intentar usar la fuente por defecto de TMP
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;

            return tmp;
        }

        private void CreateButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60f);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = buttonColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            // Colores de hover/click
            ColorBlock colors = btn.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = new Color(buttonColor.r + 0.15f, buttonColor.g + 0.15f, buttonColor.b + 0.2f, 1f);
            colors.pressedColor = new Color(buttonColor.r + 0.25f, buttonColor.g + 0.25f, buttonColor.b + 0.35f, 1f);
            colors.selectedColor = colors.highlightedColor;
            btn.colors = colors;

            btn.onClick.AddListener(onClick);

            // Texto del botón
            CreateText(btnObj.transform, "Label", label, 24, buttonTextColor, FontStyles.Normal, 60f);
        }

        private void CreateSeparator(Transform parent)
        {
            GameObject sep = new GameObject("Separator");
            sep.transform.SetParent(parent, false);

            RectTransform rt = sep.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 2f);

            Image img = sep.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.2f);
        }

        #endregion
    }
}
