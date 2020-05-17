using System.Collections;
using System.Collections.Generic;
using Game;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelGeneration
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        [HideInInspector] public List<GameObject> activeEnemies;

        public GameObject dronePrefab;
        public float startChance = 15f;
        public float chanceJump = 10f;
        private float _currentChance;
        private float _diffIncrement = 5f;
        private int _spawnedEnemies;
        private int _nextDiffCap = 10;

        public float droneSpawnRate = 20f;
        private int _checkCount = 20;
        private int _currentCount;
        private int _droneSpawnAmount = 2;
        private float _droneSpawnInterval = 3f;
        private bool _currentlySpawning;

        private void Start()
        {
            _currentChance = startChance;
        }

        public bool CheckChance()
        {
            if (Random.Range(0, 100f) <= _currentChance)
            {
                _currentChance = startChance;
                _spawnedEnemies++;
                _currentCount++;
                CheckDiffCap();
                CheckDroneSpawn();
                return true;
            }

            _currentChance += chanceJump;
            return false;
        }

        private void CheckDiffCap()
        {
            if (_spawnedEnemies != _nextDiffCap) return;
            
            _nextDiffCap += (_nextDiffCap / 2);
            _spawnedEnemies = 0;
            IncreaseDifficulty();
        }

        private void IncreaseDifficulty()
        {
            startChance += _diffIncrement;
            _diffIncrement += (_diffIncrement / 2);
            droneSpawnRate += 10f;
            _droneSpawnAmount++;
            _droneSpawnInterval -= (_droneSpawnInterval <= 1f) ? 0f : 0.1f;
            _checkCount -= (_checkCount <= 5) ? 0 : 2;
            if (droneSpawnRate > 100) droneSpawnRate = 100;
        }

        public void RemoveActiveEnemy(GameObject enemy)
        {
            activeEnemies.Remove(enemy);

            if (activeEnemies.Count == 0)
                StartCoroutine(IdleCountdown());

        }

        private void CheckDroneSpawn()
        {
            if (!GameHandler.Instance.inGame) return;
            
            if (_currentCount >= _checkCount)
            {
                if (!_currentlySpawning && Random.Range(0, 100f) <= droneSpawnRate)
                {
                    _currentCount = 0;
                    StartCoroutine(SpawnDrones(_droneSpawnAmount, _droneSpawnInterval));
                }
            }
        }
        
        private IEnumerator SpawnDrones(int amount, float interval)
        {
            _currentlySpawning = true;

            for (int i = 0; i < amount; i++)
            {
                Vector3 rand = Random.insideUnitSphere * 2f;
                var spawnPos = PlayerManager.Instance.transform.position + rand;
                spawnPos.z -= 100f;
                spawnPos.y = 0;
                //Instantiate(dronePrefab, spawnPos, Quaternion.identity);
                ObjectPooler.Instance.SpawnFromPool("Drone", spawnPos, Quaternion.identity);

                yield return new WaitForSecondsRealtime(interval);
            }

            _currentlySpawning = false;
        }

        private IEnumerator IdleCountdown()
        {
            yield return new WaitForSecondsRealtime(1f);

            if (activeEnemies.Count == 0) 
                yield return SpawnDrones(_droneSpawnAmount, _droneSpawnInterval);
        }
    }
}