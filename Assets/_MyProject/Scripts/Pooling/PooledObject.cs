using UnityEngine;

/// <summary>
/// 挂在池化对象上，记录它来自哪个预制体，方便归还时找到对应的池子。
/// </summary>
public class PooledObject : MonoBehaviour
{
    public GameObject SourcePrefab { get; private set; }

    public void SetSourcePrefab(GameObject prefab)
    {
        SourcePrefab = prefab;
    }
}