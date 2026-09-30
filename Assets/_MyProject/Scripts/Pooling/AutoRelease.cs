using UnityEngine;

public class AutoRelease : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;

    void OnEnable()
    {
        Invoke(nameof(Release), lifetime);
    }

    void Release()
    {
        PoolManager.Instance.Release(gameObject);
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}