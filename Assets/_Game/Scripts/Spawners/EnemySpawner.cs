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
        public float minSpawnInterval = 4f;

        [Tooltip("Tiempo máximo entre la aparición de enemigos (segundos)")]
        public float maxSpawnInterval = 8f;

        [Tooltip("Distancia mínima X entre slimes activos para evitar que se encimen")]
        public float minSeparationDistance = 5f;

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

        public struct PooledEnemy
        {
            public GameObject obj;
            public SlimeController slime;
        }

        // ── Pool y estado ────────────────────────────────────────────
        private Dictionary<int, Queue<PooledEnemy>> pools = new Dictionary<int, Queue<PooledEnemy>>();
        private Dictionary<GameObject, int> enemyTypeMap = new Dictionary<GameObject, int>();
        private List<PooledEnemy> activeEnemies = new List<PooledEnemy>();

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
                pools[i] = new Queue<PooledEnemy>();
                for (int j = 0; j < poolSizePerPrefab; j++)
                {
                    GameObject obj = Instantiate(enemyPrefabs[i], Vector3.zero, Quaternion.identity, this.transform);
                    EnsureComponents(obj);
                    obj.SetActive(false);
                    pools[i].Enqueue(new PooledEnemy { obj = obj, slime = obj.GetComponent<SlimeController>() });
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

            if (!_speedManager.isPlaying || _speedManager.isBossPhase) return;

            timeSinceStart += Time.deltaTime;

            // ── ¿Es hora de generar un enemigo? ─────────────────
            if (Time.time >= nextSpawnTime)
            {
                // Verificar que no haya otro slime demasiado cerca del punto de spawn
                if (!IsEnemyTooCloseToSpawn())
                {
                    SpawnEnemy();
                }

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
        /// <summary>
        /// Verifica si algún enemigo activo está demasiado cerca del punto de spawn.
        /// </summary>
        bool IsEnemyTooCloseToSpawn()
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i].obj == null) continue;
                float dist = Mathf.Abs(activeEnemies[i].obj.transform.position.x - spawnX);
                if (dist < minSeparationDistance)
                    return true;
            }
            return false;
        }

        void SpawnEnemy()
        {
            int prefabIndex = Random.Range(0, enemyPrefabs.Length);
            PooledEnemy enemy = GetFromPool(prefabIndex);

            // Posicionar fuera de pantalla a la derecha
            enemy.obj.transform.position = new Vector3(spawnX, spawnY, 0f);

            // Decidir aleatoriamente si este slime salta o no
            bool shouldJump = Random.value < jumpChance;

            // Configurar el slime antes de activarlo (usando componente cacheado)
            if (enemy.slime != null)
            {
                enemy.slime.SetGroundY(spawnY);
                enemy.slime.ResetSlime(shouldJump);
            }

            enemy.obj.SetActive(true);
            activeEnemies.Add(enemy);
        }

        void RecycleOffScreenEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                PooledEnemy enemy = activeEnemies[i];
                if (enemy.obj == null)
                {
                    int last = activeEnemies.Count - 1;
                    activeEnemies[i] = activeEnemies[last];
                    activeEnemies.RemoveAt(last);
                    continue;
                }

                if (enemy.obj.transform.position.x < despawnX)
                {
                    ReturnToPool(enemy);
                    int last = activeEnemies.Count - 1;
                    activeEnemies[i] = activeEnemies[last];
                    activeEnemies.RemoveAt(last);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        #region Object Pool

        PooledEnemy GetFromPool(int prefabIndex)
        {
            if (pools[prefabIndex].Count > 0)
            {
                PooledEnemy enemy = pools[prefabIndex].Dequeue();
                enemyTypeMap[enemy.obj] = prefabIndex;
                return enemy;
            }

            // Fallback: crear uno nuevo si el pool está vacío
            GameObject newObj = Instantiate(enemyPrefabs[prefabIndex], Vector3.zero, Quaternion.identity, this.transform);
            EnsureComponents(newObj);
            enemyTypeMap[newObj] = prefabIndex;
            return new PooledEnemy { obj = newObj, slime = newObj.GetComponent<SlimeController>() };
        }

        void ReturnToPool(PooledEnemy enemy)
        {
            if (enemy.obj != null)
            {
                enemy.obj.SetActive(false);
                if (enemyTypeMap.TryGetValue(enemy.obj, out int prefabIndex))
                {
                    pools[prefabIndex].Enqueue(enemy);
                }
            }
        }

        /// <summary>
        /// Limpia componentes que conflictuarían con el SlimeController.
        /// El SlimeController maneja su propio movimiento (no necesita WorldMover)
        /// y usa Rigidbody2D Dynamic (incompatible con AnimationController/KinematicObject).
        /// </summary>
        void EnsureComponents(GameObject obj)
        {
            // Eliminar AnimationController (hereda KinematicObject, fuerza Kinematic)
            var animCtrl = obj.GetComponent<AnimationController>();
            if (animCtrl != null)
                DestroyImmediate(animCtrl);

            // Eliminar KinematicObject suelto
            var kinematic = obj.GetComponent<KinematicObject>();
            if (kinematic != null)
                DestroyImmediate(kinematic);

            // Eliminar WorldMover — SlimeController maneja el movimiento
            var worldMover = obj.GetComponent<WorldMover>();
            if (worldMover != null)
                DestroyImmediate(worldMover);
        }

        #endregion
    }
}
