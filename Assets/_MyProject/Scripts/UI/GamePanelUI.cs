using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏内存档面板。按 ESC 打开/关闭，提供保存、读取、返回游戏、返回主菜单。
/// </summary>
public class GamePanelUI : MonoBehaviour
{
    [Header("根面板")]
    [SerializeField] private GameObject panelRoot;

    [Header("存档槽位面板")]
    [SerializeField] private SaveSlotPanel slotPanel;

    [Header("读档加载界面")]
    [SerializeField] private LoadOverlayUI loadOverlay;

    [Header("按钮")]
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnLoad;
    [SerializeField] private Button btnReturnGame;   // 返回游戏
    [SerializeField] private Button btnReturnMenu;   // 返回主菜单

    [Header("提示")]
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("场景名")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    private bool isOpen = false;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        // 注册监听点击事件
        if (btnSave != null) btnSave.onClick.AddListener(OnSaveClicked);
        if (btnLoad != null) btnLoad.onClick.AddListener(OnLoadClicked);
        if (btnReturnGame != null) btnReturnGame.onClick.AddListener(OnReturnGameClicked);
        if (btnReturnMenu != null) btnReturnMenu.onClick.AddListener(OnReturnMenuClicked);
    }

    void OnEnable()
    {
        // 监听存档、读档事件
        EventManager<string>.StartListening(GameEvents.GameSaved, OnGameSaved);
        EventManager<string>.StartListening(GameEvents.GameLoaded, OnGameLoaded);
    }

    void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.GameSaved, OnGameSaved);
        EventManager<string>.StopListening(GameEvents.GameLoaded, OnGameLoaded);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if(UIState.IsAnyPanelOpen) return;

        UIState.IsAnyPanelOpen = true;
        isOpen = true;
        panelRoot.SetActive(true);
        SetTip("");
        Time.timeScale = 0f;
        // 鼠标可见
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        isOpen = false;
        UIState.IsAnyPanelOpen = false;
        
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
        // 恢复游戏场景， 鼠标不可见
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnSaveClicked()
    {
        if (slotPanel == null) return;
        // 打开存档槽面板进行存档，不需要回调
        slotPanel.Open(SaveSlotPanel.Mode.Save);
    }

    private void OnLoadClicked()
    {
        if (slotPanel == null) return;
        // 打开存档槽面板进行读档，需要回调
        slotPanel.Open(SaveSlotPanel.Mode.Load, (slot) =>
        {
            StartCoroutine(LoadRoutine(slot));
        });
    }

    // 协程
    private System.Collections.IEnumerator LoadRoutine(int slot)
    {
        // 1. 显示加载界面
        if (loadOverlay != null)
            loadOverlay.Show("读取中...");

        // 2. 等遮罩淡入
        yield return new WaitForSecondsRealtime(0.3f);

        // 3. 恢复游戏，让物理和相机开始跑
        Time.timeScale = 1f;

        // 4. 执行读档
        bool ok = SaveManager.Instance.Load(slot);
        if (!ok)
        {
            if (loadOverlay != null) loadOverlay.Hide();
            yield break;
        }

        // 5. 等 Invector 完成追赶（被加载界面盖住）
        yield return new WaitForSecondsRealtime(2f);

        // 6. 关闭菜单（会再设 timeScale = 1，并锁鼠标）
        Close();


        // 7. 加载界面淡出
        if (loadOverlay != null)
            loadOverlay.Hide();
    }

    private void OnReturnGameClicked()
    {
        // 返回游戏：关面板，继续玩
        Close();
    }

    private void OnReturnMenuClicked()
    {
        // 返回主菜单：恢复时间，加载主菜单场景
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneLoader.LoadScene(mainMenuSceneName);
    }
    // 点击存档按钮之后的提示
    private void OnGameSaved(string path)
    {
        SetTip("已保存");
        // 1.5s 之后清除提示
        Invoke(nameof(ClearTip), 1.5f);
    }
    // 点击读档按钮之后的提示
    private void OnGameLoaded(string path)
    {
        SetTip("已读取");
        // 1.5s 之后清除提示
        Invoke(nameof(ClearTip), 1.5f);
    }

    private void SetTip(string text)
    {
        if (tipText != null)
            tipText.text = text;
    }

    private void ClearTip()
    {
        SetTip("");
    }
}