using UnityEngine;
using Platformer.Gameplay;
using Platformer.Model;
using Platformer.Core;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Controlador independiente para enemigos tipo Slime en el Endless Runner.
    /// 
    /// Usa un Rigidbody2D con gravityScale = 0 para movimiento horizontal estable.
    /// Solo activa la gravedad cuando salta, y la desactiva al aterrizar.
    /// 
    /// IMPORTANTE: El prefab del Slime necesita:
    /// - Rigidbody2D (Dynamic, Freeze Rotation Z)
    /// - Collider2D (BoxCollider2D o similar)
    /// - Animator + SpriteRenderer (para animaciones)
    /// - NO necesita AnimationController, KinematicObject ni WorldMover
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class SlimeController : MonoBehaviour
    {
        [Header("Movimiento")]
        [Tooltip("Velocidad propia del slime en unidades/segundo")]
        public float moveSpeed = 2f;

        [Header("Salto")]
        [Tooltip("¿Este slime puede saltar? Se asigna desde el EnemySpawner.")]
        public bool canJump = false;

        [Tooltip("Fuerza del salto (velocidad vertical inicial)")]
        public float jumpForce = 10f;

        [Tooltip("Gravedad durante el salto (se desactiva al aterrizar)")]
        public float jumpGravityScale = 3f;

        [Header("Zona de Amenaza")]
        [Tooltip("Distancia X al jugador donde el slime decide si salta")]
        public float threatDistance = 8f;

        [Tooltip("Probabilidad de saltar en la zona de amenaza (0-1)")]
        [Range(0f, 1f)]
        public float jumpProbabilityInThreatZone = 0.5f;

        [Header("Detección de Suelo")]
        [Tooltip("Distancia del raycast para detectar el suelo")]
        public float groundCheckDistance = 0.3f;

        // ── Componentes ──────────────────────────────────────────────
        private Rigidbody2D rb;
        private Collider2D _collider;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private GameSpeedManager _speedManager;

        // ── Estado ───────────────────────────────────────────────────
        private bool isGrounded;
        private bool isJumping = false;
        private bool hasDecidedJump = false;
        private bool willJump = false;
        private Transform playerTransform;

        void Awake()
        {
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
            playerTransform = null;
            _speedManager = GameSpeedManager.Instance;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
                rb.gravityScale = 0f; // Sin gravedad: se mueve en línea recta
                rb.linearVelocity = Vector2.zero;
            }
        }

        void Update()
        {
            if (_speedManager == null)
                _speedManager = GameSpeedManager.Instance;

            // ── Buscar al jugador ───────────────────────────────────
            if (playerTransform == null)
            {
                PlatformerModel model = Simulation.GetModel<PlatformerModel>();
                if (model != null && model.player != null)
                    playerTransform = model.player.transform;
            }

            // ── Detección de suelo ──────────────────────────────────
            isGrounded = CheckGrounded();

            // Si estaba saltando y aterrizó, desactivar gravedad
            if (isJumping && isGrounded)
            {
                isJumping = false;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }

            // ── Sistema de salto por zona de amenaza ─────────────────
            if (canJump && playerTransform != null && isGrounded && !isJumping)
            {
                float distanceToPlayer = transform.position.x - playerTransform.position.x;

                if (distanceToPlayer > 0 && distanceToPlayer <= threatDistance)
                {
                    if (!hasDecidedJump)
                    {
                        hasDecidedJump = true;
                        willJump = Random.value < jumpProbabilityInThreatZone;
                    }

                    if (willJump)
                    {
                        // ¡SALTAR! Activar gravedad y aplicar fuerza vertical
                        isJumping = true;
                        rb.gravityScale = jumpGravityScale;
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                        willJump = false;
                    }
                }
            }

            // ── Actualizar animaciones ───────────────────────────────
            if (animator != null)
            {
                animator.SetBool("grounded", isGrounded && !isJumping);
                animator.SetFloat("velocityX", Mathf.Abs(rb.linearVelocity.x) / 7f);
            }

            if (spriteRenderer != null && rb.linearVelocity.x < -0.01f)
                spriteRenderer.flipX = true;
        }

        void FixedUpdate()
        {
            // ── Movimiento horizontal ────────────────────────────────
            float worldSpeed = 0f;
            if (_speedManager != null && _speedManager.isPlaying)
                worldSpeed = _speedManager.CurrentSpeed;

            float totalHorizontalSpeed = -(worldSpeed + moveSpeed);

            // Preservar velocidad Y (importante durante saltos)
            rb.linearVelocity = new Vector2(totalHorizontalSpeed, rb.linearVelocity.y);
        }

        /// <summary>
        /// Detecta si el slime está en el suelo usando un raycast corto.
        /// </summary>
        private bool CheckGrounded()
        {
            if (_collider == null) return false;

            Vector2 origin = new Vector2(
                _collider.bounds.center.x,
                _collider.bounds.min.y
            );
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance);
            return hit.collider != null && hit.collider.gameObject != gameObject;
        }

        /// <summary>
        /// Colisión con el jugador = muerte instantánea.
        /// </summary>
        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                Schedule<PlayerDeath>();
            }
        }

        /// <summary>
        /// Resetea el estado del slime para reciclaje.
        /// </summary>
        public void ResetSlime(bool shouldJump)
        {
            canJump = shouldJump;
            hasDecidedJump = false;
            willJump = false;
            isJumping = false;

            if (_collider != null)
                _collider.enabled = true;

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
