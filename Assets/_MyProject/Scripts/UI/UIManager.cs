using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    
    private static UIManager _instance;
    private Transform _uiRoot;
    private Dictionary<string, string> pathDict;
    //预制件缓存字典
    private Dictionary<string, GameObject> prefabDict;
    //已打开界面的缓存字典
    private Dictionary<string, BasePanel> panelDict;

    public Transform UIRoot
    {
        get
        {
            if (_uiRoot == null)
            {
                _uiRoot = GameObject.Find("CrazeCanvas").transform;
            }
            return _uiRoot;
        }
    }
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager();
                
            }
            return _instance;
        }
    }
    private UIManager()
    {
            InitDict();
    }
    //打开界面
    public BasePanel OpenPanel(string name)
    {
        BasePanel panel = null;

        if (panelDict.TryGetValue(name, out panel))
        {
            if (panel == null || panel.gameObject == null)
            {
                panelDict.Remove(name);
            }
            else
            {
                panel.OpenPanel();
                return panel;
            }
        }

        if (!pathDict.TryGetValue(name, out string path))
        {
            return null;
        }

        GameObject panelPrefab = Resources.Load<GameObject>(path);
        if (panelPrefab == null)
        {
            return null;
        }

        GameObject panelObject = GameObject.Instantiate(panelPrefab, UIRoot, false);
        panel = panelObject.GetComponent<BasePanel>();
        if (panel == null)
        {
            return null;
        }

        panelDict[name] = panel;
        panel.OpenPanel();
        return panel;
    }
    //关闭界面
    public bool ClosePanel(string name)
    {
       BasePanel panel = null;
        if(panelDict.TryGetValue(name, out panel))
        {
            panel.ClosePanel();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemovePanel(string name)
    {
        if (panelDict == null)
        {
            return;
        }

        if (panelDict.ContainsKey(name))
        {
            panelDict.Remove(name);
        }
    }

    private void InitDict()
    {
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, BasePanel>();
        pathDict = new Dictionary<string, string>()
        {
            {UIConst.MainMenuPanel, "Prefab/Panel/Menu/MainMenu"},
            {UIConst.SettingsPanel, "Prefab/Panel/Menu/Settings"}
        };
    }

}
public class UIConst
{
    public const string MainMenuPanel = "MainMenu";
    public const string SettingsPanel = "Settings";
}