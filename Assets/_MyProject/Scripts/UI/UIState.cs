/// <summary>
/// 记录当前哪个全屏面板是打开状态，用于面板互斥。
/// </summary>
public static class UIState
{
    public static bool IsAnyPanelOpen = false;
}