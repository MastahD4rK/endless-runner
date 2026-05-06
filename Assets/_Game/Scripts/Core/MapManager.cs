using Platformer.UI;
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
    /// - Transición crossfade basada en TIEMPO REAL (no en puntos) para velocidad consistente.
    /// - Los backgrounds son estáticos (sin parallax) para evitar que las capas
    ///   se desplacen fuera de pantalla, ya que los sprites no tienen tiling.
    /// 
    /// Configuración en Inspector:
    /// 1. Arrastrar los prefabs de background al array "mapPrefabs".
    /// 2. Asignar "initialBackground" al GameObject de background actual en la escena.
    /// 3. Asignar "backgroundParent" al Transform padre donde se instancian los backgrounds.
    /// 4. Ajustar scorePerMapChange (default: 1000) y fadeDuration (default: 2s).
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
        public int scorePerMapChange = 1000;

        [Header("Transición")]
        [Tooltip("Duración del crossfade en segundos (tiempo real, no depende de la velocidad del juego)")]
        public float fadeDuration = 2f;

        [Tooltip("Offset vertical del background. Valores negativos lo bajan. " +
                 "Ajustar para que el suelo visual del fondo se alinee con el suelo del jugador.")]
        public float backgroundYOffset = -1.5f;

        // ── Estado interno ──────────────────────────────────────────
        private int _nextChangeScore;
        private int _mapChangeCount = 0;

        // ── Baraja de mapas ─────────────────────────────────────────
        private List<int> _shuffledIndices = new List<int>();
        private int _shufflePosition = 0;

        // ── Crossfade ───────────────────────────────────────────────
        private GameObject _currentBackground;
        private SpriteRenderer[] _currentRenderers;
        private GameObject _nextBackground;
        private SpriteRenderer[] _nextRenderers;
        private bool _isFading = false;
        private float _fadeStartTime;           // Tiempo real en que empezó el fade
        private bool _subscribedToScore = false;

        // ── Posición/escala de referencia (copiada del background inicial) ──
        private Vector3 _bgPosition;
        private Vector3 _bgScale = Vector3.one;

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

                // Aplicar offset vertical al background inicial
                Vector3 pos = _currentBackground.transform.position;
                pos.y += backgroundYOffset;
                _currentBackground.transform.position = pos;

                // Guardar posición y escala del background inicial como referencia.
                _bgPosition = _currentBackground.transform.position;
                _bgScale = _currentBackground.transform.localScale;

                Debug.Log($"[MapManager] Background inicial: '{_currentBackground.name}' " +
                          $"pos={_bgPosition} scale={_bgScale} (offset Y={backgroundYOffset})");
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

            // Hacer que cualquier área no cubierta por backgrounds sea negra (no gris)
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = Color.black;
            }

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

            // ── Crossfade basado en tiempo real ──────────────────────
            if (_isFading && _nextBackground != null)
            {
                float elapsed = Time.time - _fadeStartTime;
                float progress = Mathf.Clamp01(elapsed / fadeDuration);

                // Fade-in del nuevo background y fade-out del viejo simultáneamente
                SetBackgroundAlpha(_nextRenderers, progress);
                SetBackgroundAlpha(_currentRenderers, 1f - progress);

                if (progress >= 1f)
                {
                    CompleteCrossfade();
                }
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (ScoreCounter.Instance != null)
            {
                ScoreCounter.Instance.OnScoreChanged -= HandleScoreChanged;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Baraja (Shuffle sin repetición)

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

        int GetNextMapIndex()
        {
            if (_shufflePosition >= _shuffledIndices.Count)
            {
                ShuffleList(_shuffledIndices);
                _shufflePosition = 0;
                Debug.Log("[MapManager] Baraja agotada — re-mezclando todos los mapas.");
            }

            int index = _shuffledIndices[_shufflePosition];
            _shufflePosition++;
            return index;
        }

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
        /// Solo se encarga de detectar CUÁNDO iniciar el crossfade.
        /// La animación del fade se actualiza en Update() con tiempo real.
        /// </summary>
        void HandleScoreChanged(int currentScore)
        {
            if (!_isFading && currentScore >= _nextChangeScore)
            {
                StartCrossfade();
            }
        }

        /// <summary>
        /// Inicia el crossfade: instancia el nuevo background con alpha 0.
        /// El fade se controla por tiempo real en Update().
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
            _fadeStartTime = Time.time;

            // Instanciar el nuevo background con la misma posición y escala del inicial
            _nextBackground = Instantiate(mapPrefabs[nextIndex], backgroundParent);
            _nextBackground.name = $"Background_{mapPrefabs[nextIndex].name}";
            _nextBackground.transform.position = _bgPosition;
            _nextBackground.transform.localScale = _bgScale;

            // Obtener todos los SpriteRenderers del nuevo background
            _nextRenderers = _nextBackground.GetComponentsInChildren<SpriteRenderer>();

            // Empezar invisible (alpha = 0)
            SetBackgroundAlpha(_nextRenderers, 0f);

            Debug.Log($"[MapManager] Crossfade iniciado → Mapa #{nextIndex} ('{mapPrefabs[nextIndex].name}'). " +
                       $"Fade de {fadeDuration}s.");
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

        #endregion
    }
}
