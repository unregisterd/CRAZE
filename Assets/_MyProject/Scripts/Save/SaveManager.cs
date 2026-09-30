using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private int slotCount = 4;   // 存档槽数量

    private readonly List<ISaveable> saveables = new List<ISaveable>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(ISaveable saveable)
    {
        if (saveable != null && !saveables.Contains(saveable))
            saveables.Add(saveable);
    }

    public void Unregister(ISaveable saveable)
    {
        if (saveable != null)
            saveables.Remove(saveable);
    }

    /// <summary>获取某个槽位的文件路径。</summary>
    public string GetSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }

    /// <summary>存档到指定槽位。</summary>
    public void Save(int slot)
    {
        SaveData data = new SaveData();
        data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.slotIndex = slot;

        foreach (var s in saveables)
            s.OnSave(data); // 将要保存的数据加载进来

        // 将 data 转换为 JSON 文件
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slot), json);

        Debug.Log($"[SaveManager] 已存档到槽 {slot}：{GetSlotPath(slot)}");
        // 通知所有关心 游戏已存档 的监听者
        EventManager<string>.TriggerEvent(GameEvents.GameSaved, slot.ToString());
    }

    /// <summary>从指定槽位读档。</summary>
    public bool Load(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] 槽 {slot} 没有存档");
            return false;
        }

        string json = File.ReadAllText(path);
        // 将 JSON 文件转换为 SaveData 对象
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogError("[SaveManager] 存档解析失败");
            return false;
        }

        foreach (var s in saveables)
            s.OnLoad(data);

        Debug.Log($"[SaveManager] 已从槽 {slot} 读档");
        // 通知所有关心 游戏已读档 的监听者
        EventManager<string>.TriggerEvent(GameEvents.GameLoaded, slot.ToString());
        return true;
    }

    /// <summary>某个槽位是否有存档。</summary>
    public bool HasSave(int slot)
    {
        return File.Exists(GetSlotPath(slot));
    }

    /// <summary>读取某个槽位的存档信息（用于 UI 显示），没有则返回 null。</summary>
    public SaveData GetSaveInfo(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>删除某个槽位的存档。</summary>
    public void DeleteSave(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] 已删除槽 {slot}");
        }
    }

    public int SlotCount => slotCount;
}