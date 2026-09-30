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
            Debug.Log("[PlayerPositionSaver] 已注册到 SaveManager");
        }
        else
        {
            Debug.LogError("[PlayerPositionSaver] SaveManager.Instance 为空，注册失败");
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
        Debug.Log($"[PlayerPositionSaver] 存档位置：{transform.position}");
    }

    public void OnLoad(SaveData data)
    {
        Debug.Log($"[PlayerPositionSaver] 读档位置：{data.playerPosition}");
        
        transform.position = data.playerPosition;
  
    }
}