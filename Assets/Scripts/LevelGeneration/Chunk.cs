using System.Collections.Generic;
using System.Linq;
using Enemy;
using UnityEngine;

namespace LevelGeneration
{
    public class Chunk : MonoBehaviour, IPoolerObject
    {
        [System.Serializable]
        public struct DynamicObjectChance
        {
            public int chance;
            public GameObject alternative;
        }
        [System.Serializable]
        public struct DynamicObject
        {
            public List<DynamicObjectChance> variations;
            public Dictionary<GameObject, int> VariationsToDictionary()
            {
                return this.variations.ToDictionary(obj => obj.alternative, obj => obj.chance);
            }
        }
    
        public List<DynamicObject> dynamicObjects;
        public List<EnemyBehaviourScript> enemies;
    
        private void Awake()
        {
            this.SetObjectVariations();
        }
    
        /// <summary>
        /// Sets Active variations for all chunk objects set in the unity editor. 
        /// </summary>
        private void SetObjectVariations()
        {
            this.dynamicObjects.ForEach(e => WeightedRandomizer.From(e.VariationsToDictionary()).TakeOne().SetActive(true));
        }

        public void OnObjectSpawn()
        {
            foreach (var enemy in enemies)
            {
                if (!EnemySpawner.Instance.CheckChance())
                {
                    enemy.gameObject.SetActive(false);
                }   
            }
        }
    }
}
