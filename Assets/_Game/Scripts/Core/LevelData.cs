using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Asset de datos que describe un nivel o escena de jefe.
    /// Crear con: clic derecho en Project → Create → Endless Runner → Level Data
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Endless Runner/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identificación")]
        [Tooltip("Nombre exacto de la escena de Unity (debe coincidircon la Build Settings)")]
        public string sceneName;

        [Tooltip("Nombre mostrado en el selector de niveles")]
        public string displayName = "Nivel X";

        [Tooltip("Descripción breve mostrada en la tarjeta del nivel")]
        [TextArea(2, 4)]
        public string description;

        [Header("Tipo de Nivel")]
        [Tooltip("Marcar si esta escena es un combate de jefe")]
        public bool isBossLevel = false;

        [Tooltip("Nombre del jefe (solo si isBossLevel = true)")]
        public string bossName = "";

        [Header("Visual")]
        [Tooltip("Imagen de previsualización del nivel en el selector")]
        public Sprite previewSprite;

        [Tooltip("Color temático del nivel (usado en la tarjeta)")]
        public Color themeColor = new Color(0.2f, 0.6f, 1f);
    }
}
