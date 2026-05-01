using UnityEngine;
using TMPro;

namespace Platformer.UI
{
    /// <summary>
    /// Muestra un contador de Fotogramas Por Segundo (FPS) en pantalla si la opción está activada.
    /// Se auto-instancia si es necesario y gestiona su propia UI.
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        private const string SHOW_FPS_KEY = "ShowFPS";
        private const float UPDATE_INTERVAL = 0.5f;

        private static FPSCounter _instance;
        private TextMeshProUGUI _fpsText;
        private float _accum = 0f;
        private int _frames = 0;
        private float _timeLeft;
        private bool _isShowing;

        public static FPSCounter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<FPSCounter>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[FPSCounter]");
                        _instance = go.AddComponent<FPSCounter>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                _timeLeft = UPDATE_INTERVAL;
                CheckPreference();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Comprueba las preferencias del usuario y activa/desactiva el contador.
        /// Llamado al iniciar y cuando se cambia la opción en el menú.
        /// </summary>
        public void CheckPreference()
        {
            _isShowing = PlayerPrefs.GetInt(SHOW_FPS_KEY, 0) == 1;

            if (_isShowing && _fpsText == null)
            {
                CreateUI();
            }
            
            if (_fpsText != null)
            {
                _fpsText.gameObject.SetActive(_isShowing);
            }
        }

        void Update()
        {
            if (!_isShowing || _fpsText == null) return;

            _timeLeft -= Time.unscaledDeltaTime;
            _accum += Time.unscaledDeltaTime;
            _frames++;

            if (_timeLeft <= 0.0)
            {
                float fps = _frames / _accum;
                _fpsText.text = $"{Mathf.RoundToInt(fps)} FPS";

                // Colores simples según rendimiento
                if (fps >= 50)
                    _fpsText.color = Color.green;
                else if (fps >= 30)
                    _fpsText.color = Color.yellow;
                else
                    _fpsText.color = Color.red;

                _timeLeft = UPDATE_INTERVAL;
                _accum = 0.0f;
                _frames = 0;
            }
        }

        private void CreateUI()
        {
            // Crear Canvas
            GameObject canvasObj = new GameObject("FPSCanvas");
            canvasObj.transform.SetParent(this.transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Siempre por encima de todo

            // Crear Texto
            GameObject textObj = new GameObject("FPSText");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rt = textObj.AddComponent<RectTransform>();
            // Posicionar en la esquina superior izquierda
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(100, 30);

            _fpsText = textObj.AddComponent<TextMeshProUGUI>();
            _fpsText.fontSize = 24;
            _fpsText.alignment = TextAlignmentOptions.TopLeft;
            _fpsText.fontStyle = FontStyles.Bold;
            _fpsText.enableWordWrapping = false;
            _fpsText.raycastTarget = false;

            // Borde/sombra negra para que sea visible en cualquier fondo
            _fpsText.fontMaterial.EnableKeyword("UNDERLAY_ON");
            _fpsText.fontMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.8f));
            _fpsText.fontMaterial.SetFloat("_UnderlayOffsetX", 1f);
            _fpsText.fontMaterial.SetFloat("_UnderlayOffsetY", -1f);
            _fpsText.fontMaterial.SetFloat("_UnderlayDilate", 0.5f);
        }
    }
}
