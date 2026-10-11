using UnityEngine;
using Cinemachine;

public class PlayerPositionSaver : MonoBehaviour, ISaveable
{
    void Start()
    {
        // 延迟注册，确保 SaveManager 已经初始化
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
        }
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }

    public void OnSave(SaveData data)
    {
        data.playerPosition = transform.position;
    }

    public void OnLoad(SaveData data)
    {
        transform.position = data.playerPosition;
    }
}