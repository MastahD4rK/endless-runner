using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        bool _doubleJumpUsed;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            // Buscamos en los hijos por si el usuario metió un prefab (como la Brujita)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                // Endless Runner: El jugador está fijo en X.
                move.x = 0;
                
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (jumpState == JumpState.InFlight && m_JumpAction.WasPressedThisFrame() && SkillManager.Instance.GetSkillLevel(SkillType.DoubleJump) > 0 && !_doubleJumpUsed)
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            // Si el jugador está muerto, congelar toda la física y no tocar el Animator.
            // Esto evita que el "truco de caída" (Play Player-Land) interfiera con la
            // animación de muerte y cree un bucle visual de caída infinita.
            if (health != null && !health.IsAlive)
            {
                velocity = Vector2.zero;
                targetVelocity = Vector2.zero;
                return;
            }

            if (IsGrounded)
            {
                _doubleJumpUsed = false;
            }

            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (jump && !IsGrounded && SkillManager.Instance.GetSkillLevel(SkillType.DoubleJump) > 0 && !_doubleJumpUsed)
            {
                // Execute double jump
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
                _doubleJumpUsed = true; // Mark as used
                
                // Play jump audio again
                if (audioSource != null && jumpAudio != null)
                    audioSource.PlayOneShot(jumpAudio);
                
                // Note: The animator uses 'velocityY' and 'grounded' for transitions, 
                // so we don't need an explicit 'jump' trigger here.
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            // Endless runner: always facing right
            if (spriteRenderer != null)
                spriteRenderer.flipX = false;

            if (animator != null)
            {
                animator.SetBool("grounded", IsGrounded);
                // Velocidad horizontal fija (el mundo se mueve, no el jugador)
                animator.SetFloat("velocityX", 1f);
                // Velocidad vertical: positiva = subiendo, negativa = cayendo
                animator.SetFloat("velocityY", velocity.y);

                // TRUCO: El Animator de la plantilla original no tiene estado de "Caída".
                // Pero el creador del Raptor.overrideController mapeó la animación de caída
                // (raptor-caida) al estado de aterrizaje ("Player-Land").
                // Forzamos ese estado mientras estamos cayendo en el aire:
                if (!IsGrounded && velocity.y < 0)
                {
                    animator.Play("Player-Land", 0, 0f);
                }
            }

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}