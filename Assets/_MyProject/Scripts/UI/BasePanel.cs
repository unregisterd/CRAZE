using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour
{
    protected bool isRemove = false;
    protected string panelName;

    public virtual void OpenPanel()
    {
        panelName = gameObject.name;
        gameObject.SetActive(true);
    }

    public virtual void ClosePanel()
    {
        isRemove = true;

        if (!string.IsNullOrEmpty(panelName))
        {
            UIManager.Instance.RemovePanel(panelName);
        }
        else if (gameObject != null)
        {
            UIManager.Instance.RemovePanel(gameObject.name);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
