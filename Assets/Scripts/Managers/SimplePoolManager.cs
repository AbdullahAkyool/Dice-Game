using System.Collections;
using System.Collections.Generic;
using DiceGame.Data;
using DiceGame.Pooling;
using UnityEngine;
using UnityEngine.Pool;

namespace DiceGame.Managers
{
    public class SimplePoolManager : MonoBehaviour
    {
        public static SimplePoolManager Instance { get; private set; }

        [Header("Parents")]
        [SerializeField] private Transform uiPoolParent;
        [SerializeField] private Transform worldPoolParent;

        private readonly Dictionary<PoolKey, ObjectPool<IPoolable>> pools = new Dictionary<PoolKey, ObjectPool<IPoolable>>();
        private readonly Dictionary<PoolKey, Transform> defaultParents = new Dictionary<PoolKey, Transform>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (DatabaseManager.Instance == null || DatabaseManager.Instance.PoolObjectsDatabase == null)
            {
                Debug.LogError("[SimplePoolManager] DatabaseManager or PoolObjectsDatabase not assigned.");
                return;
            }

            EnsureParents();
            InitializePools();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void EnsureParents()
        {
            if (uiPoolParent == null)
            {
                var go = new GameObject("UI_Pool_Parent");
                uiPoolParent = go.transform;
                uiPoolParent.SetParent(transform, false);
            }

            if (worldPoolParent == null)
            {
                var go = new GameObject("World_Pool_Parent");
                worldPoolParent = go.transform;
                worldPoolParent.SetParent(transform, true);
            }
        }

        private void InitializePools()
        {
            var poolDatabase = DatabaseManager.Instance.PoolObjectsDatabase;
            var list = poolDatabase.GetPoolObjectDataList();
            if (list == null)
            {
                Debug.LogError("[SimplePoolManager] PoolObjectsDatabase list is null.");
                return;
            }

            var processedTypes = new HashSet<PoolKey>();

            foreach (var poolData in list)
            {
                if (poolData.Type == PoolKey.None)
                {
                    Debug.LogWarning("[SimplePoolManager] Skipped entry with Type None.");
                    continue;
                }

                if (processedTypes.Contains(poolData.Type))
                    continue;

                if (poolData.Prefab == null)
                {
                    Debug.LogWarning($"[SimplePoolManager] Pool data '{poolData.Type}' has null Prefab.");
                    continue;
                }

                if (poolData.Prefab.GetComponent<IPoolable>() == null)
                {
                    Debug.LogError($"[SimplePoolManager] Prefab '{poolData.Prefab.name}' for '{poolData.Type}' needs IPoolable.");
                    continue;
                }

                CreatePool(poolData);
                processedTypes.Add(poolData.Type);
            }
        }

        private void CreatePool(PoolObjectData poolData)
        {
            PoolKey key = poolData.Type;
            string prefabName = poolData.Prefab.name;
            string parentName = poolData.IsUIObject ? $"{prefabName}_UIParent" : $"{prefabName}_WorldParent";

            var poolParentObj = new GameObject(parentName);
            Transform poolParent = poolParentObj.transform;

            if (poolData.IsUIObject)
                poolParent.SetParent(uiPoolParent, false);
            else
                poolParent.SetParent(worldPoolParent, true);

            defaultParents[key] = poolParent;

            int initialSize = Mathf.Max(1, poolData.InitialSize);
            int maxSize = poolData.MaxSize > 0 ? poolData.MaxSize : 100;

            var pool = new ObjectPool<IPoolable>(
                createFunc: () => CreatePooledObject(poolData, poolParent),
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPooledObject,
                collectionCheck: true,
                defaultCapacity: initialSize,
                maxSize: maxSize);

            pools[key] = pool;
            PrewarmPool(pool, initialSize);
        }

        private IPoolable CreatePooledObject(PoolObjectData poolData, Transform parent)
        {
            GameObject obj = Instantiate(poolData.Prefab, parent);
            obj.name = poolData.Prefab.name;

            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable == null)
            {
                Destroy(obj);
                return null;
            }

            obj.SetActive(false);
            return poolable;
        }

        private static void OnGetFromPool(IPoolable poolable)
        {
            poolable?.OnSpawn();
        }

        private static void OnReleaseToPool(IPoolable poolable)
        {
            poolable?.OnDespawn();
        }

        private static void PrewarmPool(ObjectPool<IPoolable> pool, int initialSize)
        {
            var createdObjects = new List<IPoolable>(initialSize);

            for (int i = 0; i < initialSize; i++)
            {
                IPoolable poolable = pool.Get();
                if (poolable != null)
                {
                    createdObjects.Add(poolable);
                }
            }

            for (int i = 0; i < createdObjects.Count; i++)
            {
                pool.Release(createdObjects[i]);
            }
        }

        private void OnDestroyPooledObject(IPoolable poolable)
        {
            if (poolable is Component component && component != null)
                Destroy(component.gameObject);
        }

        public T Spawn<T>(PoolKey poolKey) where T : Component, IPoolable
        {
            if (!pools.TryGetValue(poolKey, out ObjectPool<IPoolable> pool))
            {
                Debug.LogError($"[SimplePoolManager] Pool '{poolKey}' not found.");
                return null;
            }

            IPoolable poolable = pool.Get();
            return poolable as T;
        }

        public void Despawn(PoolKey poolKey, IPoolable poolable)
        {
            if (poolable == null)
                return;

            if (!pools.TryGetValue(poolKey, out ObjectPool<IPoolable> pool))
            {
                Debug.LogError($"[SimplePoolManager] Pool '{poolKey}' not found for despawn.");
                return;
            }

            if (defaultParents.TryGetValue(poolKey, out Transform parent) && poolable is Component component)
                component.transform.SetParent(parent);

            pool.Release(poolable);
        }
    }
}
