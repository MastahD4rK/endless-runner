using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Genera obstáculos aleatorios que aparecen desde la derecha de la pantalla
    /// y se mueven hacia la izquierda usando el WorldMover.
    /// Incluye Object Pooling para rendimiento óptimo.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Configuración de Prefabs")]
        [Tooltip("Lista de prefabs de obstáculos. Cada uno DEBE tener un Collider2D y el script Obstacle.")]
        public GameObject[] obstaclePrefabs;

        [Header("Configuración de Spawn")]
        [Tooltip("Posición X donde aparecen los obstáculos (fuera de la pantalla a la derecha)")]
        public float spawnX = 15f;
        
        [Tooltip("Altura Y base donde aparecen los obstáculos (debe coincidir con el nivel del suelo)")]
        public float spawnY = -1.5f;
        
        [Tooltip("Tiempo mínimo entre la aparición de obstáculos (en segundos)")]
        public float minSpawnInterval = 1.5f;
        
        [Tooltip("Tiempo máximo entre la aparición de obstáculos (en segundos)")]
        public float maxSpawnInterval = 4f;
        
        [Tooltip("Posición X donde se reciclan los obstáculos (fuera de pantalla por la izquierda)")]
        public float despawnX = -15f;

        [Header("Dificultad Progresiva")]
        [Tooltip("Cada cuántos segundos se reduce el intervalo de spawn")]
        public float difficultyIncreaseInterval = 30f;
        
        [Tooltip("Cuánto se reduce el intervalo mínimo en cada aumento de dificultad")]
        public float intervalReduction = 0.1f;
        
        [Tooltip("Intervalo mínimo absoluto (nunca bajará de este valor)")]
        public float absoluteMinInterval = 0.8f;

        [Header("Object Pool")]
        public int poolSizePerPrefab = 3;

        // Pool de obstáculos
        private Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();
        private Dictionary<GameObject, int> obstacleTypeMap = new Dictionary<GameObject, int>();
        
        // Lista de obstáculos activos en pantalla
        private List<GameObject> activeObstacles = new List<GameObject>();

        private float nextSpawnTime;
        private float timeSinceStart;
        private GameSpeedManager _speedManager;

        void Start()
        {
            _speedManager = GameSpeedManager.Instance;

            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            {
                Debug.LogWarning("[ObstacleSpawner] No hay prefabs de obstáculos asignados.");
                enabled = false;
                return;
            }

            // Inicializar pools
            for (int i = 0; i < obstaclePrefabs.Length; i++)
            {
                pools[i] = new Queue<GameObject>();
                for (int j = 0; j < poolSizePerPrefab; j++)
                {
                    GameObject obj = Instantiate(obstaclePrefabs[i], Vector3.zero, Quaternion.identity, this.transform);
                    EnsureComponents(obj);
                    obj.SetActive(false);
                    pools[i].Enqueue(obj);
                }
            }

            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        void Update()
        {
            if (_speedManager == null)
            {
                _speedManager = GameSpeedManager.Instance;
                if (_speedManager == null) return;
            }

            if (!_speedManager.isPlaying) return;

            timeSinceStart += Time.deltaTime;

            // ¿Es hora de spawnear?
            if (Time.time >= nextSpawnTime)
            {
                SpawnObstacle();
                
                // Dificultad progresiva: reducir intervalo con el tiempo
                float currentMinInterval = Mathf.Max(
                    absoluteMinInterval, 
                    minSpawnInterval - (timeSinceStart / difficultyIncreaseInterval) * intervalReduction
                );
                float currentMaxInterval = Mathf.Max(
                    currentMinInterval + 0.5f, 
                    maxSpawnInterval - (timeSinceStart / difficultyIncreaseInterval) * intervalReduction
                );
                
                nextSpawnTime = Time.time + Random.Range(currentMinInterval, currentMaxInterval);
            }

            // Reciclar obstáculos que salieron de pantalla
            RecycleOffScreenObstacles();
        }

        void SpawnObstacle()
        {
            int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obstacle = GetFromPool(prefabIndex);

            // Posicionar justo fuera de la pantalla a la derecha, a la altura del suelo
            obstacle.transform.position = new Vector3(spawnX, spawnY, 0f);
            obstacle.SetActive(true);

            activeObstacles.Add(obstacle);
        }

        void RecycleOffScreenObstacles()
        {
            for (int i = activeObstacles.Count - 1; i >= 0; i--)
            {
                if (activeObstacles[i] == null)
                {
                    activeObstacles.RemoveAt(i);
                    continue;
                }

                if (activeObstacles[i].transform.position.x < despawnX)
                {
                    ReturnToPool(activeObstacles[i]);
                    activeObstacles.RemoveAt(i);
                }
            }
        }

        GameObject GetFromPool(int prefabIndex)
        {
            if (pools[prefabIndex].Count > 0)
            {
                GameObject obj = pools[prefabIndex].Dequeue();
                obstacleTypeMap[obj] = prefabIndex;
                return obj;
            }

            // Fallback: crear uno nuevo
            GameObject newObj = Instantiate(obstaclePrefabs[prefabIndex], Vector3.zero, Quaternion.identity, this.transform);
            EnsureComponents(newObj);
            obstacleTypeMap[newObj] = prefabIndex;
            return newObj;
        }

        void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            if (obstacleTypeMap.TryGetValue(obj, out int prefabIndex))
            {
                pools[prefabIndex].Enqueue(obj);
            }
        }

        /// <summary>
        /// Garantiza que el obstáculo se mueva con el mundo y tenga el componente necesario.
        /// </summary>
        void EnsureComponents(GameObject obj)
        {
            if (obj.GetComponent<WorldMover>() == null)
                obj.AddComponent<WorldMover>();
        }
    }
}
