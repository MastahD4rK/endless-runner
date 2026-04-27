using Platformer.UI;
using Platformer.View;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Gestiona el cambio de mapa/background por puntuación en el Endless Runner.
    /// 
    /// Funcionalidad:
    /// - Cada N puntos (scorePerMapChange) cambia el background a uno aleatorio.
    /// - Usa sistema de "baraja" (shuffle): recorre TODOS los mapas antes de repetir.
    /// - Transición crossfade gradual: el nuevo mapa se desvanece sobre el actual
    ///   comenzando X puntos antes del umbral (fadeAnticipation).
    /// 
    /// Configuración en Inspector:
    /// 1. Arrastrar los prefabs de background al array "mapPrefabs".
    /// 2. Asignar "initialBackground" al GameObject de background actual en la escena.
    /// 3. Asignar "backgroundParent" al Transform padre donde se instancian los backgrounds.
    /// 4. Ajustar scorePerMapChange (default: 5000).
    /// 
    /// NOTA: El piso (Piso_Base, tilemap, etc.) NO debe ser hijo del backgroundParent.
    /// El MapManager solo gestiona los backgrounds visuales, nunca toca el piso.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("Configuración de Mapas")]
        [Tooltip("Prefabs de background para rotar. Arrastrar los prefabs de Nature, Moon, Sky, etc.")]
        public GameObject[] mapPrefabs;

        [Tooltip("El GameObject de background que ya está en la escena al iniciar. " +
                 "Este será el primer mapa y se destruirá al cambiar. " +
                 "NO asignar el piso aquí, solo el background visual.")]
        public GameObject initialBackground;

        [Tooltip("Transform padre donde se instancian los nuevos backgrounds. " +
                 "NO debe contener el piso como hijo.")]
        public Transform backgroundParent;

        [Header("Puntuación")]
        [Tooltip("Cada cuántos puntos se cambia de mapa")]
        public int scorePerMapChange = 5000;

        [Tooltip("Cuántos puntos antes del umbral empieza la transición de crossfade")]
        public int fadeAnticipation = 500;

        [Header("Parallax Automático")]
        [Tooltip("Factor mínimo de parallax (para las capas más lejanas)")]
        public float minParallaxFactor = 0.05f;

        [Tooltip("Factor máximo de parallax (para las capas más cercanas)")]
        public float maxParallaxFactor = 0.8f;

        // ── Estado interno ──────────────────────────────────────────
        private int _nextChangeScore;        // Próximo umbral de cambio
        private int _mapChangeCount = 0;     // Cuántos cambios se han hecho

        // ── Baraja de mapas ─────────────────────────────────────────
        private List<int> _shuffledIndices = new List<int>();
        private int _shufflePosition = 0;

        // ── Crossfade ───────────────────────────────────────────────
        private GameObject _currentBackground;      // Background activo (el que se ve)
        private SpriteRenderer[] _currentRenderers;  // Renderers del background actual
        private GameObject _nextBackground;          // Background que está entrando (crossfade)
        private SpriteRenderer[] _nextRenderers;     // Renderers del background entrante
        private bool _isFading = false;
        private float _fadeStartScore;               // Score en el que empezó el fade
        private float _fadeEndScore;                 // Score en el que termina el fade
        private bool _subscribedToScore = false;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            if (mapPrefabs == null || mapPrefabs.Length == 0)
            {
                Debug.LogWarning("[MapManager] No hay prefabs de mapas asignados. El sistema de cambio de mapa está deshabilitado.");
                enabled = false;
                return;
            }

            // Usar el background que el usuario asignó en el Inspector
            if (initialBackground != null)
            {
                _currentBackground = initialBackground;
                _currentRenderers = _currentBackground.GetComponentsInChildren<SpriteRenderer>();
                Debug.Log($"[MapManager] Background inicial: '{_currentBackground.name}'");
            }
            else
            {
                Debug.LogWarning("[MapManager] No se asignó 'initialBackground'. " +
                                 "El primer cambio de mapa no podrá hacer crossfade del viejo.");
            }

            // Preparar la baraja
            InitializeShuffle();

            // Primer cambio ocurre en scorePerMapChange
            _nextChangeScore = scorePerMapChange;

            // Suscribirse al evento de score
            if (ScoreCounter.Instance != null)
            {
                ScoreCounter.Instance.OnScoreChanged += HandleScoreChanged;
                _subscribedToScore = true;
            }
            else
            {
                Debug.LogWarning("[MapManager] ScoreCounter.Instance no encontrado al inicio. Se buscará en Update.");
            }
        }

        void Update()
        {
            // Lazy subscription si ScoreCounter no existía al Start
            if (!_subscribedToScore && ScoreCounter.Instance != null)
            {
                ScoreCounter.Instance.OnScoreChanged += HandleScoreChanged;
                _subscribedToScore = true;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Desuscribirse para evitar leaks
            if (ScoreCounter.Instance != null)
            {
                ScoreCounter.Instance.OnScoreChanged -= HandleScoreChanged;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Baraja (Shuffle sin repetición)

        /// <summary>
        /// Inicializa la baraja: crea una lista de índices y la mezcla.
        /// </summary>
        void InitializeShuffle()
        {
            _shuffledIndices.Clear();
            for (int i = 0; i < mapPrefabs.Length; i++)
            {
                _shuffledIndices.Add(i);
            }
            ShuffleList(_shuffledIndices);
            _shufflePosition = 0;

            Debug.Log($"[MapManager] Baraja inicializada con {mapPrefabs.Length} mapas.");
        }

        /// <summary>
        /// Obtiene el siguiente índice de mapa de la baraja.
        /// Si se acabaron todos, re-baraja y empieza de nuevo.
        /// </summary>
        int GetNextMapIndex()
        {
            if (_shufflePosition >= _shuffledIndices.Count)
            {
                // Se acabó la baraja — re-mezclar
                ShuffleList(_shuffledIndices);
                _shufflePosition = 0;
                Debug.Log("[MapManager] Baraja agotada — re-mezclando todos los mapas.");
            }

            int index = _shuffledIndices[_shufflePosition];
            _shufflePosition++;
            return index;
        }

        /// <summary>
        /// Fisher-Yates shuffle para mezclar la lista in-place.
        /// </summary>
        void ShuffleList(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Lógica de Cambio de Mapa

        /// <summary>
        /// Callback del evento OnScoreChanged del ScoreCounter.
        /// Gestiona cuándo iniciar el crossfade y cuándo completar el cambio.
        /// </summary>
        void HandleScoreChanged(int currentScore)
        {
            // ── ¿Es hora de empezar el fade? (empieza EN el umbral, no antes) ──
            if (!_isFading && currentScore >= _nextChangeScore)
            {
                StartCrossfade();
            }

            // ── Actualizar el alpha del crossfade ───────────────────
            if (_isFading && _nextBackground != null)
            {
                float progress = Mathf.InverseLerp(_fadeStartScore, _fadeEndScore, currentScore);
                progress = Mathf.Clamp01(progress);

                // Solo fade-in del nuevo background encima del viejo.
                // El viejo se mantiene sólido (alpha=1) para que las capas
                // de suelo/vegetación no desaparezcan durante la transición.
                SetBackgroundAlpha(_nextRenderers, progress);

                // ¿Terminó el fade?
                if (progress >= 1f)
                {
                    CompleteCrossfade();
                }
            }
        }

        /// <summary>
        /// Inicia el crossfade: instancia el nuevo background con alpha 0.
        /// </summary>
        void StartCrossfade()
        {
            if (backgroundParent == null)
            {
                Debug.LogError("[MapManager] backgroundParent no asignado.");
                return;
            }

            int nextIndex = GetNextMapIndex();
            _isFading = true;
            // El fade empieza EN el umbral y termina fadeAnticipation puntos DESPUÉS
            _fadeStartScore = _nextChangeScore;
            _fadeEndScore = _nextChangeScore + fadeAnticipation;

            // Instanciar el nuevo background como hijo del padre de backgrounds
            _nextBackground = Instantiate(mapPrefabs[nextIndex], backgroundParent);
            _nextBackground.transform.localPosition = Vector3.zero;
            _nextBackground.name = $"Background_{mapPrefabs[nextIndex].name}";

            // Configurar ParallaxLayer en cada capa del nuevo background
            ConfigureParallax(_nextBackground);

            // Obtener todos los SpriteRenderers del nuevo background
            _nextRenderers = _nextBackground.GetComponentsInChildren<SpriteRenderer>();

            // Empezar invisible (alpha = 0)
            SetBackgroundAlpha(_nextRenderers, 0f);

            Debug.Log($"[MapManager] Crossfade iniciado → Mapa #{nextIndex} ('{mapPrefabs[nextIndex].name}'). " +
                       $"Fade de score {_fadeStartScore} a {_fadeEndScore}.");
        }

        /// <summary>
        /// Completa el crossfade: destruye el background viejo y prepara el próximo cambio.
        /// </summary>
        void CompleteCrossfade()
        {
            // Asegurar alpha = 1 en el nuevo
            SetBackgroundAlpha(_nextRenderers, 1f);

            // Destruir SOLO el background viejo (nunca el piso)
            if (_currentBackground != null)
            {
                Debug.Log($"[MapManager] Destruyendo background viejo: '{_currentBackground.name}'");
                Destroy(_currentBackground);
            }

            // El nuevo pasa a ser el actual
            _currentBackground = _nextBackground;
            _currentRenderers = _nextRenderers;
            _nextBackground = null;
            _nextRenderers = null;
            _isFading = false;

            // Preparar el próximo cambio
            _mapChangeCount++;
            _nextChangeScore = scorePerMapChange * (_mapChangeCount + 1);

            Debug.Log($"[MapManager] Cambio de mapa completado (cambio #{_mapChangeCount}). " +
                       $"Próximo cambio en score: {_nextChangeScore}.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Utilidades

        /// <summary>
        /// Establece el alpha de todos los SpriteRenderers de un background.
        /// </summary>
        void SetBackgroundAlpha(SpriteRenderer[] renderers, float alpha)
        {
            if (renderers == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = renderers[i].color;
                c.a = alpha;
                renderers[i].color = c;
            }
        }

        /// <summary>
        /// Agrega ParallaxLayer a cada capa hija del background.
        /// Las capas se ordenan por su índice de hijo:
        /// - Los primeros hijos son las capas más lejanas (factor bajo = se mueven lento)
        /// - Los últimos hijos son las capas más cercanas (factor alto = se mueven rápido)
        /// </summary>
        void ConfigureParallax(GameObject background)
        {
            int childCount = background.transform.childCount;
            if (childCount == 0)
            {
                // Si no tiene hijos, el parallax va directamente en el objeto
                ParallaxLayer pl = background.GetComponent<ParallaxLayer>();
                if (pl == null) pl = background.AddComponent<ParallaxLayer>();
                pl.parallaxFactor = 0.5f;
                return;
            }

            for (int i = 0; i < childCount; i++)
            {
                Transform child = background.transform.GetChild(i);
                ParallaxLayer pl = child.GetComponent<ParallaxLayer>();
                if (pl == null) pl = child.gameObject.AddComponent<ParallaxLayer>();

                // Factor de parallax progresivo: de min a max según la profundidad
                float t = childCount > 1 ? (float)i / (childCount - 1) : 0.5f;
                pl.parallaxFactor = Mathf.Lerp(minParallaxFactor, maxParallaxFactor, t);
            }
        }

        #endregion
    }
}
