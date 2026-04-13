using Platformer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Gestiona la tarjeta de un nivel individual en el selector de niveles.
    /// Asignar este script al prefab LevelCard y configurar las referencias en el Inspector.
    /// </summary>
    public class LevelCardController : MonoBehaviour
    {
        [Header("Componentes de la Tarjeta")]
        [Tooltip("Imagen de previsualización del nivel")]
        public Image previewImage;

        [Tooltip("Texto con el nombre del nivel")]
        public TextMeshProUGUI levelNameText;

        [Tooltip("Texto con el tipo ('NIVEL' o 'JEFE: NombreJefe')")]
        public TextMeshProUGUI levelTypeText;

        [Tooltip("Panel semi-transparente que se superpone si el nivel está bloqueado")]
        public GameObject lockOverlay;

        [Tooltip("Ícono de candado dentro del lockOverlay")]
        public Image lockIcon;

        [Header("Colores")]
        public Color normalColor = new Color(0.15f, 0.15f, 0.2f);
        public Color bossColor   = new Color(0.4f,  0.1f,  0.1f);

        private Button _button;

        void Awake()
        {
            _button = GetComponent<Button>();
        }

        /// <summary>
        /// Configura la tarjeta con los datos del nivel y el estado de desbloqueo.
        /// </summary>
        /// <param name="data">Datos del nivel</param>
        /// <param name="unlocked">Si el nivel está desbloqueado</param>
        /// <param name="onClickAction">Acción a ejecutar al hacer click</param>
        public void Setup(LevelData data, bool unlocked, System.Action onClickAction)
        {
            // Nombre del nivel
            if (levelNameText != null)
                levelNameText.text = data.displayName;

            // Tipo (nivel normal vs jefe)
            if (levelTypeText != null)
                levelTypeText.text = data.isBossLevel ? $"⚔ {data.bossName}" : "NIVEL";

            // Imagen de previsualización
            if (previewImage != null && data.previewSprite != null)
                previewImage.sprite = data.previewSprite;

            // Color temático del fondo (jefes en rojo oscuro)
            Image cardBackground = GetComponent<Image>();
            if (cardBackground != null)
                cardBackground.color = data.isBossLevel ? bossColor : normalColor;

            // Estado de bloqueo
            if (lockOverlay != null)
                lockOverlay.SetActive(!unlocked);

            // Botón
            if (_button != null)
            {
                _button.interactable = unlocked;
                _button.onClick.RemoveAllListeners();
                if (unlocked)
                    _button.onClick.AddListener(() => onClickAction?.Invoke());
            }
        }
    }
}
