using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CareerQuest.Enemy
{
    [CustomEditor(typeof(EnemySpawnData))]
    public sealed class EnemySpawnPosEdit : Editor
    {
        SerializedProperty spawnPointsProp;
        int selectedIndex = -1;
        Dictionary<int, float> elementHeights = new Dictionary<int, float>();

        void OnEnable()
        {
            spawnPointsProp = serializedObject.FindProperty("SpawnPoints");
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            for (int i = 0; i < spawnPointsProp.arraySize; i++)
            {
                var elementProp = spawnPointsProp.GetArrayElementAtIndex(i);

                if (i == selectedIndex)
                {
                    Rect startCursorRect = EditorGUILayout.GetControlRect(false, 0);

                    float heightToDraw = elementHeights.ContainsKey(i) ? elementHeights[i] : 60f;
                    EditorGUI.DrawRect(
                        new Rect(startCursorRect.x - 2, startCursorRect.y, startCursorRect.width + 4, heightToDraw),
                        new Color(0.24f, 0.48f, 0.90f, 0.3f)
                    );
                }

                Rect totalRect = EditorGUILayout.BeginVertical();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(elementProp, true);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedIndex = i;
                }
                EditorGUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint)
                {
                    elementHeights[i] = totalRect.height;
                }

                if (
                    Event.current != null 
                    && Event.current.type == EventType.MouseDown
                    && totalRect.Contains(Event.current.mousePosition)
                    )
                {
                    selectedIndex = i;
                    Repaint();
                }

                EditorGUILayout.Space(5);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            EnemySpawnData spawnData = (EnemySpawnData)target;
            if (spawnData == null || spawnData.SpawnPoints == null) return;

            for (int i = 0; i < spawnData.SpawnPoints.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                bool isSelected = (i == selectedIndex);

                Handles.color = isSelected ? Color.red : Color.cyan;
                float sphereSize = isSelected ? 0.8f : 0.4f;
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

                    selectedIndex = i;
                    Repaint();
                }

                GUIStyle style = new GUIStyle();
                style.normal.textColor = isSelected ? Color.red : Color.white;
                style.fontSize = isSelected ? 16 : 12;
                style.fontStyle = FontStyle.Bold;

                Handles.Label(spawnData.SpawnPoints[i].Position + Vector3.up * 1f, $"Spawn_{i}", style);
            }
        }
    }
}