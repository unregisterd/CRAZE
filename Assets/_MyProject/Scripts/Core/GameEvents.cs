/// <summary>
/// 所有事件名的常量定义，避免字符串拼写错误。
/// </summary>
public static class GameEvents
{
    public const string ItemAdded      = "ItemAdded";
    public const string ItemRemoved       = "ItemRemoved";
    public const string DoorOpened        = "DoorOpened";
    public const string DoorClosed        = "DoorClosed";
    public const string PlatePressed      = "PlatePressed";
    public const string PlateReleased     = "PlateReleased";
    public const string PlayerDied        = "PlayerDied";
    public const string CheckpointReached = "CheckpointReached";
    public const string InteractableEnter = "InteractableEnter";  // 进入可交互范围
    public const string InteractableExit  = "InteractableExit";   // 离开可交互范围
    public const string GameSaved  = "GameSaved";
    public const string GameLoaded = "GameLoaded";
    public const string DialogueStarted = "DialogueStarted";
    public const string DialogueEnded = "DialogueEnded";
    public const string QuestStateChanged = "QuestStateChanged";
    public const string QuestCompleted = "QuestCompleted";

}