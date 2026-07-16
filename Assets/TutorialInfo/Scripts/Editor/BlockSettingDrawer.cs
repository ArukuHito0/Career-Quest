using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BlockSetting))]
public class BlockSettingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 各プロパティを取得
        SerializedProperty name = property.FindPropertyRelative("name");
        SerializedProperty useSurfaceAndInside = property.FindPropertyRelative("useSurfaceAndInside");
        SerializedProperty surface = property.FindPropertyRelative("surfacePrefab");
        SerializedProperty inside = property.FindPropertyRelative("insidePrefab");
        SerializedProperty single = property.FindPropertyRelative("singlePrefab");

        // 描画位置の計算
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(rect, name);
        rect.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.PropertyField(rect, useSurfaceAndInside);
        rect.y += EditorGUIUtility.singleLineHeight + 2;

        // チェックボックスの状態に応じて表示・非表示を切り替え
        if (useSurfaceAndInside.boolValue)
        {
            EditorGUI.PropertyField(rect, surface);
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, inside);
        }
        else
        {
            EditorGUI.PropertyField(rect, single);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 描画に必要な高さを計算
        bool use = property.FindPropertyRelative("useSurfaceAndInside").boolValue;
        return use ? EditorGUIUtility.singleLineHeight * 5 + 10 : EditorGUIUtility.singleLineHeight * 4 + 8;
    }
}