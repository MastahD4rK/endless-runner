using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Singleton persistente que gestiona la divisa de monedas entre sesiones.
    /// Las monedas se acumulan al final de cada partida y se guardan en PlayerPrefs.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        private static CurrencyManager _instance;
        public static CurrencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Buscar en la escena por si ya existe
                    _instance = FindFirstObjectByType<CurrencyManager>();
                    
                    // Si no existe, auto-crearlo
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[CurrencyManager]");
                        _instance = go.AddComponent<CurrencyManager>();
                        // Nota: el Awake() se ejecutará inmediatamente después del AddComponent
                    }
                }
                return _instance;
            }
        }

        private const string COINS_KEY = "TotalCoins";

        /// <summary>Monedas totales acumuladas entre todas las sesiones.</summary>
        public int TotalCoins { get; private set; }

        /// <summary>Monedas recogidas durante la partida actual (no guardadas aún).</summary>
        public int SessionCoins { get; private set; }

        // ─────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadCoins();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region API Pública

        /// <summary>
        /// Suma monedas durante el gameplay (llamado por ScoreCoin al recoger una moneda).
        /// Solo incrementa el contador de sesión; no se guardan hasta CommitSessionCoins().
        /// </summary>
        public void AddSessionCoins(int amount)
        {
            SessionCoins += amount;
        }

        /// <summary>
        /// Consolida las monedas de la sesión actual al total permanente y las guarda.
        /// Llamado al final de una partida (cuando el jugador muere).
        /// </summary>
        public void CommitSessionCoins()
        {
            TotalCoins += SessionCoins;
            SessionCoins = 0;
            SaveCoins();
            Debug.Log($"[CurrencyManager] Monedas guardadas. Total: {TotalCoins}");
        }

        /// <summary>
        /// Gasta monedas del total permanente. Retorna true si tenía suficientes.
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (TotalCoins < amount) return false;

            TotalCoins -= amount;
            SaveCoins();
            return true;
        }

        /// <summary>
        /// Reinicia las monedas de sesión (útil al reiniciar partida sin morir).
        /// </summary>
        public void ResetSessionCoins()
        {
            SessionCoins = 0;
        }

        /// <summary>Borra todo el progreso de monedas (debug).</summary>
        [ContextMenu("Borrar monedas guardadas")]
        public void ResetAllCoins()
        {
            TotalCoins = 0;
            SessionCoins = 0;
            SaveCoins();
            Debug.Log("[CurrencyManager] Monedas borradas.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        #region Persistencia

        private void SaveCoins()
        {
            PlayerPrefs.SetInt(COINS_KEY, TotalCoins);
            PlayerPrefs.Save();
        }

        private void LoadCoins()
        {
            TotalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
            Debug.Log($"[CurrencyManager] Monedas cargadas: {TotalCoins}");
        }

        #endregion
    }
}
