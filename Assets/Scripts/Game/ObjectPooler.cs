using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LevelGeneration;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool isChunk;
    }
    
    public static ObjectPooler Instance;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Dictionary<string, Queue<GameObject>> chunkPoolDictionary;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        chunkPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            if (pool.isChunk)
                chunkPoolDictionary.Add(pool.tag, objectPool);
            else
                poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist!");
            return null;
        }

        GameObject objToSpawn = poolDictionary[tag].Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.transform.parent = parent;

        IPoolerObject poolerObject = objToSpawn.GetComponent<IPoolerObject>();
        poolerObject?.OnObjectSpawn();

        poolDictionary[tag].Enqueue(objToSpawn);

        return objToSpawn;
    }

    public GameObject SpawnRandomChunk(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var rand = Random.Range(0, chunkPoolDictionary.Count);
        GameObject objToSpawn = chunkPoolDictionary.ElementAt(rand).Value.Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.transform.parent = parent;

        IPoolerObject poolerObject = objToSpawn.GetComponent<IPoolerObject>();
        poolerObject?.OnObjectSpawn();

        chunkPoolDictionary.ElementAt(rand).Value.Enqueue(objToSpawn);

        return objToSpawn;
    }

    public IEnumerator CustomDestroy(GameObject objToDestroy, float delay = 0)
    {
        yield return new WaitForSecondsRealtime(delay);
        objToDestroy.transform.parent = null;

        var chunk = objToDestroy.GetComponent<Chunk>();

        if (chunk)
        {
            foreach (var enemy in chunk.enemies)
            {
                enemy.gameObject.SetActive(true);
            }
        }
        
        objToDestroy.SetActive(false);
    }
}