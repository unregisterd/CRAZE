using System;
using System.Collections.Generic;

/// <summary>
/// 泛型事件中心。每个类型参数 T 是独立的类，互不干扰。
/// </summary>
public static class EventManager<T>
{
    private static readonly Dictionary<string, Action<T>> eventDictionary
        = new Dictionary<string, Action<T>>();

    static EventManager()
    {
        // 注册到 EventRegistry，供统一 Clear
        EventRegistry.Register(() => eventDictionary.Clear());
    }

    public static void StartListening(string eventName, Action<T> listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null) return;

        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent += listener;
            eventDictionary[eventName] = thisEvent;
        }
        else
        {
            eventDictionary[eventName] = listener;
        }
    }

    public static void StopListening(string eventName, Action<T> listener)
    {
        if (string.IsNullOrEmpty(eventName) || listener == null) return;

        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent -= listener;
            eventDictionary[eventName] = thisEvent;

            if (thisEvent == null)
                eventDictionary.Remove(eventName);
        }
    }

    public static void TriggerEvent(string eventName, T data)
    {
        if (string.IsNullOrEmpty(eventName)) return;

        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent?.Invoke(data);
        }
    }

    public static void DebugPrintListeners(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            UnityEngine.Debug.LogWarning("事件名为空");
            return;
        }

        if (eventDictionary.TryGetValue(eventName, out var thisEvent) && thisEvent != null)
        {
            var list = thisEvent.GetInvocationList();
            UnityEngine.Debug.Log($"[{typeof(T).Name}] 事件 [{eventName}] 有 {list.Length} 个监听者：");
            foreach (var d in list)
            {
                UnityEngine.Debug.Log($"  - 对象：{d.Target}，方法：{d.Method.Name}");
            }
        }
        else
        {
            UnityEngine.Debug.Log($"[{typeof(T).Name}] 事件 [{eventName}] 没有监听者");
        }
    }
}

/// <summary>
/// 管理所有泛型事件中心的注册，供统一清理。
/// 把所有泛型实例的 Clear 收集起来，统一清理
/// </summary>
public static class EventRegistry
{
    private static readonly List<Action> clearActions = new List<Action>();

    public static void Register(Action clearAction)
    {
        clearActions.Add(clearAction);
    }

    public static void ClearAll()
    {
        foreach (var action in clearActions)
            action();
    }
}

/// <summary>
/// 非泛型入口，保留原 API，方便统一调用。
/// </summary>
public static class EventManager
{
    public static void Clear()
    {
        EventRegistry.ClearAll();
    }
}