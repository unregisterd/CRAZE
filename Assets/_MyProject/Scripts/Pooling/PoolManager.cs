using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 池子管理器，按预制体管理多个 ObjectPool。
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [SerializeField] private Transform poolRoot;   // 所有池子对象的父级

    // 预制体 -> 对应的池子
    private Dictionary<GameObject, ObjectPool> poolDict
        = new Dictionary<GameObject, ObjectPool>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (poolRoot == null)
            poolRoot = transform;
    }

    /// <summary>预热一个池子。</summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;
        if (poolDict.ContainsKey(prefab)) return;

        poolDict[prefab] = new ObjectPool(prefab, count, poolRoot);
    }

    /// <summary>从池子里取出一个对象。</summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        // 没预热过就现创建一个池子
        if (!poolDict.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool(prefab, 0, poolRoot);
            poolDict[prefab] = pool;
        }

        return pool.Get(position, rotation);
    }

    /// <summary>归还对象。如果它有 PooledObject 组件，会自动归还；没有就手动调。</summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        var pooled = obj.GetComponent<PooledObject>();
        if (pooled != null && pooled.SourcePrefab != null)
        {
            if (poolDict.TryGetValue(pooled.SourcePrefab, out var pool))
            {
                pool.Release(obj);//归还对象到指定的池子里
                return;
            }
        }

        // 找不到来源，保险起见直接销毁
        Destroy(obj);
    }
}