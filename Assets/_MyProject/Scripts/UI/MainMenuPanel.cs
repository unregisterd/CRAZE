using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : BasePanel
{
    private Button newGameBtn;
    private Button continueBtn;
    private Button settingsBtn;
    private Button quitBtn;

    private void Awake()
    {
        BindButton("Btn_NewGame", ref newGameBtn, OnNewGameClick);
        BindButton("Btn_Continue", ref continueBtn, OnContinueClick);
        BindButton("Btn_Settings", ref settingsBtn, OnSettingsClick);
        BindButton("Btn_Quit", ref quitBtn, OnQuitClick);
    }

    private void BindButton(string childName, ref Button button, UnityEngine.Events.UnityAction action)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            Debug.LogWarning("MainMenuPanel: button not found -> " + childName);
            return;
        }

        button = child.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
        else
        {
            Debug.LogWarning("MainMenuPanel: Button component missing on -> " + childName);
        }
    }

    private void OnNewGameClick()
    {
        Debug.Log("开始新游戏");
    }

    private void OnContinueClick()
    {
        Debug.Log("继续游戏");
    }

    private void OnSettingsClick()
    {
        UIManager.Instance.OpenPanel(UIConst.SettingsPanel);
    }

    private void OnQuitClick()
    {
        Application.Quit();
    }
}