using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Platformer.Core;
using System.Collections.Generic;

namespace Platformer.UI
{
    /// <summary>
    /// Controlador de UI para el Árbol de Habilidades, generado dinámicamente por código.
    /// </summary>
    public class SkillTreeController : MonoBehaviour
    {
        private Color _buttonColor;
        private Color _buttonTextColor;
        private Color _titleColor;
        private System.Action _onBack;
        private TextMeshProUGUI _coinText;

        // Referencias a los textos de nivel de cada habilidad para actualizarlos dinámicamente
        private Dictionary<SkillType, TextMeshProUGUI> _levelTexts = new Dictionary<SkillType, TextMeshProUGUI>();
        private Dictionary<SkillType, TextMeshProUGUI> _costTexts = new Dictionary<SkillType, TextMeshProUGUI>();
        private Dictionary<SkillType, Button> _upgradeButtons = new Dictionary<SkillType, Button>();

        public void BuildSkillTreeUI(Transform parent, Color btnColor, Color btnTextColor, Color titleColor, System.Action onBack)
        {
            _buttonColor = btnColor;
            _buttonTextColor = btnTextColor;
            _titleColor = titleColor;
            _onBack = onBack;

            // Layout general
            VerticalLayoutGroup layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Título
            CreateText(parent, "Title", "HABILIDADES", 40, _titleColor, FontStyles.Bold, 60f);
            
            // Monedas actuales
            _coinText = CreateText(parent, "CoinText", $"MONEDAS: {CurrencyManager.Instance.TotalCoins}", 24, new Color(1f, 0.85f, 0.2f, 1f), FontStyles.Bold, 40f);

            CreateSeparator(parent);

            // Contenedor para la lista de habilidades
            GameObject scrollView = new GameObject("SkillsScrollView");
            scrollView.transform.SetParent(parent, false);
            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            scrollRT.sizeDelta = new Vector2(0, 400f); // Alto fijo para la lista

            VerticalLayoutGroup scrollLayout = scrollView.AddComponent<VerticalLayoutGroup>();
            scrollLayout.spacing = 15f;
            scrollLayout.childControlWidth = true;
            scrollLayout.childForceExpandWidth = true;
            scrollLayout.childForceExpandHeight = false;

            // Generar tarjetas de habilidades
            if (SkillManager.Instance != null && SkillManager.Instance.Skills != null)
            {
                foreach (var kvp in SkillManager.Instance.Skills)
                {
                    CreateSkillCard(scrollView.transform, kvp.Value);
                }
            }

            // Separador inferior
            CreateSeparator(parent);

            // Botón Volver
            CreateButton(parent, "BtnBack", "VOLVER", () => _onBack?.Invoke());

            UpdateAllCards();
        }

        private void CreateSkillCard(Transform parent, SkillData data)
        {
            GameObject card = new GameObject($"Card_{data.type}");
            card.transform.SetParent(parent, false);
            
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 100f);
            
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.5f);

            HorizontalLayoutGroup hl = card.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(20, 20, 10, 10);
            hl.spacing = 15f;
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;

            // Izquierda: Textos (Título, Descripción)
            GameObject textContainer = new GameObject("TextContainer");
            textContainer.transform.SetParent(card.transform, false);
            RectTransform textRT = textContainer.AddComponent<RectTransform>();
            // LayoutElement textLayout = textContainer.AddComponent<LayoutElement>();
            // textLayout.flexibleWidth = 1f;

            VerticalLayoutGroup vl = textContainer.AddComponent<VerticalLayoutGroup>();
            vl.childAlignment = TextAnchor.MiddleLeft;
            vl.childControlWidth = true;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;

            CreateText(textContainer.transform, "Title", data.title, 24, Color.white, FontStyles.Bold, 30f, TextAlignmentOptions.Left);
            CreateText(textContainer.transform, "Desc", data.description, 16, new Color(0.8f, 0.8f, 0.8f, 1f), FontStyles.Normal, 40f, TextAlignmentOptions.TopLeft);

            // Derecha: Nivel y Botón
            GameObject actionContainer = new GameObject("ActionContainer");
            actionContainer.transform.SetParent(card.transform, false);
            VerticalLayoutGroup actionVL = actionContainer.AddComponent<VerticalLayoutGroup>();
            actionVL.childAlignment = TextAnchor.MiddleRight;
            actionVL.spacing = 5f;
            actionVL.childControlWidth = true;
            actionVL.childForceExpandWidth = true;

            TextMeshProUGUI levelTxt = CreateText(actionContainer.transform, "Level", $"Nivel {data.currentLevel} DE {data.maxLevel}", 18, Color.white, FontStyles.Bold, 25f, TextAlignmentOptions.Right);
            _levelTexts[data.type] = levelTxt;

            TextMeshProUGUI costTxt = CreateText(actionContainer.transform, "Cost", "Costo: -", 16, new Color(1f, 0.85f, 0.2f, 1f), FontStyles.Normal, 20f, TextAlignmentOptions.Right);
            _costTexts[data.type] = costTxt;

            Button upgradeBtn = CreateButton(actionContainer.transform, "BtnUpgrade", "MEJORAR", () => OnUpgradeClicked(data.type), 120f, 40f);
            _upgradeButtons[data.type] = upgradeBtn;
            
            // Layout overrides
            LayoutElement layoutElement = textContainer.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;
            
            LayoutElement actionLayout = actionContainer.AddComponent<LayoutElement>();
            actionLayout.minWidth = 120f;
        }

        private void OnUpgradeClicked(SkillType type)
        {
            if (SkillManager.Instance.TryUpgradeSkill(type))
            {
                // Play success sound?
                UpdateAllCards();
            }
            else
            {
                // Play error sound?
            }
        }

        public void UpdateAllCards()
        {
            if (CurrencyManager.Instance != null && _coinText != null)
            {
                _coinText.text = $"MONEDAS: {CurrencyManager.Instance.TotalCoins}";
            }

            if (SkillManager.Instance == null) return;

            foreach (var kvp in SkillManager.Instance.Skills)
            {
                var type = kvp.Key;
                var data = kvp.Value;

                if (_levelTexts.ContainsKey(type))
                {
                    _levelTexts[type].text = $"Nivel {data.currentLevel} DE {data.maxLevel}";
                }

                bool isMax = data.currentLevel >= data.maxLevel;
                if (_costTexts.ContainsKey(type))
                {
                    _costTexts[type].text = isMax ? "MAXIMO" : $"Costo: {data.costPerLevel[data.currentLevel]}";
                }

                if (_upgradeButtons.ContainsKey(type))
                {
                    _upgradeButtons[type].interactable = !isMax && SkillManager.Instance.CanUpgrade(type);
                }
            }
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, Color color, FontStyles style, float height, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
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
            tmp.alignment = alignment;
            tmp.enableAutoSizing = false;
            tmp.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;

            return tmp;
        }

        private Button CreateButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick, float width = 0, float height = 60f)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = _buttonColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            ColorBlock colors = btn.colors;
            colors.normalColor = _buttonColor;
            colors.highlightedColor = new Color(_buttonColor.r + 0.15f, _buttonColor.g + 0.15f, _buttonColor.b + 0.2f, 1f);
            colors.pressedColor = new Color(_buttonColor.r + 0.25f, _buttonColor.g + 0.25f, _buttonColor.b + 0.35f, 1f);
            colors.selectedColor = colors.highlightedColor;
            // Disabled color
            colors.disabledColor = new Color(_buttonColor.r, _buttonColor.g, _buttonColor.b, 0.5f);
            btn.colors = colors;

            btn.onClick.AddListener(onClick);

            TextMeshProUGUI labelTMP = CreateText(btnObj.transform, "Label", label, height * 0.4f, _buttonTextColor, FontStyles.Normal, height);
            
            RectTransform labelRT = labelTMP.rectTransform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            labelRT.sizeDelta = Vector2.zero;

            return btn;
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
    }
}
