using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Controlador de la Tienda. Por ahora solo muestra las monedas acumuladas.
    /// Se construye su propia UI por código, como todos los demás controladores.
    /// Invocado por MainMenuController.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        // ── Referencias internas ─────────────────────────────────────
        private TextMeshProUGUI _totalCoinsText;

        // ─────────────────────────────────────────────────────────────
        #region API Pública — Construcción de UI

        /// <summary>
        /// Construye el contenido del panel de Tienda dentro del contenedor proporcionado.
        /// Llamado por MainMenuController al generar la UI del menú principal.
        /// </summary>
        public void BuildShopUI(Transform container, Color buttonColor, Color buttonTextColor,
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

            // ── Título ───────────────────────────────────────────────
            CreateText(container, "TitleText", "TIENDA", 48, titleColor, FontStyles.Bold, 80f);
            CreateSeparator(container);

            // ── Total de monedas ─────────────────────────────────────
            _totalCoinsText = CreateText(container, "TotalCoinsText", "MONEDAS: 0",
                36, new Color(1f, 0.85f, 0.2f, 1f), FontStyles.Bold, 60f);

            UpdateCoinsDisplay();

            CreateSeparator(container);

            // ── Mensaje informativo ──────────────────────────────────
            CreateText(container, "InfoText", "PROXIMAMENTE...",
                22, new Color(0.5f, 0.5f, 0.55f, 1f), FontStyles.Italic, 40f);

            CreateText(container, "InfoText2", "Recoge monedas en las partidas\npara desbloquear items aqui.",
                18, new Color(0.4f, 0.4f, 0.45f, 1f), FontStyles.Normal, 50f);

            CreateSeparator(container);

            // ── Botón Volver ─────────────────────────────────────────
            CreateButton(container, "BtnBack", "VOLVER", buttonColor, buttonTextColor, () =>
            {
                onBackAction?.Invoke();
            });
        }

        /// <summary>Actualiza el display de monedas (útil si se compra algo).</summary>
        public void UpdateCoinsDisplay()
        {
            if (_totalCoinsText != null && CurrencyManager.Instance != null)
            {
                _totalCoinsText.text = $"MONEDAS: {CurrencyManager.Instance.TotalCoins}";
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Helpers de Construcción de UI

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
