using UnityEngine;
using Platformer.Gameplay;
using Platformer.Model;
using Platformer.Core;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Controlador para enemigos tipo Slime en el Endless Runner.
    /// 
    /// El slime camina por el suelo hacia el jugador (moviéndose con el mundo).
    /// Cuando está MUY cerca del jugador, tiene una probabilidad de hacer
    /// un salto pequeño y rápido para dificultar que el jugador lo evada.
    /// 
    /// Física: gravityScale = 0 siempre. Se maneja la velocidad Y manualmente.
    /// El aterrizaje se basa en volver a la posición Y original (groundY),
    /// lo que GARANTIZA que el slime baje después de saltar.
    /// 
    /// Prefab necesita: Rigidbody2D (Dynamic, Gravity Scale 0, Freeze Rotation Z),
    /// Collider2D, Animator, SpriteRenderer.
    /// NO debe tener: AnimationController, KinematicObject, WorldMover.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class SlimeController : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Velocidad adicional del slime (además de la velocidad del mundo)")]
        public float moveSpeed = 2f;

        [Header("Salto")]
        [Tooltip("¿Este slime puede saltar?")]
        public bool canJump = false;

        [Tooltip("Altura del salto en unidades. 1.5 = salto pequeño, 3 = salto alto.")]
        public float jumpHeight = 1.5f;

        [Tooltip("Duración del salto en segundos. Más bajo = hop más rápido.")]
        public float jumpDuration = 0.5f;

        [Header("Zona de Amenaza")]
        [Tooltip("Distancia X al jugador donde decide si salta. 2-3 = muy cerca.")]
        public float threatDistance = 2.5f;

        [Tooltip("Probabilidad de saltar (0-1). 0.3 = 30%.")]
        [Range(0f, 1f)]
        public float jumpProbability = 0.3f;

        // ── Componentes ──────────────────────────────────────────────
        private Rigidbody2D rb;
        private Collider2D _collider;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private GameSpeedManager _speedManager;

        // ── Estado ───────────────────────────────────────────────────
        private bool isJumping = false;
        private bool hasDecidedJump = false;
        private bool willJump = false;
        private Transform playerTransform;

        // ── Física del salto ─────────────────────────────────────────
        // groundY = la posición Y donde el slime fue colocado (nivel del suelo).
        // El salto es un arco parabólico: sube hasta groundY + jumpHeight,
        // luego baja hasta groundY. Se usa jumpTimer/jumpDuration como t [0..1].
        private float groundY;
        private float jumpTimer;

        void Awake()
        {
            // Eliminar componentes conflictivos de inmediato
            var animCtrl = GetComponent<AnimationController>();
            if (animCtrl != null) DestroyImmediate(animCtrl);

            var kinematic = GetComponent<KinematicObject>();
            if (kinematic != null) DestroyImmediate(kinematic);

            rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            hasDecidedJump = false;
            willJump = false;
            isJumping = false;
            jumpTimer = 0f;
            playerTransform = null;
            _speedManager = GameSpeedManager.Instance;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
                rb.gravityScale = 0f; // SIEMPRE 0 — controlamos Y manualmente
                rb.linearVelocity = Vector2.zero;
            }

            // Guardar la posición Y actual como "suelo"
            groundY = transform.position.y;
        }

        void Update()
        {
            if (_speedManager == null)
                _speedManager = GameSpeedManager.Instance;

            // Buscar al jugador
            if (playerTransform == null)
            {
                PlatformerModel model = Simulation.GetModel<PlatformerModel>();
                if (model != null && model.player != null)
                    playerTransform = model.player.transform;
            }

            // Animaciones
            if (animator != null)
            {
                animator.SetBool("grounded", !isJumping);
                animator.SetFloat("velocityX", 1f);
            }

            if (spriteRenderer != null)
                spriteRenderer.flipX = true; // Siempre mirando a la izquierda
        }

        void FixedUpdate()
        {
            // ── Salto: arco parabólico por timer ─────────────────────
            float yOffset = 0f;

            if (isJumping)
            {
                jumpTimer += Time.fixedDeltaTime;
                float t = jumpTimer / jumpDuration; // 0 a 1+

                if (t >= 1f)
                {
                    // Aterrizó — se acabó el salto
                    isJumping = false;
                    jumpTimer = 0f;
                    yOffset = 0f;
                }
                else
                {
                    // Parábola: 4h * t * (1 - t)
                    // En t=0: y=0, en t=0.5: y=jumpHeight, en t=1: y=0
                    yOffset = 4f * jumpHeight * t * (1f - t);
                }
            }

            // ── Zona de amenaza: decidir si salta ────────────────────
            if (canJump && playerTransform != null && !isJumping)
            {
                float distanceToPlayer = transform.position.x - playerTransform.position.x;

                if (distanceToPlayer > 0 && distanceToPlayer <= threatDistance)
                {
                    if (!hasDecidedJump)
                    {
                        hasDecidedJump = true;
                        willJump = Random.value < jumpProbability;
                    }

                    if (willJump)
                    {
                        isJumping = true;
                        jumpTimer = 0f;
                        willJump = false;
                    }
                }
            }

            // ── Movimiento horizontal ────────────────────────────────
            float worldSpeed = 0f;
            if (_speedManager != null && _speedManager.isPlaying)
                worldSpeed = _speedManager.CurrentSpeed;

            float dx = -(worldSpeed + moveSpeed) * Time.fixedDeltaTime;

            // ── Mover con MovePosition (evita jitter) ────────────────
            // Usamos rb.MovePosition para que el Rigidbody2D maneje
            // todo el movimiento de forma suave, sin conflictos.
            Vector2 newPos = new Vector2(
                rb.position.x + dx,
                groundY + yOffset
            );
            rb.linearVelocity = Vector2.zero; // Sin velocidad propia
            rb.MovePosition(newPos);
        }

        /// <summary>
        /// Trigger con el jugador = muerte instantánea.
        /// Usamos Trigger porque ambos (jugador y slime) usan MovePosition,
        /// y las colisiones físicas no se detectan bien entre ellos.
        /// 
        /// IMPORTANTE: En el Inspector del Slime, marca "Is Trigger" en el Collider2D.
        /// </summary>
        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player.health.currentHP > 1)
                {
                    // Usa el escudo
                    player.health.Decrement();
                    
                    if (player.audioSource && player.ouchAudio)
                        player.audioSource.PlayOneShot(player.ouchAudio);
                        
                    // Destruir el slime para no seguir colisionando
                    gameObject.SetActive(false);
                }
                else
                {
                    Schedule<PlayerDeath>();
                }
            }
        }

        /// <summary>
        /// Resetea el estado del slime para reciclaje (llamado por EnemySpawner).
        /// </summary>
        public void ResetSlime(bool shouldJump)
        {
            canJump = shouldJump;
            hasDecidedJump = false;
            willJump = false;
            isJumping = false;
            jumpTimer = 0f;

            if (_collider != null)
                _collider.enabled = true;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Establece la posición Y del suelo (llamado por EnemySpawner al spawnear).
        /// </summary>
        public void SetGroundY(float y)
        {
            groundY = y;
        }
    }
}
