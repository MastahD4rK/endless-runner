using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Gestiona las opciones de audio del juego.
    /// Guarda los valores en PlayerPrefs y los aplica al arrancar.
    ///
    /// Cómo usar:
    /// 1. Asignar este script a un GameObject en la escena MainMenu.
    /// 2. Asignar los sliders de Music y SFX en el Inspector.
    /// 3. Conectar OnMusicVolumeChanged y OnSFXVolumeChanged en los eventos OnValueChanged de los sliders.
    ///
    /// Nota: por ahora ambos canales afectan AudioListener.volume (volumen global).
    /// Para control independiente, conectar un AudioMixer y setear sus parámetros expuestos.
    /// </summary>
    public class OptionsController : MonoBehaviour
    {
        // Claves de PlayerPrefs
        private const string MUSIC_VOL_KEY  = "VolumenMusica";
        private const string SFX_VOL_KEY    = "VolumenSFX";
        private const float  DEFAULT_VOLUME = 0.8f;

        [Header("Sliders de Audio")]
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void OnEnable()
        {
            // Cargar y aplicar valores cada vez que el panel se activa
            LoadAndApplySettings();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Carga y Guardado

        private void LoadAndApplySettings()
        {
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, DEFAULT_VOLUME);
            float sfxVol   = PlayerPrefs.GetFloat(SFX_VOL_KEY,   DEFAULT_VOLUME);

            // Actualizar sliders sin disparar OnValueChanged
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
                musicVolumeSlider.value = musicVol;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
                sfxVolumeSlider.value = sfxVol;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            ApplyMusicVolume(musicVol);
            ApplySFXVolume(sfxVol);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Callbacks de Sliders

        /// <summary>Llamar desde el evento OnValueChanged del slider de música.</summary>
        public void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
            PlayerPrefs.Save();
            ApplyMusicVolume(value);
        }

        /// <summary>Llamar desde el evento OnValueChanged del slider de SFX.</summary>
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
            // Control global por ahora — aplicar al AudioListener (afecta todo el audio)
            // TODO: para separar música de SFX, conectar un AudioMixer y usar:
            //   mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
            AudioListener.volume = value;
        }

        private void ApplySFXVolume(float value)
        {
            // TODO: cuando haya AudioMixer separado, aplicar aquí.
            // Por ahora el volumen de SFX se guarda pero no se aplica por separado.
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Helpers Públicos

        /// <summary>Retorna el volumen de música guardado (0–1).</summary>
        public static float GetMusicVolume()
            => PlayerPrefs.GetFloat(MUSIC_VOL_KEY, DEFAULT_VOLUME);

        /// <summary>Retorna el volumen de SFX guardado (0–1).</summary>
        public static float GetSFXVolume()
            => PlayerPrefs.GetFloat(SFX_VOL_KEY, DEFAULT_VOLUME);

        #endregion
    }
}
