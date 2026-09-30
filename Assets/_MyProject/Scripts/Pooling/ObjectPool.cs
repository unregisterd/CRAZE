using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个对象池，管理一种预制体的实例。
/// 负责取出、归还、预热。
/// </summary>
/// Queue<GameObject>：先进先出，取一个就出队，还一个就入队。

/// Get：池子空了就 Instantiate 一个新的，不会报错。

/// Release：不销毁，只隐藏，重新挂回池子根。

/// SetParent(poolRoot)：让 Hierarchy 里的对象不乱跑。
/// 
public class ObjectPool
{
    private GameObject prefab;          // 这个池子对应的预制体
    private Transform poolRoot;         // 池子的父对象，让 Hierarchy 整洁
    private Queue<GameObject> pool;     // 空闲对象队列

    public ObjectPool(GameObject prefab, int prewarmCount, Transform poolRoot)
    {
        this.prefab = prefab;
        this.poolRoot = poolRoot;
        pool = new Queue<GameObject>(prewarmCount);

        // 预热：先生成一批，隐藏起来放进池子
        for (int i = 0; i < prewarmCount; i++)
        {
            var obj = CreateNew();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>从池子里取出一个对象。</summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        // 池子里有就拿出来，没有就新建一个
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateNew();
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>归还对象到池子。</summary>
    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(poolRoot);
        pool.Enqueue(obj);
    }

    /// <summary>创建一个新实例，挂到池子根下。</summary>
    private GameObject CreateNew()
    {
        var obj = Object.Instantiate(prefab, poolRoot);
        var pooled = obj.GetComponent<PooledObject>();
        if (pooled == null) pooled = obj.AddComponent<PooledObject>();
        pooled.SetSourcePrefab(prefab);
        return obj;
    }
}