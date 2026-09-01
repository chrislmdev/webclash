using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int initialSize = 10;
}

/// <summary>
/// Spawns building chunks and booster packs ahead of the player and recycles them when passed.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform poolRoot;

    [Header("Pools")]
    [SerializeField] private List<Pool> pools = new();

    [Header("Spawn")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float despawnBehindDistance = 30f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float laneWidth = 12f;
    [SerializeField] private float buildingMinHeight = 8f;
    [SerializeField] private float buildingMaxHeight = 22f;
    [SerializeField] private float boosterSpawnChance = 0.35f;

    private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new();
    private readonly List<SpawnedChunk> activeChunks = new();
    private float nextSpawnZ;
    private float spawnTimer;

    private struct SpawnedChunk
    {
        public GameObject root;
        public float endZ;
    }

    private void Awake()
    {
        if (poolRoot == null)
        {
            poolRoot = transform;
        }

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null || string.IsNullOrEmpty(pool.tag))
            {
                continue;
            }

            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.initialSize; i++)
            {
                queue.Enqueue(CreateInstance(pool));
            }

            poolDictionary[pool.tag] = queue;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }

        nextSpawnZ = player != null ? player.position.z + spawnAheadDistance * 0.5f : 0f;
    }

    private void Update()
    {
        if (player == null || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnChunk();
        }

        RecyclePassedChunks();
    }

    private void SpawnChunk()
    {
        float playerZ = player.position.z;
        if (nextSpawnZ < playerZ + spawnAheadDistance * 0.5f)
        {
            nextSpawnZ = playerZ + spawnAheadDistance;
        }

        float chunkZ = nextSpawnZ;
        nextSpawnZ += spawnInterval * 15f;

        var chunkRoot = new GameObject($"Chunk_{chunkZ:0}");
        chunkRoot.transform.SetParent(poolRoot, false);
        chunkRoot.transform.position = new Vector3(0f, 0f, chunkZ);

        SpawnBuilding(chunkRoot.transform, new Vector3(-laneWidth * 0.5f, 0f, 0f));
        SpawnBuilding(chunkRoot.transform, new Vector3(laneWidth * 0.5f, 0f, 0f));

        if (Random.value <= boosterSpawnChance)
        {
            SpawnBooster(chunkRoot.transform, new Vector3(Random.Range(-laneWidth * 0.25f, laneWidth * 0.25f), 4f, 0f));
        }

        activeChunks.Add(new SpawnedChunk
        {
            root = chunkRoot,
            endZ = chunkZ
        });
    }

    private void SpawnBuilding(Transform parent, Vector3 localPosition)
    {
        GameObject building = GetFromPool("Building");
        if (building == null)
        {
            return;
        }

        building.transform.SetParent(parent, false);
        building.transform.localPosition = localPosition;

        float height = Random.Range(buildingMinHeight, buildingMaxHeight);
        building.transform.localScale = new Vector3(
            Random.Range(3f, 6f),
            height,
            Random.Range(3f, 6f)
        );

        building.SetActive(true);
    }

    private void SpawnBooster(Transform parent, Vector3 localPosition)
    {
        string tag = Random.value > 0.5f ? "BoosterSpeed" : "BoosterJump";
        GameObject booster = GetFromPool(tag);
        if (booster == null)
        {
            return;
        }

        booster.transform.SetParent(parent, false);
        booster.transform.localPosition = localPosition;
        booster.transform.localScale = Vector3.one;
        booster.SetActive(true);

        var boosterPack = booster.GetComponent<BoosterPack>();
        if (boosterPack != null)
        {
            boosterPack.Initialize(this, tag);
        }
    }

    private GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> queue))
        {
            Debug.LogWarning($"ObjectPooler: No pool registered for tag '{tag}'.");
            return null;
        }

        GameObject instance;
        if (queue.Count > 0)
        {
            instance = queue.Dequeue();
        }
        else
        {
            Pool poolConfig = pools.Find(p => p.tag == tag);
            if (poolConfig == null)
            {
                return null;
            }

            instance = CreateInstance(poolConfig);
        }

        return instance;
    }

    public void ReturnToPool(string tag, GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(poolRoot, false);

        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            poolDictionary[tag] = queue;
        }

        queue.Enqueue(instance);
    }

    private void RecyclePassedChunks()
    {
        float recycleZ = player.position.z - despawnBehindDistance;

        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            SpawnedChunk chunk = activeChunks[i];
            if (chunk.endZ >= recycleZ)
            {
                continue;
            }

            ReturnChunkChildrenToPool(chunk.root);
            Destroy(chunk.root);
            activeChunks.RemoveAt(i);
        }
    }

    private void ReturnChunkChildrenToPool(GameObject chunkRoot)
    {
        for (int i = chunkRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = chunkRoot.transform.GetChild(i);
            GameObject childObject = child.gameObject;

            if (childObject.TryGetComponent(out BoosterPack booster) && !string.IsNullOrEmpty(booster.PoolTag))
            {
                ReturnToPool(booster.PoolTag, childObject);
                continue;
            }

            ReturnToPool("Building", childObject);
        }
    }

    private GameObject CreateInstance(Pool pool)
    {
        GameObject instance = Instantiate(pool.prefab, poolRoot);
        instance.name = pool.prefab.name;

        if (instance.TryGetComponent(out BoosterPack booster))
        {
            booster.Initialize(this, pool.tag);
        }

        instance.SetActive(false);
        return instance;
    }
}
