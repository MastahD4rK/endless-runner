using System.Collections.Generic;
using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Genera enemigos tipo Slime desde la derecha de la pantalla.
    /// Usa Object Pooling y dificultad progresiva (igual que ObstacleSpawner).
    /// 
    /// Configuración:
    /// 1. Crear un GameObject vacío en la escena y agregarle este script.
    /// 2. Asignar el prefab del Slime en el Inspector (debe tener SlimeController + Rigidbody2D Dynamic).
    /// 3. Ajustar los parámetros de spawn y dificultad.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Configuración de Prefabs")]
        [Tooltip("Prefabs de enemigos. Cada uno DEBE tener SlimeController y Rigidbody2D (Dynamic). NO usar WorldMover ni AnimationController.")]
        public GameObject[] enemyPrefabs;

        [Header("Configuración de Spawn")]
        [Tooltip("Posición X donde aparecen los enemigos (fuera de pantalla a la derecha)")]
        public float spawnX = 15f;

        [Tooltip("Altura Y donde aparecen los enemigos (nivel del suelo)")]
        public float spawnY = -1f;

        [Tooltip("Tiempo mínimo entre la aparición de enemigos (segundos)")]
        public float minSpawnInterval = 3f;

        [Tooltip("Tiempo máximo entre la aparición de enemigos (segundos)")]
        public float maxSpawnInterval = 7f;

        [Tooltip("Posición X donde se reciclan los enemigos (fuera de pantalla izquierda)")]
        public float despawnX = -15f;

        [Header("Comportamiento de Salto")]
        [Tooltip("Probabilidad (0-1) de que un slime sea del tipo saltarín")]
        [Range(0f, 1f)]
        public float jumpChance = 0.35f;

        [Header("Dificultad Progresiva")]
        [Tooltip("Cada cuántos segundos se reduce el intervalo entre spawns")]
        public float difficultyIncreaseInterval = 30f;

        [Tooltip("Cuánto se reduce el intervalo por cada ciclo de dificultad")]
        public float intervalReduction = 0.15f;

        [Tooltip("Intervalo mínimo absoluto (nunca bajará de este valor)")]
        public float absoluteMinInterval = 1.5f;

        [Header("Object Pool")]
        [Tooltip("Cuántas instancias pre-crear por cada prefab de enemigo")]
        public int poolSizePerPrefab = 3;

        // ── Pool y estado ────────────────────────────────────────────
        private Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();
        private Dictionary<GameObject, int> enemyTypeMap = new Dictionary<GameObject, int>();
        private List<GameObject> activeEnemies = new List<GameObject>();

        private float nextSpawnTime;
        private float timeSinceStart;
        private GameSpeedManager _speedManager;

        // ─────────────────────────────────────────────────────────────
        void Start()
        {
            _speedManager = GameSpeedManager.Instance;

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[EnemySpawner] No hay prefabs de enemigos asignados.");
                enabled = false;
                return;
            }

            // Inicializar los pools
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                pools[i] = new Queue<GameObject>();
                for (int j = 0; j < poolSizePerPrefab; j++)
                {
                    GameObject obj = Instantiate(enemyPrefabs[i], Vector3.zero, Quaternion.identity, this.transform);
                    EnsureComponents(obj);
                    obj.SetActive(false);
                    pools[i].Enqueue(obj);
                }
            }

            // Dar un poco de gracia al inicio antes del primer spawn
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

            // ── ¿Es hora de generar un enemigo? ─────────────────
            if (Time.time >= nextSpawnTime)
            {
                SpawnEnemy();

                // Dificultad progresiva
                float difficultyFactor = (timeSinceStart / difficultyIncreaseInterval) * intervalReduction;
                float currentMinInterval = Mathf.Max(absoluteMinInterval, minSpawnInterval - difficultyFactor);
                float currentMaxInterval = Mathf.Max(currentMinInterval + 0.5f, maxSpawnInterval - difficultyFactor);

                nextSpawnTime = Time.time + Random.Range(currentMinInterval, currentMaxInterval);
            }

            // ── Reciclar enemigos que salieron de pantalla ───────
            RecycleOffScreenEnemies();
        }

        // ─────────────────────────────────────────────────────────────
        void SpawnEnemy()
        {
            int prefabIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemy = GetFromPool(prefabIndex);

            // Posicionar fuera de pantalla a la derecha
            enemy.transform.position = new Vector3(spawnX, spawnY, 0f);

            // Decidir aleatoriamente si este slime salta o no
            bool shouldJump = Random.value < jumpChance;

            // Configurar el slime antes de activarlo
            SlimeController slime = enemy.GetComponent<SlimeController>();
            if (slime != null)
            {
                slime.ResetSlime(shouldJump);
            }

            enemy.SetActive(true);
            activeEnemies.Add(enemy);
            Debug.Log($"[EnemySpawner] Slime spawneado en ({spawnX}, {spawnY}). canJump={shouldJump}. Activos: {activeEnemies.Count}");
        }

        void RecycleOffScreenEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                if (activeEnemies[i].transform.position.x < despawnX)
                {
                    ReturnToPool(activeEnemies[i]);
                    activeEnemies.RemoveAt(i);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        #region Object Pool

        GameObject GetFromPool(int prefabIndex)
        {
            if (pools[prefabIndex].Count > 0)
            {
                GameObject obj = pools[prefabIndex].Dequeue();
                enemyTypeMap[obj] = prefabIndex;
                return obj;
            }

            // Fallback: crear uno nuevo si el pool está vacío
            GameObject newObj = Instantiate(enemyPrefabs[prefabIndex], Vector3.zero, Quaternion.identity, this.transform);
            EnsureComponents(newObj);
            enemyTypeMap[newObj] = prefabIndex;
            return newObj;
        }

        void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            if (enemyTypeMap.TryGetValue(obj, out int prefabIndex))
            {
                pools[prefabIndex].Enqueue(obj);
            }
        }

        /// <summary>
        /// Limpia componentes que conflictuarían con el SlimeController.
        /// El SlimeController maneja su propio movimiento (no necesita WorldMover)
        /// y usa Rigidbody2D Dynamic (incompatible con AnimationController/KinematicObject).
        /// </summary>
        void EnsureComponents(GameObject obj)
        {
            // Eliminar WorldMover si existe — el SlimeController ya maneja el movimiento del mundo
            var worldMover = obj.GetComponent<WorldMover>();
            if (worldMover != null)
            {
                Destroy(worldMover);
            }

            // Eliminar AnimationController si existe — fuerza Kinematic y rompe el salto
            var animCtrl = obj.GetComponent<AnimationController>();
            if (animCtrl != null)
            {
                Debug.LogWarning($"[EnemySpawner] Se eliminó AnimationController de '{obj.name}'. Usa solo SlimeController.");
                Destroy(animCtrl);
            }
        }

        #endregion
    }
}
