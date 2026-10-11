using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Craze.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class CrazeMenuController : MonoBehaviour
    {
        [Header("元素引用")]
        public RectTransform panelMain;
        public RectTransform titleBlock;
        public TMP_Text titleText;
        public CanvasGroup titleGroup;
        public Button[] buttons;
        public CanvasGroup[] buttonGroups;

        [Header("节奏参数")]
        public float titleDelay = 0.15f;
        public float panelSweepTime = 0.45f;
        public float titleBounceTime = 0.35f;
        public float buttonBounceTime = 0.30f;
        public float buttonStagger = 0.08f;

        public bool playOnStart = true;

        [Header("跳转场景")]
        public string newGameSceneName = "Demo";
        public string continueSceneName = "Demo";
        public bool autoBindButtons = true;

        [Header("存档槽位面板")]
        public SaveSlotPanel saveSlotPanel;

        [Header("加载界面")]
        public LoadingScreenUI loadingScreen;

    void Start()
    {
        if (autoBindButtons) BindButtons();
        if (playOnStart) PlayIntro();
    }
        public void BindButtons()
        {
            if (buttons == null) return;

            // 挂 CrazeButtonHover，让按钮悬停时自动放大
            for (int i = 0; i < buttons.Length; i++)
            {
                EnsureHover(buttons[i]);
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

    public void PlayIntro()
    {
        // ① 主面板
        if (panelMain != null)
        {
            var cg = panelMain.GetComponent<CanvasGroup>();

            if (cg != null)
            {
                Vector2 start = panelMain.anchoredPosition + new Vector2(320f, 0f);
                Vector2 end = panelMain.anchoredPosition;
                panelMain.anchoredPosition = start;
    
                CrazeTween.MoveTo(panelMain, end, panelSweepTime, Easing.OutCubic);

                cg.alpha = 0f;

                CrazeTween.Fade(cg, 1f, panelSweepTime, Easing.OutCubic);
            }
        }
        // ② 标题
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

        // ③ 按钮
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
            }
        }
    }

        public void OnNewGameClicked() => LoadScene(newGameSceneName);

        public void OnContinueClicked()
        {
            if (saveSlotPanel == null)
            {
                return;
            }

            saveSlotPanel.Open(
                SaveSlotPanel.Mode.Load,
                (slot) =>
                {
                    PendingLoadFlag.ShouldLoadAfterScene = true;
                    PendingLoadFlag.SlotToLoad = slot;
                    LoadScene(continueSceneName);
                }
            );
        }

        public void OnSettingsClicked()
        {
            UIManager.Instance.OpenPanel(UIConst.SettingsPanel);
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return;
            }

            if (loadingScreen != null)
                loadingScreen.LoadSceneAsync(sceneName);
            else
                SceneLoader.LoadScene(sceneName);
        }
    }

    public static class CrazeMenuDefaults
    {
        public const string ShaderName = "Craze/UI/Diagonal";
        public const float TitleDelay = 0.15f;
    }
}