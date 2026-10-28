using UnityEngine;
using System.Collections.Generic;
using CareerQuest.Core;

[DefaultExecutionOrder(-1)]
public class CustomButtonManager : MonoBehaviour
{
    private Dictionary<string, CustomButton> customButtons = new();

    private void Awake()
    {
        ServiceLocator.Register(this);
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    /// <summary>
    /// “n‚³‚ê‚½ CustomButton ‚Æ‚»‚Ì–¼‘O‚ğ Dictionary ‚É•Û‘¶
    /// </summary>
    /// <param name="button"></param>
    public void AddButton(CustomButton button)
    {
        customButtons[button.name] = button;
    }

    /// <summary>
    /// “n‚³‚ê‚½ CustomButton ‚ğ Dicitonary ‚©‚çíœ
    /// </summary>
    /// <param name="button"></param>
    public void RemoveButton(CustomButton button)
    {
        customButtons.Remove(button.name);
    }

    /// <summary>
    /// “n‚³‚ê‚½•¶š—ñ‚Æ“¯‚¶ƒ{ƒ^ƒ“‚ğ•Ô‚·
    /// </summary>
    /// <returns>CustomButton</returns>
    public CustomButton FindButton(string name)
    {
        if(customButtons.TryGetValue(name, out var button))
            return button;
        return null;
    }
}
