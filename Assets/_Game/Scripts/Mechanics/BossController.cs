using UnityEngine;
using TMPro;
using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    public class BossController : MonoBehaviour
    {
        public static BossController Instance { get; private set; }

        [Header("Configuración de Jefe")]
        public int maxHealth = 3;
        public float moveSpeed = 2f;
        public float amplitude = 1.2f; // Altura de oscilación reducida para que se quede al centro
        public float fireRate = 1.5f;
        
        [Header("Referencias")]
        public GameObject projectilePrefab;
        
        public bool IsActive { get; private set; } = false;
        
        private int _currentHealth;
        private float _startY;
        private float _timeSinceLastFire;
        private SpriteRenderer _spriteRenderer;
        private TextMeshPro _hpText;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            _currentHealth = maxHealth;
            _startY = transform.position.y;
            IsActive = true;
            
            // Crear texto de vida sobre el jefe
            GameObject textObj = new GameObject("BossHPText");
            textObj.transform.SetParent(this.transform);
            textObj.transform.localPosition = new Vector3(0, 1.5f, 0); // Un poco arriba
            _hpText = textObj.AddComponent<TextMeshPro>();
            _hpText.text = "BOSS HP: " + _currentHealth;
            _hpText.fontSize = 4;
            _hpText.alignment = TextAlignmentOptions.Center;
            _hpText.color = Color.white;
            _hpText.sortingOrder = 50; // Para que se vea por encima

            // Pausar score y spawners mientras el jefe está activo
            if (GameSpeedManager.Instance != null)
            {
                GameSpeedManager.Instance.isBossPhase = true;
            }
        }

        void Update()
        {
            if (!IsActive) return;
            if (GameSpeedManager.Instance != null && !GameSpeedManager.Instance.isPlaying) return;

            // Movimiento sinusoidal de arriba a abajo
            float newY = _startY + Mathf.Sin(Time.time * moveSpeed) * amplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Disparo
            _timeSinceLastFire += Time.deltaTime;
            if (_timeSinceLastFire >= fireRate)
            {
                FireProjectile();
                _timeSinceLastFire = 0f;
            }
        }

        void FireProjectile()
        {
            if (projectilePrefab != null)
            {
                // Disparar desde el centro izquierda del jefe
                Vector3 spawnPos = transform.position + Vector3.left * 1.5f;
                GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                proj.SetActive(true);
                var projScript = proj.GetComponent<BossProjectile>();
                if (projScript != null)
                {
                    projScript.Initialize(this);
                }
            }
        }

        public void TakeDamage()
        {
            _currentHealth--;
            if (_hpText != null) _hpText.text = "BOSS HP: " + _currentHealth;

            // Feedback visual de daño (Parpadeo rápido)
            if (_spriteRenderer != null)
            {
                StartCoroutine(FlashRed());
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private System.Collections.IEnumerator FlashRed()
        {
            Color original = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = original;
        }

        void Die()
        {
            IsActive = false;
            
            // Reanudar spawners
            if (GameSpeedManager.Instance != null)
            {
                GameSpeedManager.Instance.isBossPhase = false;
            }

            // Avisar al MapManager que puede hacer el crossfade
            if (MapManager.Instance != null)
            {
                MapManager.Instance.OnBossDefeated();
            }

            // Destruir el jefe (luego podemos añadir una explosión)
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
