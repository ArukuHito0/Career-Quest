using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using CareerQuest.Core;

public static class CustomButtonSubscriber
{
    public static void Subscribe(string name, ButtonEventType type, UnityAction action)
    {
        if (TryFind(name, out var button))
            button.AddListener(type, action);
        else
            Debug.LogWarning($"[CustomButtonSubscriber] ボタンが見つかりません: {name}");
    }

    public static void Unsubscribe(string name, ButtonEventType type, UnityAction action)
    {
        // 解除は、ボタンが既に無い場合は何もしなくてよい(警告も不要)
        if (TryFind(name, out var button))
            button.RemoveListener(type, action);
    }

    public static void UnsubscribeAll(string name)
    {
        if (TryFind(name, out var button))
            button.RemoveAllListeners();
    }

    private static bool TryFind(string name, out CustomButton button)
    {
        button = ServiceLocator.Resolve<CustomButtonManager>()?.FindButton(name);
        return button != null;
    }
}