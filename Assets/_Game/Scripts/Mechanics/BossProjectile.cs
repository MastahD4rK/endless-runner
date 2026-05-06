using UnityEngine;
using Platformer.Mechanics;
using Platformer.Core;

namespace Platformer.Gameplay
{
    public class BossProjectile : MonoBehaviour
    {
        public float moveSpeed = 6f; // Reducido de 10 a 6 para que sea más fácil
        public bool isDeflected = false;

        private BossController _boss;

        public void Initialize(BossController boss)
        {
            _boss = boss;
            isDeflected = false;
            
            // Si el prefab tiene un SpriteRenderer, ponle un color rojo/naranja inicial
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 0.3f, 0f); // Naranja rojizo
        }

        void Update()
        {
            if (GameSpeedManager.Instance != null && !GameSpeedManager.Instance.isPlaying) return;

            // Move left if normal, move right if deflected
            float dir = isDeflected ? 1f : -1f;
            transform.Translate(Vector3.right * dir * moveSpeed * Time.deltaTime);

            // Destroy if out of bounds (fuera de la cámara)
            if (transform.position.x < -15f || transform.position.x > 15f)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            // Si está siendo devuelto (deflected), solo le importa chocar con el Boss
            if (isDeflected)
            {
                var boss = collision.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage();
                    Destroy(gameObject); // El proyectil explota al darle
                }
                return;
            }

            // Si no está siendo devuelto, le importa chocar con el jugador
            var player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // Chequear si el jugador está "pisando" el proyectil
                // player.Bounds.center.y >= bounds.max.y significa que la mitad del jugador está por encima del borde superior del proyectil
                bool isJumpingOnIt = player.Bounds.center.y >= GetComponent<Collider2D>().bounds.max.y;

                if (isJumpingOnIt)
                {
                    // ¡PARRY EXITOSO!
                    Deflect();
                    player.Bounce(5); // Rebote del jugador para seguir en el aire
                }
                else
                {
                    // DAÑO
                    if (player.health.currentHP > 1)
                    {
                        // Pierde un corazón / escudo
                        player.health.Decrement();
                        player.Bounce(4);
                        if (player.audioSource && player.ouchAudio)
                            player.audioSource.PlayOneShot(player.ouchAudio);
                    }
                    else
                    {
                        // Muere
                        Simulation.Schedule<PlayerDeath>();
                    }
                    Destroy(gameObject); // El proyectil se destruye al impactar (o romper escudo)
                }
            }
        }

        void Deflect()
        {
            isDeflected = true;
            moveSpeed *= 1.5f; // Vuelve más rápido hacia el jefe
            
            // Cambiar color a Verde/Cyan para indicar que es aliado ahora
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.cyan; 
        }
    }
}
