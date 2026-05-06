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
        public Color buttonColor = new Color(0.15f, 0.1f, 0.25f, 1f);
        public Color buttonTextColor = Color.white;
        public Color titleColor = new Color(1f, 0.1f, 0.6f, 1f);

        [Header("Animación del Título")]
        public float titleAnimDuration = 0.8f;

        // ── Referencias internas ─────────────────────────────────────
        private GameObject _menuCanvas;
        private GameObject _panelMain;
        private GameObject _panelOptions;
        private GameObject _panelShop;
        private GameObject _panelSkills;
        private RectTransform _titleRT;
        private TextMeshProUGUI _coinDisplayText;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            // Forzar nuevos colores neon por si el Inspector tiene guardados los viejos
            overlayColor = new Color(0f, 0f, 0f, 0.75f);
            buttonColor = new Color(0.15f, 0.1f, 0.25f, 1f);
            buttonTextColor = Color.white;
            titleColor = new Color(1f, 0.1f, 0.6f, 1f);

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
            if (_panelSkills != null) _panelSkills.SetActive(_panelSkills == target);

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
        
        public void ShowSkillsPanel()
        {
            var skillsCtrl = GetComponent<SkillTreeController>();
            if (skillsCtrl != null) skillsCtrl.UpdateAllCards();
            ShowPanel(_panelSkills);
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
        public void OnSkillsButton() => ShowSkillsPanel();

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

        private IEnumerator PulseRoutine(Transform target)
        {
            Vector3 baseScale = Vector3.one;
            while (target != null)
            {
                float scale = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.05f;
                target.localScale = baseScale * scale;
                yield return null;
            }
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
            BuildSkillsPanel();
        }

        private void BuildMainPanel()
        {
            _panelMain = CreatePanel(_menuCanvas.transform, "PanelMain", overlayColor);

            // ── Zona del Título (Arriba Centro) ──────────────────────────
            GameObject titleContainer = CreatePanel(_panelMain.transform, "TitleContainer", Color.clear);
            RectTransform titleContainerRT = titleContainer.GetComponent<RectTransform>();
            titleContainerRT.anchorMin = new Vector2(0f, 0.65f);
            titleContainerRT.anchorMax = new Vector2(1f, 1f);
            titleContainerRT.offsetMin = Vector2.zero;
            titleContainerRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup titleLayout = titleContainer.AddComponent<VerticalLayoutGroup>();
            titleLayout.childAlignment = TextAnchor.LowerCenter; // Alinear abajo para que no flote demasiado alto
            titleLayout.spacing = 10f;
            titleLayout.childControlHeight = false;
            titleLayout.childControlWidth = true;

            // Título
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(titleContainer.transform, false);
            RectTransform titleObjRT = titleObj.AddComponent<RectTransform>();
            titleObjRT.sizeDelta = new Vector2(0, 80f); // Altura fija
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "OUTRUN EXTINCTION";
            titleTMP.fontSize = 65;
            titleTMP.color = titleColor;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            if (TMP_Settings.defaultFontAsset != null) titleTMP.font = TMP_Settings.defaultFontAsset;
            
            Shadow shadow = titleObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.8f);
            shadow.effectDistance = new Vector2(4, -4);
            
            // Animación del contenedor completo
            _titleRT = titleContainerRT;

            // Subtítulo
            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(titleContainer.transform, false);
            RectTransform subObjRT = subObj.AddComponent<RectTransform>();
            subObjRT.sizeDelta = new Vector2(0, 40f); // Altura fija
            TextMeshProUGUI subTMP = subObj.AddComponent<TextMeshProUGUI>();
            subTMP.text = "ENDLESS RUNNER";
            subTMP.fontSize = 28;
            subTMP.color = new Color(0f, 1f, 0.8f, 1f);
            subTMP.fontStyle = FontStyles.Bold;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.characterSpacing = 15f; 
            if (TMP_Settings.defaultFontAsset != null) subTMP.font = TMP_Settings.defaultFontAsset;

            // ── Zona de Monedas (Arriba Derecha) ─────────────────────────
            GameObject coinContainer = CreatePanel(_panelMain.transform, "CoinContainer", new Color(0.1f, 0.05f, 0.15f, 0.9f));
            RectTransform coinRT = coinContainer.GetComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(1f, 1f);
            coinRT.anchorMax = new Vector2(1f, 1f);
            coinRT.pivot = new Vector2(1f, 1f);
            coinRT.anchoredPosition = new Vector2(-40f, -40f);
            coinRT.sizeDelta = new Vector2(300f, 60f); // Un poco más ancho

            Outline coinOutline = coinContainer.AddComponent<Outline>();
            coinOutline.effectColor = new Color(1f, 0.1f, 0.6f, 0.5f);
            coinOutline.effectDistance = new Vector2(2, -2);

            _coinDisplayText = CreateText(coinContainer.transform, "CoinText", "MONEDAS: 0",
                24, new Color(1f, 0.85f, 0.2f, 1f), FontStyles.Bold, 60f);
            RectTransform coinTextRT = _coinDisplayText.rectTransform;
            coinTextRT.anchorMin = Vector2.zero;
            coinTextRT.anchorMax = Vector2.one;
            coinTextRT.offsetMin = Vector2.zero;
            coinTextRT.offsetMax = Vector2.zero;
            UpdateCoinDisplay();

            // ── Zona de Botones (Centro Abajo) ───────────────────────────
            GameObject btnContainer = CreatePanel(_panelMain.transform, "ButtonContainer", Color.clear);
            RectTransform btnRT = btnContainer.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.35f, 0.05f); // 30% del ancho de la pantalla
            btnRT.anchorMax = new Vector2(0.65f, 0.55f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup btnLayout = btnContainer.AddComponent<VerticalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.UpperCenter;
            btnLayout.spacing = 15f;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = false;

            // Botón Principal JUGAR (Más grande y llamativo)
            GameObject btnPlay = CreateButton(btnContainer.transform, "BtnPlay", "JUGAR", OnPlayButton);
            RectTransform playRT = btnPlay.GetComponent<RectTransform>();
            playRT.sizeDelta = new Vector2(0, 90f); 
            TextMeshProUGUI playText = btnPlay.GetComponentInChildren<TextMeshProUGUI>();
            playText.fontSize = 36; 
            StartCoroutine(PulseRoutine(btnPlay.transform));

            CreateSeparator(btnContainer.transform);

            // Botones Secundarios
            CreateButton(btnContainer.transform, "BtnShop", "TIENDA", OnShopButton);
            CreateButton(btnContainer.transform, "BtnSkills", "HABILIDADES", OnSkillsButton);
            CreateButton(btnContainer.transform, "BtnOptions", "OPCIONES", OnOptionsButton);
            
            // ── Botón de Salir (Esquina Superior Izquierda) ──────────────────
            GameObject btnQuit = CreateButton(_panelMain.transform, "BtnQuit", "X", OnQuitButton);
            RectTransform quitRT = btnQuit.GetComponent<RectTransform>();
            quitRT.anchorMin = new Vector2(0f, 1f);
            quitRT.anchorMax = new Vector2(0f, 1f);
            quitRT.pivot = new Vector2(0f, 1f);
            quitRT.anchoredPosition = new Vector2(40f, -40f);
            quitRT.sizeDelta = new Vector2(60f, 60f); // Cuadrado

            Button quitBtnComp = btnQuit.GetComponent<Button>();
            ColorBlock qc = quitBtnComp.colors;
            qc.normalColor = new Color(0.2f, 0.05f, 0.1f, 0.9f); // Fondo rojizo oscuro
            qc.highlightedColor = new Color(1f, 0.1f, 0.2f, 1f); // Rojo brillante
            quitBtnComp.colors = qc;

            Outline quitOutline = btnQuit.AddComponent<Outline>();
            quitOutline.effectColor = new Color(1f, 0.1f, 0.2f, 0.5f);
            quitOutline.effectDistance = new Vector2(2, -2);

            TextMeshProUGUI quitText = btnQuit.GetComponentInChildren<TextMeshProUGUI>();
            quitText.fontSize = 40;
            quitText.color = new Color(1f, 0.8f, 0.8f, 1f);
            quitText.alignment = TextAlignmentOptions.Center;
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

        private void BuildSkillsPanel()
        {
            _panelSkills = CreatePanel(_menuCanvas.transform, "PanelSkills", overlayColor);

            GameObject container = CreatePanel(_panelSkills.transform, "Container", Color.clear);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.2f, 0.1f);
            containerRT.anchorMax = new Vector2(0.8f, 0.9f);
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;

            Image containerBG = container.GetComponent<Image>();
            containerBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            SkillTreeController skillsCtrl = GetComponent<SkillTreeController>();
            if (skillsCtrl == null)
                skillsCtrl = gameObject.AddComponent<SkillTreeController>();

            skillsCtrl.BuildSkillTreeUI(container.transform, buttonColor, buttonTextColor,
                titleColor, () => ShowMainPanel());

            _panelSkills.SetActive(false);
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

        private GameObject CreateButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick)
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
            colors.highlightedColor = new Color(1f, 0.1f, 0.6f, 1f);
            colors.pressedColor = new Color(0.8f, 0f, 0.4f, 1f);
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

            return btnObj;
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
