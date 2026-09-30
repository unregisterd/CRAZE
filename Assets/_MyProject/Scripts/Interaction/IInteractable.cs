using UnityEngine;

/// <summary>
/// 所有可交互物体实现的接口。
/// </summary>
public interface IInteractable
{
    /// <summary>UI 显示的提示文字，比如“按 F 开门”</summary>
    string GetPrompt();

    /// <summary>当前是否允许交互，比如需要钥匙时返回 false</summary>
    bool CanInteract(GameObject interactor);

    /// <summary>执行交互</summary>
    void Interact(GameObject interactor);
}