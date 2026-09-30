using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 加载界面：异步加载目标场景，显示进度条，加载完切换场景。
/// </summary>
public class LoadingScreenUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("设置")]
    [SerializeField] private float minShowTime = 0.5f;   // 最短显示时间，避免一闪而过

    private bool isLoading = false;

    void Start()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    /// <summary>开始异步加载目标场景。</summary>
    public void LoadSceneAsync(string sceneName)
    {
        if (isLoading) return;
        if (string.IsNullOrEmpty(sceneName)) return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"[LoadingScreenUI] 场景 '{sceneName}' 未加入 Build Settings");
            return;
        }

        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        // 1. 显示加载面板
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        if (progressBar != null)
            progressBar.value = 0f;
        if (progressText != null)
            progressText.text = "0%";

        // 2. 清空事件中心（切场景前）
        EventManager.Clear();

        // 3. 异步加载
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;   // 先不要自动切

        float timer = 0f;

        // 4. 更新进度
        while (!op.isDone)
        {
            timer += Time.unscaledDeltaTime;

            // progress 到 0.9 就是加载完了，剩下的 0.1 是等待激活
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = $"{progress * 100:F0}%";

            // 加载完 + 最短显示时间到了，才允许切换
            if (op.progress >= 0.9f && timer >= minShowTime)
            {
                if (progressText != null)
                    progressText.text = "100%";

                yield return new WaitForSecondsRealtime(0.2f);

                op.allowSceneActivation = true;
            }

            yield return null;
        }

        isLoading = false;
    }
}