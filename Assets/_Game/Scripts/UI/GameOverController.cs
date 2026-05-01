using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Controlador auto-contenido para la pantalla de Game Over.
    /// Se construye su propia UI por código (no requiere setup manual en el Editor).
    /// Activado por PlayerSpawn cuando el jugador muere.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        public static GameOverController Instance { get; private set; }

        /// <summary>
        /// El panel raíz del Game Over. Público para que PauseController
        /// pueda verificar si está activo antes de permitir pausar.
        /// Se genera automáticamente por código en Awake().
        /// </summary>
        [HideInInspector]
        public GameObject gameOverPanel;

        // ── Configuración visual (modificable desde Inspector) ────────
        [Header("Colores del Menú")]
        public Color overlayColor = new Color(0f, 0f, 0f, 0.75f);
        public Color buttonColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        public Color buttonTextColor = Color.white;
        public Color titleColor = Color.white;

        // ── Referencias internas ─────────────────────────────────────
        private GameObject _gameOverCanvas;
        private TextMeshProUGUI _finalScoreText;
        private TextMeshProUGUI _survivalTimeText;
        private float _levelStartTime;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            Instance = this;
            BuildGameOverUI();
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
                Debug.LogWarning("[GameOverController] No hay un gameOverPanel.");
                return;
            }

            if (_finalScoreText != null)
            {
                int score = ScoreCounter.Instance != null
                    ? ScoreCounter.Instance.GetCurrentScore()
                    : (GameManager.Instance != null ? GameManager.Instance.TotalScore : 0);
                _finalScoreText.text = $"PUNTAJE: {score}";
            }

            if (_survivalTimeText != null)
            {
                float survived = Time.time - _levelStartTime;
                int minutes = Mathf.FloorToInt(survived / 60f);
                int seconds = Mathf.FloorToInt(survived % 60f);
                _survivalTimeText.text = $"TIEMPO: {minutes:00}:{seconds:00}";
            }

            if (Gameplay.GameSpeedManager.Instance != null)
                Gameplay.GameSpeedManager.Instance.StopWorld();

            gameOverPanel.SetActive(true);
            Debug.Log("[GameOverController] Panel de Game Over mostrado correctamente.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Botones del Panel

        public void OnRetryButton()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReloadCurrentLevel();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnMainMenuButton()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GoToMainMenu();
            else
                SceneManager.LoadScene("MainMenu");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Construcción automática de la UI

        private void BuildGameOverUI()
        {
            _gameOverCanvas = new GameObject("GameOverCanvas");
            _gameOverCanvas.transform.SetParent(this.transform);

            Canvas canvas = _gameOverCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            CanvasScaler scaler = _gameOverCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _gameOverCanvas.AddComponent<GraphicRaycaster>();

            gameOverPanel = CreatePanel(_gameOverCanvas.transform, "GameOverPanel", overlayColor);
            gameOverPanel.SetActive(false);

            GameObject container = CreatePanel(gameOverPanel.transform, "Container", Color.clear);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.3f, 0.2f);
            containerRT.anchorMax = new Vector2(0.7f, 0.8f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            Image containerBG = container.GetComponent<Image>();
            containerBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateText(container.transform, "TitleText", "GAME OVER", 48, titleColor, FontStyles.Bold, 80f);
            CreateSeparator(container.transform);
            _finalScoreText = CreateText(container.transform, "ScoreText", "PUNTAJE: 00000", 28, Color.white, FontStyles.Normal, 50f);
            _survivalTimeText = CreateText(container.transform, "TimeText", "TIEMPO: 00:00", 28, Color.white, FontStyles.Normal, 50f);
            CreateSeparator(container.transform);
            CreateButton(container.transform, "BtnRetry", "REINTENTAR", OnRetryButton);
            CreateButton(container.transform, "BtnMainMenu", "MENU PRINCIPAL", OnMainMenuButton);
        }

        // ── Helpers idénticos a PauseController ─────────────────────

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
            tmp.raycastTarget = false;

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

            ColorBlock colors = btn.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = new Color(buttonColor.r + 0.15f, buttonColor.g + 0.15f, buttonColor.b + 0.2f, 1f);
            colors.pressedColor = new Color(buttonColor.r + 0.25f, buttonColor.g + 0.25f, buttonColor.b + 0.35f, 1f);
            colors.selectedColor = colors.highlightedColor;
            btn.colors = colors;

            btn.onClick.AddListener(onClick);

            TextMeshProUGUI labelTMP = CreateText(btnObj.transform, "Label", label, 24, buttonTextColor, FontStyles.Normal, 60f);
            RectTransform labelRT = labelTMP.rectTransform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            labelRT.sizeDelta = Vector2.zero;
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
