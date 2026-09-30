using System;
using UnityEngine;

/// <summary>
/// 存档数据。纯数据类，只有字段，没有逻辑。
/// 用 [Serializable] 让 JsonUtility 能序列化。
/// </summary>
[Serializable]
public class SaveData
{
    // 玩家位置
    public Vector3 playerPosition;

    // 存档时间（可选，用来显示"上次存档于 xx"）
    public string saveTime;

    // 存档版本（可选，用于以后兼容旧存档）
    public int version = 1;
    public int slotIndex;
}