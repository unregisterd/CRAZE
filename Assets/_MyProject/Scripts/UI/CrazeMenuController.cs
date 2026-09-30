using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Craze.UI
{
    /// <summary>
    /// 主菜单运行时控制器。
    /// 演出 P5 风格的入场动画：扫入 + 弹跳（OutBack）+ 错位淡入。
    /// 挂在与生成器配套的 Canvas 层级上。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CrazeMenuController : MonoBehaviour
    {
        [Header("元素引用（由生成器自动填充）")]
        public RectTransform panelMain;
        public RectTransform titleBlock;   // 标题所在的斜切块
        public TMP_Text titleText;
        public CanvasGroup titleGroup;
        public Button[] buttons;
        public CanvasGroup[] buttonGroups;

        [Header("节奏参数")]
        public float titleDelay = 0.15f;       // 标题延迟出现
        public float panelSweepTime = 0.45f;   // 主面板扫入时长
        public float titleBounceTime = 0.35f;  // 标题弹跳时长
        public float buttonBounceTime = 0.30f; // 按钮逐个弹出时长
        public float buttonStagger = 0.08f;    // 按钮之间的错峰

        public bool playOnStart = true;

        [Header("跳转场景")]
        public string newGameSceneName = "Demo1";   // 点「新游戏」跳转的场景
        public string continueSceneName = "Demo1";  // 点「继续」跳转的场景
        public bool autoBindButtons = true;           // 运行时自动绑定按钮点击

        [Header("存档槽位面板")]
        public SaveSlotPanel saveSlotPanel;

        [Header("加载界面")]
        public LoadingScreenUI loadingScreen;

        [Header("背景切换")]
        public string[] backgroundNames = { "one", "two", "three", "four" };  // 图片名
        public string backgroundFolder = "Pictures/scene_0";                  // Resources 下的文件夹路径
        CrazeBackgroundSwitcher bgSwitcher;

        void Start()
        {
            SetupBackground();
            if (autoBindButtons) BindButtons();
            if (playOnStart) PlayIntro();
        }

        /// <summary>创建/获取背景切换器并加载图片。</summary>
        void SetupBackground()
        {
            bgSwitcher = GetComponent<CrazeBackgroundSwitcher>();
            if (bgSwitcher == null) bgSwitcher = gameObject.AddComponent<CrazeBackgroundSwitcher>();
            bgSwitcher.Setup(null, backgroundNames, backgroundFolder);
        }

        /// <summary>按钮悬停 → 切换对应背景图。</summary>
        void OnButtonHover(int index)
        {
            if (bgSwitcher != null) bgSwitcher.Show(index);
        }

        /// <summary>按钮移出 → 目前无需处理，保留接口以便扩展。</summary>
        void OnButtonExit(int index)
        {
        }

        /// <summary>运行时给按钮绑定点击回调（按钮顺序：NewGame / Continue / Settings / Quit）。</summary>
        public void BindButtons()
        {
            if (buttons == null) return;
            for (int i = 0; i < buttons.Length; i++)
            {
                var hover = EnsureHover(buttons[i]);
                if (hover != null)
                {
                    hover.index = i;
                    hover.onHoverEnter = OnButtonHover;
                    hover.onHoverExit = OnButtonExit;
                }
            }
            Bind(0, OnNewGameClicked);
            Bind(1, OnContinueClicked);
            Bind(2, OnSettingsClicked);
            Bind(3, OnQuitClicked);
        }

        static CrazeButtonHover EnsureHover(Button btn)
        {
            if (btn == null) return null;
            var hover = btn.GetComponent<CrazeButtonHover>();
            if (hover == null) hover = btn.gameObject.AddComponent<CrazeButtonHover>();
            return hover;
        }

        void Bind(int index, UnityEngine.Events.UnityAction action)
        {
            if (buttons == null || index >= buttons.Length || buttons[index] == null) return;
            buttons[index].onClick.RemoveListener(action);
            buttons[index].onClick.AddListener(action);
        }

        /// <summary>播放整套入场动画。可在任意时刻调用以重启演出。</summary>
        public void PlayIntro()
        {
            // ① 主面板：从右侧错位扫入 + 整体淡入
            if (panelMain != null && panelMain.GetComponent<CanvasGroup>() != null)
            {
                Vector2 start = panelMain.anchoredPosition + new Vector2(320f, 0f);
                Vector2 end = panelMain.anchoredPosition;
                panelMain.anchoredPosition = start;
                CrazeTween.MoveTo(panelMain, end, panelSweepTime, Easing.OutCubic);
                var cg = panelMain.GetComponent<CanvasGroup>();
                cg.alpha = 0f;
                CrazeTween.Fade(cg, 1f, panelSweepTime, Easing.OutCubic);
            }

            // ② 标题块：弹跳放大（OutBack）+ 淡入
            if (titleBlock != null)
            {
                titleBlock.localScale = Vector3.one * 0.6f;
                CrazeTween.ScaleTo(titleBlock, Vector3.one, titleBounceTime, Easing.OutBack, titleDelay);
            }
            if (titleGroup != null)
            {
                titleGroup.alpha = 0f;
                CrazeTween.Fade(titleGroup, 1f, titleBounceTime, Easing.OutCubic, titleDelay);
            }

            // ③ 按钮：依次弹出（OutBounce/OutBack），错峰
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    float delay = titleDelay + titleBounceTime + (i * buttonStagger);
                    RectTransform rt = buttons[i].transform as RectTransform;
                    if (rt != null)
                    {
                        rt.localScale = Vector3.one * 0.7f;
                        CrazeTween.ScaleTo(rt, Vector3.one, buttonBounceTime, Easing.OutBack, delay);
                    }
                    if (buttonGroups != null && i < buttonGroups.Length && buttonGroups[i] != null)
                    {
                        buttonGroups[i].alpha = 0f;
                        CrazeTween.Fade(buttonGroups[i], 1f, buttonBounceTime, Easing.OutCubic, delay);
                    }
                }
            }
        }

        // 按钮行为
        public void OnNewGameClicked() => LoadScene(newGameSceneName);
        public void OnContinueClicked()
        {   
            if (saveSlotPanel == null)
            {
                Debug.LogWarning("[CRAZE] 没有配置存档槽位面板");
                return;
            }

            // 打开读档模式，executeLoad = false（主菜单不真正读档，只选槽位）
            saveSlotPanel.Open(
                SaveSlotPanel.Mode.Load,
                (slot) =>
                {
                    // 记录要读的槽位，进入游戏场景后由 SceneLoadHandler 执行
                    PendingLoadFlag.ShouldLoadAfterScene = true;
                    PendingLoadFlag.SlotToLoad = slot;
                    LoadScene(continueSceneName);
                }
            );
        }
        public void OnSettingsClicked()
        {
            //Debug.Log("[CRAZE] Settings click ignored; use MainMenuPanel entry.");
            UIManager.Instance.OpenPanel(UIConst.SettingsPanel);
        }
        public void OnQuitClicked()
        {
            // 编辑器里停止 Play
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        /// <summary>加载目标场景；若未加入 Build Settings 则给出明确提示。</summary>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[CRAZE] 场景 '{sceneName}' 未加入 Build Settings，无法加载。" +
                         "请在 File > Build Settings 中把它拖入 Scenes In Build。");
                return;
            }

            if (loadingScreen != null)
            {
                //异步加载
                loadingScreen.LoadSceneAsync(sceneName);
            }
            else
            {
                SceneLoader.LoadScene(sceneName);
            }
            
        }
    }

    // 单独声明，避免字段顺序混乱
    public static class CrazeMenuDefaults
    {
        public const string ShaderName = "Craze/UI/Diagonal";
        public const float TitleDelay = 0.15f;
    }
}