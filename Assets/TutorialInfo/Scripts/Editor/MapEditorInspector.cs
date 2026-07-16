using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapEditor))]
public class MapEditorInspector : Editor
{
    SerializedProperty blockSettings;
    SerializedProperty currentBlockIndex;

    private void OnEnable()
    {
        blockSettings = serializedObject.FindProperty("blockSettings");
        currentBlockIndex = serializedObject.FindProperty("currentBlockIndex");
    }

    public override void OnInspectorGUI()
    {
        // 既存のプロパティを更新
        serializedObject.Update();

        // 1. 基本的なプロパティ（EditorMode等）を描画
        DrawPropertiesExcluding(serializedObject, "blockSettings", "currentBlockIndex");

        // 2. ブロックリストを描画
        EditorGUILayout.PropertyField(blockSettings, true);

        // 3. インデックスの描画と名前の表示
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(currentBlockIndex);

        // 現在のインデックスに対応する名前を取得して表示
        string currentName = "None";
        int idx = currentBlockIndex.intValue;
        if (idx >= 0 && idx < blockSettings.arraySize)
        {
            SerializedProperty element = blockSettings.GetArrayElementAtIndex(idx);
            currentName = element.FindPropertyRelative("name").stringValue;
        }

        // ラベルとして現在選択中のブロック名を表示（わかりやすく強調）
        EditorGUILayout.HelpBox("現在選択中のブロック: " + currentName, MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }
}