using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Gestiona centralizadamente el movimiento de todos los objetos "WorldMover".
    /// En lugar de que docenas de objetos ejecuten FixedUpdate individualmente,
    /// se registran aquí y este manager los desplaza de una sola vez.
    /// </summary>
    public class WorldMoverManager : MonoBehaviour
    {
        private static WorldMoverManager _instance;
        public static WorldMoverManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<WorldMoverManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[WorldMoverManager]");
                        _instance = go.AddComponent<WorldMoverManager>();
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        // Usamos una lista para iteración rápida
        private List<WorldMover> _movers = new List<WorldMover>(100);
        
        private GameSpeedManager _speedManager;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            _speedManager = GameSpeedManager.Instance;
        }

        void FixedUpdate()
        {
            if (_speedManager == null)
            {
                _speedManager = GameSpeedManager.Instance;
                if (_speedManager == null) return;
            }

            if (!_speedManager.isPlaying) return;

            float moveAmount = _speedManager.CurrentSpeed * Time.fixedDeltaTime;
            Vector3 movement = Vector3.left * moveAmount;

            for (int i = 0; i < _movers.Count; i++)
            {
                if (_movers[i] != null)
                {
                    _movers[i].transform.position += movement;
                }
            }
        }

        /// <summary>
        /// Registra un WorldMover para que se actualice.
        /// </summary>
        public void Register(WorldMover mover)
        {
            if (!_movers.Contains(mover))
            {
                _movers.Add(mover);
            }
        }

        /// <summary>
        /// Elimina un WorldMover usando swap-remove para rendimiento O(1).
        /// </summary>
        public void Unregister(WorldMover mover)
        {
            int index = _movers.IndexOf(mover);
            if (index >= 0)
            {
                int last = _movers.Count - 1;
                _movers[index] = _movers[last];
                _movers.RemoveAt(last);
            }
        }
        
        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
