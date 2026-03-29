using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Genera un suelo constante que simula un mundo infinito.
    /// Reutiliza bloques y los elimina cuando salen de la pantalla.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Configuración de Prefabs")]
        public GameObject[] platformPrefabs;
        
        [Header("Configuración de Spawning")]
        public float blockWidth = 10f; // Ancho exacto de cada bloque/prefab
        public int initialBlocks = 6;
        public float despawnX = -15f; // Posición X donde se destruye un bloque (borde izquierdo visible)
        
        public Vector3 startPosition = new Vector3(-5f, -2f, 0f);

        private Queue<GameObject> activePlatforms = new Queue<GameObject>();
        private GameObject lastPlatform;

        void Start()
        {
            for (int i = 0; i < initialBlocks; i++)
            {
                // El primer par de bloques debería ser seguro (index 0)
                SpawnBlock(i < 2); 
            }
        }

        void Update()
        {
            if (activePlatforms.Count == 0) return;

            // Si el bloque más antiguo salió de la vista, destruirlo e instanciar uno nuevo
            GameObject firstPlatform = activePlatforms.Peek();
            if (firstPlatform.transform.position.x < despawnX)
            {
                activePlatforms.Dequeue();
                Destroy(firstPlatform);
                SpawnBlock(false);
            }
        }

        void SpawnBlock(bool isSafeZone)
        {
            if (platformPrefabs == null || platformPrefabs.Length == 0) return;

            // Elegir un prefab aleatorio. Si es SafeZone, se asume que el indice 0 es suelo normal sin trampas.
            int randomIndex = isSafeZone ? 0 : Random.Range(0, platformPrefabs.Length);
            GameObject prefabToSpawn = platformPrefabs[randomIndex];

            Vector3 spawnPos;
            if (lastPlatform == null)
            {
                spawnPos = startPosition;
            }
            else
            {
                spawnPos = lastPlatform.transform.position + Vector3.right * blockWidth;
            }

            GameObject newBlock = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, this.transform);
            
            // Obligatorio: cada bloque del entorno debe moverse
            if (newBlock.GetComponent<WorldMover>() == null)
            {
                newBlock.AddComponent<WorldMover>();
            }

            activePlatforms.Enqueue(newBlock);
            lastPlatform = newBlock;
        }
    }
}
