using System.Collections;
using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.UI
{
    /// <summary>
    /// Controlador auto-contenido para el menú principal.
    /// Se construye su propia UI por código (no requiere setup manual en el Editor).
    /// Genera dos paneles: Principal (Jugar, Opciones, Salir) y Opciones (delegado a OptionsController).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ── Configuración visual ─────────────────────────────────────
        [Header("Colores del Menú")]
        public Color overlayColor = new Color(0f, 0f, 0f, 0.75f);
        public Color buttonColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        public Color buttonTextColor = Color.white;
        public Color titleColor = Color.white;

        [Header("Animación del Título")]
        public float titleAnimDuration = 0.8f;

        // ── Referencias internas ─────────────────────────────────────
        private GameObject _menuCanvas;
        private GameObject _panelMain;
        private GameObject _panelOptions;
        private GameObject _panelShop;
        private RectTransform _titleRT;
        private TextMeshProUGUI _coinDisplayText;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            // Inicializar el contador de FPS
            var fps = FPSCounter.Instance;
            
            BuildMainMenuUI();
        }

        void Start()
        {
            ShowPanel(_panelMain);
            AnimateTitle();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Navegación entre Paneles

        private void ShowPanel(GameObject target)
        {
            if (_panelMain != null) _panelMain.SetActive(_panelMain == target);
            if (_panelOptions != null) _panelOptions.SetActive(_panelOptions == target);
            if (_panelShop != null) _panelShop.SetActive(_panelShop == target);

            // Actualizar monedas al volver al panel principal
            if (target == _panelMain)
                UpdateCoinDisplay();
        }

        public void ShowMainPanel()     => ShowPanel(_panelMain);
        public void ShowOptionsPanel()  => ShowPanel(_panelOptions);
        public void ShowShopPanel()
        {
            // Actualizar el display de monedas de la tienda al entrar
            var shopCtrl = GetComponent<ShopController>();
            if (shopCtrl != null) shopCtrl.UpdateCoinsDisplay();
            ShowPanel(_panelShop);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Acciones de Botones

        public void OnPlayButton()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuController] No se encontró el GameManager en la escena.");
                return;
            }
            GameManager.Instance.LoadLevel(0);
        }

        public void OnOptionsButton() => ShowOptionsPanel();
        public void OnShopButton() => ShowShopPanel();

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
            if (_titleRT == null) return;
            StartCoroutine(AnimateTitleRoutine());
        }

        private IEnumerator AnimateTitleRoutine()
        {
            Vector2 startPos = _titleRT.anchoredPosition + Vector2.up * 80f;
            Vector2 endPos   = _titleRT.anchoredPosition;
            float elapsed = 0f;

            _titleRT.anchoredPosition = startPos;

            while (elapsed < titleAnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / titleAnimDuration;
                t = 1f - Mathf.Pow(1f - t, 3f);
                _titleRT.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            _titleRT.anchoredPosition = endPos;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Construcción automática de la UI

        private void BuildMainMenuUI()
        {
            _menuCanvas = new GameObject("MainMenuCanvas");
            _menuCanvas.transform.SetParent(this.transform);

            Canvas canvas = _menuCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 800;

            CanvasScaler scaler = _menuCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _menuCanvas.AddComponent<GraphicRaycaster>();

            BuildMainPanel();
            BuildOptionsPanel();
            BuildShopPanel();
        }

        private void BuildMainPanel()
        {
            _panelMain = CreatePanel(_menuCanvas.transform, "PanelMain", overlayColor);

            GameObject container = CreatePanel(_panelMain.transform, "Container", Color.clear);
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

            // Wrapper para el título (el LayoutGroup controla el wrapper, no el texto)
            // Esto permite animar el título sin conflicto con el LayoutGroup
            GameObject titleWrapper = new GameObject("TitleWrapper");
            titleWrapper.transform.SetParent(container.transform, false);
            RectTransform wrapperRT = titleWrapper.AddComponent<RectTransform>();
            wrapperRT.sizeDelta = new Vector2(0, 80f);

            TextMeshProUGUI titleTMP = CreateText(titleWrapper.transform, "TitleText",
                "ENDLESS RUNNER", 48, titleColor, FontStyles.Bold, 80f);
            // Estirar el título para que llene el wrapper
            RectTransform titleRT = titleTMP.rectTransform;
            titleRT.anchorMin = Vector2.zero;
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            titleRT.sizeDelta = Vector2.zero;
            _titleRT = titleRT;

            // ── Display de monedas debajo del título ───────────────────
            _coinDisplayText = CreateText(container.transform, "CoinText", "MONEDAS: 0",
                24, new Color(1f, 0.85f, 0.2f, 1f), FontStyles.Bold, 30f);
            UpdateCoinDisplay();

            CreateSeparator(container.transform);
            CreateButton(container.transform, "BtnPlay", "JUGAR", OnPlayButton);
            CreateButton(container.transform, "BtnShop", "TIENDA", OnShopButton);
            CreateButton(container.transform, "BtnOptions", "OPCIONES", OnOptionsButton);
            CreateButton(container.transform, "BtnQuit", "SALIR", OnQuitButton);
        }

        private void BuildOptionsPanel()
        {
            _panelOptions = CreatePanel(_menuCanvas.transform, "PanelOptions", overlayColor);

            GameObject container = CreatePanel(_panelOptions.transform, "Container", Color.clear);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.3f, 0.2f);
            containerRT.anchorMax = new Vector2(0.7f, 0.8f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            Image containerBG = container.GetComponent<Image>();
            containerBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            OptionsController optionsCtrl = GetComponent<OptionsController>();
            if (optionsCtrl == null)
                optionsCtrl = gameObject.AddComponent<OptionsController>();

            optionsCtrl.BuildOptionsUI(container.transform, buttonColor, buttonTextColor,
                titleColor, () => ShowMainPanel());

            _panelOptions.SetActive(false);
        }

        private void BuildShopPanel()
        {
            _panelShop = CreatePanel(_menuCanvas.transform, "PanelShop", overlayColor);

            GameObject container = CreatePanel(_panelShop.transform, "Container", Color.clear);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.3f, 0.15f);
            containerRT.anchorMax = new Vector2(0.7f, 0.85f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            Image containerBG = container.GetComponent<Image>();
            containerBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            ShopController shopCtrl = GetComponent<ShopController>();
            if (shopCtrl == null)
                shopCtrl = gameObject.AddComponent<ShopController>();

            shopCtrl.BuildShopUI(container.transform, buttonColor, buttonTextColor,
                titleColor, () => ShowMainPanel());

            _panelShop.SetActive(false);
        }

        /// <summary>Actualiza el texto de monedas en el menú principal.</summary>
        private void UpdateCoinDisplay()
        {
            if (_coinDisplayText != null && Core.CurrencyManager.Instance != null)
            {
                _coinDisplayText.text = $"MONEDAS: {Core.CurrencyManager.Instance.TotalCoins}";
            }
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
            // Estirar el texto para que llene el botón padre (no es hijo directo del LayoutGroup)
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
