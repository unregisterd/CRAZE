using UnityEngine;

/// <summary>
/// 挂在游戏场景里。场景加载完后，检查 PendingLoadFlag，
/// 如果需要读档，延迟一帧执行 SaveManager.Load(slot)。
/// </summary>
public class SceneLoadHandler : MonoBehaviour
{
    private int _slotToLoad = -1;

    void Start()
    {
        if (PendingLoadFlag.ShouldLoadAfterScene)
        {
            PendingLoadFlag.ShouldLoadAfterScene = false;
            _slotToLoad = PendingLoadFlag.SlotToLoad;
            PendingLoadFlag.SlotToLoad = -1;

            // 延迟一帧，确保所有 ISaveable 都注册好了
            Invoke(nameof(DoLoad), 0.1f);
        }
    }

    void DoLoad()
    {
        if (SaveManager.Instance != null && _slotToLoad >= 0)
        {
            SaveManager.Instance.Load(_slotToLoad);
            Debug.Log($"[SceneLoadHandler] 已执行读档，槽位：{_slotToLoad}");
        }
    }
}