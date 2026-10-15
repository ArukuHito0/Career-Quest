using UnityEngine;
using UnityEditor;

namespace CareerQuest.Enemy
{
    [CustomEditor(typeof(EnemySpawnData))]
    public sealed class EnemySpawnPosEdit : Editor
    {
        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            EnemySpawnData spawnData = (EnemySpawnData)target;
            if (spawnData == null || spawnData.SpawnPoints == null) return;

            for (int i = 0; i < spawnData.SpawnPoints.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newPosition = Handles.PositionHandle(
                    spawnData.SpawnPoints   [i].Position,
                    Quaternion.identity
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spawnData, "Move Spawn Point");
                    spawnData.SpawnPoints[i].Position = newPosition;
                    EditorUtility.SetDirty(spawnData);
                }

                Handles.Label(spawnData.SpawnPoints[i].Position + Vector3.up * 1f, $"Spawn_{i}");
            }
        }
    }
}