using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace CareerQuest.Enemy
{
    //  スポーンデータのSOをGUIで操作できるようにするクラス
    [CustomEditor(typeof(EnemySpawnData))]
    public sealed class EnemySpawnPosEdit : Editor
    {
        ReorderableList _reorderableList;

        void OnEnable()
        {
            _reorderableList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("SpawnPoints"),
                true, true, true, true
                );

            _reorderableList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Spawn Points");
            };

            _reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element, true)),
                    element,
                    true
                );
            };

            _reorderableList.elementHeightCallback = (int index) => {
                var element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6;
            };

            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            EnemySpawnData spawnData = (EnemySpawnData)target;
            if (spawnData == null || spawnData.SpawnPoints == null) return;

            int selectedIndex = _reorderableList.index;

            for (int i = 0; i < spawnData.SpawnPoints.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                bool isSelected = (i == selectedIndex);

                Handles.color = isSelected ? Color.red : Color.cyan;
                float sphereSize = isSelected ? 0.7f : 0.4f;
                Handles.SphereHandleCap(
                    0,
                    spawnData.SpawnPoints[i].Position,
                    Quaternion.identity,
                    sphereSize,
                    EventType.Repaint
                );

                Vector3 newPosition = Handles.PositionHandle(
                    spawnData.SpawnPoints[i].Position,
                    Quaternion.identity
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spawnData, "Move Spawn Point");
                    spawnData.SpawnPoints[i].Position = newPosition;
                    EditorUtility.SetDirty(spawnData);
                    Repaint();
                }

                GUIStyle style = new GUIStyle();
                style.normal.textColor = isSelected ? Color.red : Color.white;
                style.fontSize = isSelected ? 15 : 12;
                style.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;

                Handles.Label(spawnData.SpawnPoints[i].Position + Vector3.up * 1f, $"Spawn_{i}", style);
            }
        }
    }
}