using UnityEngine;

public class PoolTest : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PoolManager.Instance.Get(effectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }
}