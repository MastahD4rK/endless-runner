using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.UI
{
    /// <summary>
    /// Gestiona las opciones de audio del juego.
    /// Se construye su propia UI por código cuando es invocado por MainMenuController.
    /// Guarda los valores en PlayerPrefs y los aplica al arrancar.
    /// </summary>
    public class OptionsController : MonoBehaviour
    {
        // Claves de PlayerPrefs
        private const string MUSIC_VOL_KEY  = "VolumenMusica";
        private const string SFX_VOL_KEY    = "VolumenSFX";
        private const string SHOW_FPS_KEY   = "ShowFPS";
        private const string FULLSCREEN_KEY = "Fullscreen";
        private const float  DEFAULT_VOLUME = 0.8f;

        // ── Referencias internas ─────────────────────────────────────
        private Slider _musicSlider;
        private Slider _sfxSlider;

        // ─────────────────────────────────────────────────────────────
        #region API Pública — Construcción de UI

        /// <summary>
        /// Construye el contenido del panel de Opciones dentro del contenedor proporcionado.
        /// Llamado por MainMenuController al generar la UI del menú principal.
        /// </summary>
        public void BuildOptionsUI(Transform container, Color buttonColor, Color buttonTextColor,
            Color titleColor, System.Action onBackAction)
        {
            VerticalLayoutGroup layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateText(container, "TitleText", "OPCIONES", 48, titleColor, FontStyles.Bold, 80f);
            CreateSeparator(container);

            CreateText(container, "LabelMusic", "MUSICA", 22, Color.white, FontStyles.Normal, 35f);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, DEFAULT_VOLUME);
            _musicSlider = CreateSlider(container, "SliderMusic", musicVol, buttonColor);
            _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            CreateText(container, "LabelSFX", "EFECTOS", 22, Color.white, FontStyles.Normal, 35f);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, DEFAULT_VOLUME);
            _sfxSlider = CreateSlider(container, "SliderSFX", sfxVol, buttonColor);
            _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            CreateSeparator(container);

            bool showFPS = PlayerPrefs.GetInt(SHOW_FPS_KEY, 0) == 1;
            CreateButton(container, "BtnFPS", $"MOSTRAR FPS: {(showFPS ? "SI" : "NO")}", buttonColor, buttonTextColor, () =>
            {
                showFPS = !showFPS;
                PlayerPrefs.SetInt(SHOW_FPS_KEY, showFPS ? 1 : 0);
                PlayerPrefs.Save();
                FPSCounter.Instance.CheckPreference();
                
                Transform btn = container.Find("BtnFPS");
                if (btn != null)
                {
                    TextMeshProUGUI txt = btn.Find("Label")?.GetComponent<TextMeshProUGUI>();
                    if (txt != null) txt.text = $"MOSTRAR FPS: {(showFPS ? "SI" : "NO")}";
                }
            });

            CreateSeparator(container);

            bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
            CreateButton(container, "BtnFullscreen", $"PANTALLA COMPLETA: {(isFullscreen ? "SI" : "NO")}", buttonColor, buttonTextColor, () =>
            {
                isFullscreen = !isFullscreen;
                PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
                PlayerPrefs.Save();
                Screen.fullScreen = isFullscreen;
                
                Transform btn = container.Find("BtnFullscreen");
                if (btn != null)
                {
                    TextMeshProUGUI txt = btn.Find("Label")?.GetComponent<TextMeshProUGUI>();
                    if (txt != null) txt.text = $"PANTALLA COMPLETA: {(isFullscreen ? "SI" : "NO")}";
                }
            });

            CreateSeparator(container);

            CreateButton(container, "BtnBack", "VOLVER", buttonColor, buttonTextColor, () =>
            {
                onBackAction?.Invoke();
            });

            ApplyMusicVolume(musicVol);
            ApplySFXVolume(sfxVol);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Callbacks de Sliders

        public void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
            PlayerPrefs.Save();
            ApplyMusicVolume(value);
        }

        public void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SFX_VOL_KEY, value);
            PlayerPrefs.Save();
            ApplySFXVolume(value);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Aplicación de Volumen

        private void ApplyMusicVolume(float value)
        {
            AudioListener.volume = value;
        }

        private void ApplySFXVolume(float value)
        {
            // TODO: cuando haya AudioMixer separado, aplicar aquí.
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Helpers Públicos

        public static float GetMusicVolume()
            => PlayerPrefs.GetFloat(MUSIC_VOL_KEY, DEFAULT_VOLUME);

        public static float GetSFXVolume()
            => PlayerPrefs.GetFloat(SFX_VOL_KEY, DEFAULT_VOLUME);

        /// <summary>
        /// Aplica las configuraciones guardadas al iniciar el juego.
        /// Llamado por el GameManager.
        /// </summary>
        public static void ApplyStartupPreferences()
        {
            AudioListener.volume = GetMusicVolume();
            // Fullscreen por defecto es true (1)
            Screen.fullScreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Helpers de Construcción de UI

        private Slider CreateSlider(Transform parent, string name, float initialValue, Color accentColor)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);

            RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
            sliderRT.sizeDelta = new Vector2(0, 30f);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.25f);
            bgRT.anchorMax = new Vector2(1f, 0.75f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRT = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRT.offsetMin = new Vector2(5f, 0f);
            fillAreaRT.offsetMax = new Vector2(-5f, 0f);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRT = fillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(
                Mathf.Min(accentColor.r + 0.3f, 1f),
                Mathf.Min(accentColor.g + 0.3f, 1f),
                Mathf.Min(accentColor.b + 0.4f, 1f), 1f);

            // Handle Slide Area
            GameObject handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRT = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.offsetMin = new Vector2(10f, 0f);
            handleAreaRT.offsetMax = new Vector2(-10f, 0f);

            // Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            RectTransform handleRT = handleObj.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(20f, 0f);
            handleRT.anchorMin = new Vector2(0f, 0f);
            handleRT.anchorMax = new Vector2(0f, 1f);
            Image handleImg = handleObj.AddComponent<Image>();
            handleImg.color = Color.white;

            // Conectar componentes al Slider
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;

            ColorBlock colors = slider.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.85f, 1f);
            colors.selectedColor = colors.highlightedColor;
            slider.colors = colors;

            slider.value = initialValue;
            return slider;
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

        private void CreateButton(Transform parent, string name, string label,
            Color btnColor, Color textColor, System.Action onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 60f);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = btnColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            ColorBlock colors = btn.colors;
            colors.normalColor = btnColor;
            colors.highlightedColor = new Color(btnColor.r + 0.15f, btnColor.g + 0.15f, btnColor.b + 0.2f, 1f);
            colors.pressedColor = new Color(btnColor.r + 0.25f, btnColor.g + 0.25f, btnColor.b + 0.35f, 1f);
            colors.selectedColor = colors.highlightedColor;
            btn.colors = colors;

            btn.onClick.AddListener(() => onClick?.Invoke());

            TextMeshProUGUI labelTMP = CreateText(btnObj.transform, "Label", label, 24, textColor, FontStyles.Normal, 60f);
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
