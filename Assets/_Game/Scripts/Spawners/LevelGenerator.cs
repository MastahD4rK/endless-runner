using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Genera un suelo constante que simula un mundo infinito.
    /// Usa Object Pooling para reutilizar bloques en vez de Instantiate/Destroy,
    /// evitando micro-stutters por Garbage Collection.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Configuración de Prefabs")]
        public GameObject[] platformPrefabs;
        
        [Header("Configuración de Spawning")]
        public float blockWidth = 10f;
        public int initialBlocks = 6;
        public float despawnX = -15f;
        
        public Vector3 startPosition = new Vector3(-5f, -2f, 0f);

        [Header("Object Pool")]
        [Tooltip("Cuántos bloques extras pre-crear por cada prefab tipo")]
        public int poolSizePerPrefab = 3;

        // Cola de bloques activos en pantalla (en orden de izquierda a derecha)
        private Queue<GameObject> activePlatforms = new Queue<GameObject>();
        
        // Pool de objetos inactivos reutilizables, organizados por prefab index
        private Dictionary<int, Queue<GameObject>> objectPools = new Dictionary<int, Queue<GameObject>>();
        
        // Mapeo de cada bloque activo a su índice de prefab (para saber a qué pool devolverlo)
        private Dictionary<GameObject, int> blockPrefabIndex = new Dictionary<GameObject, int>();
        
        // Posición X donde debe nacer el siguiente bloque
        private float nextSpawnX;

        void Start()
        {
            if (platformPrefabs == null || platformPrefabs.Length == 0)
            {
                Debug.LogError("[LevelGenerator] No hay prefabs asignados en Platform Prefabs.");
                return;
            }

            // Inicializar los pools
            for (int i = 0; i < platformPrefabs.Length; i++)
            {
                objectPools[i] = new Queue<GameObject>();
                for (int j = 0; j < poolSizePerPrefab; j++)
                {
                    GameObject pooled = Instantiate(platformPrefabs[i], Vector3.zero, Quaternion.identity, this.transform);
                    EnsureWorldMover(pooled);
                    pooled.SetActive(false);
                    objectPools[i].Enqueue(pooled);
                }
            }

            // Generar los bloques iniciales en pantalla
            nextSpawnX = startPosition.x;
            for (int i = 0; i < initialBlocks; i++)
            {
                SpawnBlock(i < 2); // Los dos primeros son zona segura
            }
        }

        void Update()
        {
            if (activePlatforms.Count == 0) return;

            // Si el bloque más antiguo salió de la vista, reciclarlo
            GameObject firstPlatform = activePlatforms.Peek();
            
            // Bug fix: Verificar que el objeto no haya sido destruido externamente
            if (firstPlatform == null)
            {
                activePlatforms.Dequeue();
                return;
            }
            
            if (firstPlatform.transform.position.x < despawnX)
            {
                activePlatforms.Dequeue();
                RecycleBlock(firstPlatform);
                SpawnBlock(false);
            }
        }

        void SpawnBlock(bool isSafeZone)
        {
            // Elegir un prefab aleatorio. Si es SafeZone, usar siempre el índice 0
            int prefabIndex = isSafeZone ? 0 : Random.Range(0, platformPrefabs.Length);

            GameObject block = GetFromPool(prefabIndex);

            Vector3 spawnPos = new Vector3(nextSpawnX, startPosition.y, startPosition.z);
            block.transform.position = spawnPos;
            block.SetActive(true);

            activePlatforms.Enqueue(block);
            nextSpawnX += blockWidth;
        }

        /// <summary>
        /// Obtiene un bloque del pool. Si el pool está vacío, crea uno nuevo.
        /// </summary>
        GameObject GetFromPool(int prefabIndex)
        {
            if (objectPools[prefabIndex].Count > 0)
            {
                GameObject pooled = objectPools[prefabIndex].Dequeue();
                blockPrefabIndex[pooled] = prefabIndex;
                return pooled;
            }

            // Pool vacío: crear uno nuevo (fallback)
            GameObject newBlock = Instantiate(platformPrefabs[prefabIndex], Vector3.zero, Quaternion.identity, this.transform);
            EnsureWorldMover(newBlock);
            blockPrefabIndex[newBlock] = prefabIndex;
            return newBlock;
        }

        /// <summary>
        /// Devuelve un bloque al pool para reutilizarlo después.
        /// </summary>
        void RecycleBlock(GameObject block)
        {
            block.SetActive(false);

            if (blockPrefabIndex.TryGetValue(block, out int prefabIndex))
            {
                objectPools[prefabIndex].Enqueue(block);
            }
            else
            {
                // Si por alguna razón no tenemos registro, destruirlo como fallback
                Destroy(block);
            }
        }

        /// <summary>
        /// Garantiza que el bloque tenga un WorldMover adjunto.
        /// </summary>
        void EnsureWorldMover(GameObject block)
        {
            if (block.GetComponent<WorldMover>() == null)
            {
                block.AddComponent<WorldMover>();
            }
        }
    }
}
