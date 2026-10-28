using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SceneType
{
    Title,
    Lobby,
    Result,
    Shop
}

[System.Serializable]
public class SceneData : ISerializationCallbackReceiver
{
    public SceneType sceneType;
#if UNITY_EDITOR
    public SceneAsset scene;
#endif
    [SerializeField, HideInInspector] private string sceneName;

    public string SceneName => sceneName;

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        sceneName = scene != null ? scene.name : string.Empty;
#endif
    }

    public void OnAfterDeserialize() { }
}

[CreateAssetMenu(fileName = "SceneDatabase", menuName = "Scriptable Objects/SceneDatabase")]
public class SceneDatabase : ScriptableObject
{
    public SceneData[] sceneDatas;

    private Dictionary<SceneType, string> cache;

    public bool TryGetSceneName(SceneType type, out string sceneName)
    {
        if (cache == null)
        {
            cache = new Dictionary<SceneType, string>();
            foreach (var data in sceneDatas)
            {
                cache[data.sceneType] = data.SceneName;
            }
        }

        return cache.TryGetValue(type, out sceneName) && !string.IsNullOrEmpty(sceneName);
    }
}
