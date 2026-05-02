using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Core
{
    public enum SkillType
    {
        DoubleJump,
        Shield,
        Multiplier
    }

    [Serializable]
    public class SkillData
    {
        public SkillType type;
        public int currentLevel;
        public int maxLevel;
        public int[] costPerLevel;
        public string title;
        public string description;
    }

    /// <summary>
    /// Singleton persistente que gestiona los niveles de las habilidades.
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        private static SkillManager _instance;
        public static SkillManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SkillManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[SkillManager]");
                        _instance = go.AddComponent<SkillManager>();
                    }
                }
                return _instance;
            }
        }

        public Dictionary<SkillType, SkillData> Skills { get; private set; }

        void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSkills();
                LoadSkills();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSkills()
        {
            Skills = new Dictionary<SkillType, SkillData>();

            Skills.Add(SkillType.DoubleJump, new SkillData
            {
                type = SkillType.DoubleJump,
                maxLevel = 1,
                costPerLevel = new int[] { 50 },
                title = "Doble Salto",
                description = "Permite realizar un segundo salto en el aire."
            });

            Skills.Add(SkillType.Shield, new SkillData
            {
                type = SkillType.Shield,
                maxLevel = 3,
                costPerLevel = new int[] { 100, 200, 300 },
                title = "Escudo Vital",
                description = "Otorga vidas extra al iniciar la partida."
            });

            Skills.Add(SkillType.Multiplier, new SkillData
            {
                type = SkillType.Multiplier,
                maxLevel = 3,
                costPerLevel = new int[] { 75, 150, 250 },
                title = "Multiplicador",
                description = "Multiplica las monedas y puntos obtenidos."
            });
        }

        public int GetSkillLevel(SkillType type)
        {
            if (Skills.TryGetValue(type, out var data))
            {
                return data.currentLevel;
            }
            return 0;
        }

        public bool CanUpgrade(SkillType type)
        {
            if (Skills.TryGetValue(type, out var data))
            {
                if (data.currentLevel >= data.maxLevel) return false;
                int cost = data.costPerLevel[data.currentLevel];
                return CurrencyManager.Instance.TotalCoins >= cost;
            }
            return false;
        }

        public bool TryUpgradeSkill(SkillType type)
        {
            if (Skills.TryGetValue(type, out var data))
            {
                if (data.currentLevel >= data.maxLevel) return false;
                
                int cost = data.costPerLevel[data.currentLevel];
                if (CurrencyManager.Instance.SpendCoins(cost))
                {
                    data.currentLevel++;
                    SaveSkills();
                    return true;
                }
            }
            return false;
        }

        private void SaveSkills()
        {
            foreach (var kvp in Skills)
            {
                PlayerPrefs.SetInt($"Skill_{kvp.Key}_Level", kvp.Value.currentLevel);
            }
            PlayerPrefs.Save();
        }

        private void LoadSkills()
        {
            foreach (var kvp in Skills)
            {
                kvp.Value.currentLevel = PlayerPrefs.GetInt($"Skill_{kvp.Key}_Level", 0);
            }
        }

        [ContextMenu("Reset Skills (Debug)")]
        public void ResetSkills()
        {
            foreach (var kvp in Skills)
            {
                kvp.Value.currentLevel = 0;
                PlayerPrefs.SetInt($"Skill_{kvp.Key}_Level", 0);
            }
            PlayerPrefs.Save();
            Debug.Log("[SkillManager] Skills reset.");
        }
    }
}
