using System;
using UnityEngine;

[Serializable]
public class QuestEntry
{
    public string questId;
    public int progress;   // -1=未开始，0~n=进行中，n+1=已完成
}

[Serializable]
public class ItemEntry
{
    public string itemId;
    public int count;
}

[Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string saveTime;
    public int slotIndex;
    public int version = 1;

    // 通用任务状态，加新任务不用改这里
    public QuestEntry[] questStates;
    public ItemEntry[] itemEntries;
}