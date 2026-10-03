using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载管理，切场景前清理事件中心，避免残留监听者。
/// </summary>
public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        // 切场景前清空事件中心
        EventManager.Clear();

        // 重置 UI 状态
        UIState.IsAnyPanelOpen = false;

        SceneManager.LoadScene(sceneName);
    }
}