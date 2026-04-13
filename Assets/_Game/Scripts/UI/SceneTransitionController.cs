using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Maneja las transiciones de fade (negro) entre escenas.
    /// Singleton persistente — se crea a sí mismo el Canvas de fade en Awake().
    /// Vive en la escena MainMenu junto al GameManager y persiste con DontDestroyOnLoad.
    /// </summary>
    public class SceneTransitionController : MonoBehaviour
    {
        public static SceneTransitionController Instance { get; private set; }

        [Header("Configuración")]
        [Tooltip("Duración del fade en segundos")]
        public float fadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;
        private bool _isTransitioning = false;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                BuildFadeCanvas();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Al arrancar la escena, siempre hacer fade in para revelar el contenido
            StartCoroutine(DoFade(1f, 0f));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region API Pública

        /// <summary>
        /// Hace fade out a negro, carga la escena y hace fade in.
        /// </summary>
        public void FadeToScene(string sceneName)
        {
            if (!_isTransitioning)
                StartCoroutine(TransitionRoutine(sceneName));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Coroutines

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _isTransitioning = true;

            // 1. Fade a negro
            yield return StartCoroutine(DoFade(0f, 1f));

            // 2. Cargar escena de forma asíncrona
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            // Esperar hasta que la escena esté lista (90%)
            while (asyncOp.progress < 0.9f)
                yield return null;

            // 3. Activar la nueva escena
            asyncOp.allowSceneActivation = true;
            yield return asyncOp;

            // 4. Fade desde negro a transparente
            yield return StartCoroutine(DoFade(1f, 0f));

            _isTransitioning = false;
        }

        private IEnumerator DoFade(float from, float to)
        {
            _canvasGroup.alpha = from;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // unscaled para funcionar con timeScale = 0
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = to;
            _canvasGroup.blocksRaycasts = (to > 0.5f); // bloquear clicks solo si está opaco
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Setup del Canvas (automático, no requiere setup manual)

        private void BuildFadeCanvas()
        {
            // Canvas de overlay de máxima prioridad
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // CanvasGroup en el root canvas para controlar alpha global
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;          // comienza opaco (negro)
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = false;

            // Panel negro que cubre toda la pantalla
            GameObject overlay = new GameObject("FadeOverlay");
            overlay.transform.SetParent(transform, false);

            Image img = overlay.AddComponent<Image>();
            img.color = Color.black;

            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
