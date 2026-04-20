using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Generador de monedas independiente del suelo.
    /// Usa Object Pooling para optimizar rendimiento.
    /// </summary>
    public class CoinSpawner : MonoBehaviour
    {
        [Header("Configuración de Prefabs")]
        [Tooltip("Prefab de la moneda. Debe tener Collider2D(Trigger) y ScoreCoin.")]
        public GameObject coinPrefab;

        [Header("Configuración de Spawn")]
        [Tooltip("Posición X donde aparecen las monedas (fuera de pantalla a la derecha)")]
        public float spawnX = 15f;
        
        [Tooltip("Rango de altura Y donde pueden aparecer las monedas")]
        public float minY = 0f;
        public float maxY = 3.5f;
        
        [Tooltip("Tiempo mínimo entre spawns (segundos)")]
        public float minSpawnInterval = 0.5f;
        [Tooltip("Tiempo máximo entre spawns (segundos)")]
        public float maxSpawnInterval = 2f;
        
        [Tooltip("Posición X donde se destruyen/reciclan (fuera de pantalla a la izquierda)")]
        public float despawnX = -15f;

        [Header("Patrones (Opcional)")]
        [Tooltip("Cantidad máxima de monedas en línea (patrón horizontal)")]
        public int maxCoinsInRow = 3;
        [Tooltip("Espacio horizontal entre monedas en fila")]
        public float spacingX = 1.5f;

        [Header("Object Pool")]
        public int poolSize = 10;

        private Queue<GameObject> _pool = new Queue<GameObject>();
        private List<GameObject> _activeCoins = new List<GameObject>();
        private float _nextSpawnTime;
        private GameSpeedManager _speedManager;

        void Start()
        {
            _speedManager = GameSpeedManager.Instance;

            if (coinPrefab == null)
            {
                Debug.LogWarning("[CoinSpawner] No hay prefab de moneda asignado.");
                enabled = false;
                return;
            }

            // Inicializar pool
            for (int i = 0; i < poolSize; i++)
            {
                CreateCoinForPool();
            }

            _nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        void Update()
        {
            if (_speedManager == null)
            {
                _speedManager = GameSpeedManager.Instance;
                if (_speedManager == null) return;
            }

            if (!_speedManager.isPlaying) return;

            // Revisar si es hora de spawnear
            if (Time.time >= _nextSpawnTime)
            {
                SpawnCoinPattern();
                _nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
            }

            // Reciclar monedas fuera de pantalla
            RecycleOffScreenCoins();
        }

        void SpawnCoinPattern()
        {
            int count = Random.Range(1, maxCoinsInRow + 1);
            float startY = Random.Range(minY, maxY);
            
            // Decidir patrón sutil (línea recta o pequeña variación de altura)
            float patternOffsetY = Random.Range(-0.2f, 0.2f);

            for (int i = 0; i < count; i++)
            {
                GameObject coin = GetFromPool();
                // Calcular posición de cada moneda del patrón en X y una ligera curva en Y
                float spawnPosX = spawnX + (i * spacingX);
                float spawnPosY = startY + (i * patternOffsetY);
                
                coin.transform.position = new Vector3(spawnPosX, spawnPosY, 0f);
                coin.SetActive(true);
                
                _activeCoins.Add(coin);
            }
        }

        void RecycleOffScreenCoins()
        {
            // Iteración inversa con swap-remove
            for (int i = _activeCoins.Count - 1; i >= 0; i--)
            {
                GameObject coin = _activeCoins[i];
                if (coin == null || !coin.activeSelf) // También recicla si el jugador la agarró
                {
                    if (coin != null) ReturnToPool(coin);
                    
                    int last = _activeCoins.Count - 1;
                    _activeCoins[i] = _activeCoins[last];
                    _activeCoins.RemoveAt(last);
                    continue;
                }

                if (coin.transform.position.x < despawnX)
                {
                    ReturnToPool(coin);
                    int last = _activeCoins.Count - 1;
                    _activeCoins[i] = _activeCoins[last];
                    _activeCoins.RemoveAt(last);
                }
            }
        }

        void CreateCoinForPool()
        {
            GameObject obj = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity, this.transform);
            if (obj.GetComponent<WorldMover>() == null)
                obj.AddComponent<WorldMover>();
            
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }

        GameObject GetFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            // Fallback
            GameObject obj = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity, this.transform);
            if (obj.GetComponent<WorldMover>() == null)
                obj.AddComponent<WorldMover>();
            return obj;
        }

        void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
