/// <summary>
/// 跨场景的读档标记。
/// 主菜单选完槽位后，把要读的槽位号记在这里；
/// 进入游戏场景后，由 SceneLoadHandler 读取这个标记，执行读档。
/// </summary>
public static class PendingLoadFlag
{
    /// <summary>进入游戏场景后是否需要执行读档。</summary>
    public static bool ShouldLoadAfterScene = false;

    /// <summary>要读取的槽位号，-1 表示无效。</summary>
    public static int SlotToLoad = -1;
}