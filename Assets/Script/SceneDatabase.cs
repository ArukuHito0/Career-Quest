using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public enum SceneType
{
    Title,
    Lobby,
    Stage,
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

#if UNITY_EDITOR
    // このDBがBuild Settingsに登録したシーンのパス（削除判定用）
    [SerializeField, HideInInspector] private List<string> registeredScenePaths = new List<string>();

    private void OnValidate()
    {
        cache = null; // 編集後にキャッシュが古くならないようにする

        // OnValidate中にアセットやBuildSettingsを触るのを避けるため遅延実行
        EditorApplication.delayCall -= SyncBuildSettings;
        EditorApplication.delayCall += SyncBuildSettings;
    }

    private void SyncBuildSettings()
    {
        if (this == null) return; // 既に破棄されている場合

        // 現在DBに設定されているシーンのパス一覧
        var current = new HashSet<string>();
        if (sceneDatas != null)
        {
            foreach (var data in sceneDatas)
            {
                if (data == null || data.scene == null) continue;
                var path = AssetDatabase.GetAssetPath(data.scene);
                if (!string.IsNullOrEmpty(path)) current.Add(path);
            }
        }

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        // 外されたシーンを削除
        foreach (var path in registeredScenePaths)
        {
            if (current.Contains(path)) continue;
            if (scenes.RemoveAll(s => s.path == path) > 0) changed = true;
        }

        // 追加されたシーンを登録
        foreach (var path in current)
        {
            if (scenes.Exists(s => s.path == path)) continue;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            changed = true;
        }

        if (changed)
            EditorBuildSettings.scenes = scenes.ToArray();

        // 登録済みリストを更新
        var newList = current.OrderBy(p => p).ToList();
        if (!registeredScenePaths.SequenceEqual(newList))
        {
            registeredScenePaths = newList;
            EditorUtility.SetDirty(this);
        }
    }
#endif
}