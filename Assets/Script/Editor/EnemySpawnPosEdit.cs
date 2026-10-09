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
            if (spawnData == null || spawnData.SpawnPositions == null) return;

            for (int i = 0; i < spawnData.SpawnPositions.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newPosition = Handles.PositionHandle(
                    spawnData.SpawnPositions[i].Position,
                    Quaternion.identity
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spawnData, "Move Spawn Point");
                    spawnData.SpawnPositions[i].Position = newPosition;
                    EditorUtility.SetDirty(spawnData);
                }

                Handles.Label(spawnData.SpawnPositions[i].Position + Vector3.up * 1f, $"Spawn_{i}");
            }
        }
    }
}