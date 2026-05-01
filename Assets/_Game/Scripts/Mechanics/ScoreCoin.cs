using Platformer.Gameplay;
using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Componente optimizado para monedas (Tokens) en el Endless Runner.
    /// Al colisionar con el jugador, suma puntos y se deshabilita para ser reciclado.
    /// </summary>
    public class ScoreCoin : MonoBehaviour
    {
        [Tooltip("Puntos que da esta moneda al ser recogida")]
        public int scoreValue = 50;

        [Tooltip("Sonido al recoger la moneda")]
        public AudioClip collectAudio;

        void Awake()
        {
            // Auto-configurar Collider si no existe
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;
        }

        // Utilizamos OnTriggerEnter2D asumiendo que la moneda tiene un Collider en modo Trigger
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CollectCoin();
            }
        }

        private void CollectCoin()
        {
            // Reproducir sonido
            if (collectAudio != null)
            {
                AudioSource.PlayClipAtPoint(collectAudio, transform.position);
            }

            // Sumar puntos al score visual
            if (ScoreCounter.Instance != null)
            {
                ScoreCounter.Instance.AddBonusScore(scoreValue);
            }

            // Sumar moneda como divisa persistente
            if (Platformer.Core.CurrencyManager.Instance != null)
            {
                Platformer.Core.CurrencyManager.Instance.AddSessionCoins(1);
            }

            // Deshabilitar la moneda (el Object Pool la limpiará de pantalla después)
            gameObject.SetActive(false);
        }
    }
}
