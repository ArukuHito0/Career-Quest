using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class CustomUIMenu
{
    // 作成したいプレファブのパス
    const string CustomButtonPath = "Assets/Prefabs/CustomButton.prefab";

    // 第3引数(priority)を10台にすると、標準UIの項目より上にまとまる
    [MenuItem("GameObject/Custom UI/CustomButton", false, 0)]
    static void CreateMyButton(MenuCommand command) => CreateFromPrefab(CustomButtonPath, command);

    static void CreateFromPrefab(string path, MenuCommand command)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"プレファブが見つかりません: {path}");
            return;
        }

        // 右クリックした対象(上部メニューから呼ばれた場合はnull)
        var context = command.context as GameObject;
        Transform parent = ResolveParent(context);

        // プレファブとして接続したままインスタンス化
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        Undo.RegisterCreatedObjectUndo(instance, $"Create {prefab.name}");

        GameObjectUtility.EnsureUniqueNameForSibling(instance);
        EnsureEventSystem();

        Selection.activeGameObject = instance;
    }

    // 親にするTransformを決める(Canvas配下でなければCanvasを探す/作る)
    static Transform ResolveParent(GameObject context)
    {
        if (context == null)
            context = Selection.activeGameObject;

        // Canvas配下ならそのまま親にする
        if (context != null && context.GetComponentInParent<Canvas>() != null)
            return context.transform;

        // プレハブ編集モード中はルートの下に作る
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
            return stage.prefabContentsRoot.transform;

        // シーン内の既存Canvasを探す、なければ作成
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();
        return canvas.transform;
    }

    static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = LayerMask.NameToLayer("UI");
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
        return go.GetComponent<Canvas>();
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        var es = new GameObject("EventSystem", typeof(EventSystem));
        // 新Input System使用時は InputSystemUIInputModule、旧Inputなら StandaloneInputModule
        es.AddComponent<StandaloneInputModule>();
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }
}