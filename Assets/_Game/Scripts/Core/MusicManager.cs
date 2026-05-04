using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Core
{
    /// <summary>
    /// Singleton persistente que reproduce la música del juego.
    /// Vive en la escena MainMenu y sobrevive con DontDestroyOnLoad.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [Header("Configuración")]
        [Tooltip("Clip de música principal (asignar en el Inspector)")]
        public AudioClip musicClip;

        [Tooltip("Volumen base de la música (0-1)")]
        [Range(0f, 1f)]
        public float baseVolume = 0.07f;

        private AudioSource _audioSource;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            Debug.Log("[MusicManager] Awake() ejecutado.");

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupAudioSource();
            }
            else
            {
                Debug.Log("[MusicManager] Instancia duplicada — destruyendo.");
                Destroy(gameObject);
                return;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[MusicManager] Escena cargada: {scene.name} — AudioSource playing: {(_audioSource != null ? _audioSource.isPlaying.ToString() : "NULL")}");

            if (_audioSource != null && !_audioSource.isPlaying && _audioSource.clip != null)
            {
                Debug.Log("[MusicManager] Reanudando música tras cambio de escena.");
                _audioSource.Play();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Setup

        private void SetupAudioSource()
        {
            // Obtener o crear el AudioSource
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.Log("[MusicManager] No se encontró AudioSource, creando uno nuevo.");
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Detener cualquier reproducción previa (de PlayOnAwake)
            _audioSource.Stop();

            // Determinar qué clip usar:
            // Prioridad 1: musicClip asignado en el script
            // Prioridad 2: clip ya asignado en el AudioSource del Inspector
            AudioClip clipToUse = musicClip;
            if (clipToUse == null)
            {
                clipToUse = _audioSource.clip;
                Debug.Log($"[MusicManager] musicClip del script es null, usando clip del AudioSource: {(clipToUse != null ? clipToUse.name : "TAMBIÉN NULL")}");
            }

            Debug.Log($"[MusicManager] Clip a usar: {(clipToUse != null ? clipToUse.name : "NULL")}");
            Debug.Log($"[MusicManager] baseVolume: {baseVolume}");

            // Configurar AudioSource
            _audioSource.clip = clipToUse;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = baseVolume;
            _audioSource.spatialBlend = 0f;
            _audioSource.ignoreListenerPause = true;
            _audioSource.priority = 0;

            if (clipToUse != null)
            {
                _audioSource.Play();
                Debug.Log($"[MusicManager] ✓ Música iniciada: {clipToUse.name}, isPlaying: {_audioSource.isPlaying}");
            }
            else
            {
                Debug.LogError("[MusicManager] ✗ NO HAY CLIP DE MÚSICA. Asigna el clip en el Inspector del MusicManager (campo 'Music Clip') o en el AudioSource.");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region API Pública

        public void SetVolume(float volume)
        {
            baseVolume = Mathf.Clamp01(volume);
            if (_audioSource != null)
                _audioSource.volume = baseVolume;
        }

        public void Pause()
        {
            if (_audioSource != null && _audioSource.isPlaying)
                _audioSource.Pause();
        }

        public void Resume()
        {
            if (_audioSource != null && !_audioSource.isPlaying)
                _audioSource.UnPause();
        }

        public void ChangeMusic(AudioClip newClip)
        {
            if (_audioSource == null || newClip == null) return;
            _audioSource.Stop();
            _audioSource.clip = newClip;
            musicClip = newClip;
            _audioSource.Play();
        }

        #endregion
    }
}
