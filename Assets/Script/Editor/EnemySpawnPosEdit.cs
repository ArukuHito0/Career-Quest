using UnityEngine;
using UnityEditor;

namespace CareerQuest.Enemy
{
    [CustomEditor(typeof(EnemySpawnPos))]
    public sealed class EnemySpawnPosEdit : Editor
    {
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            EnemySpawnPos spawnData = (EnemySpawnPos)target;
            if (spawnData == null || spawnData.SpawnPositions == null) return;

            for (int i = 0; i < spawnData.SpawnPositions.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newPosition = Handles.PositionHandle(spawnData.SpawnPositions[i], Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spawnData, "Move Spawn Point");
                    spawnData.SpawnPositions[i] = newPosition;
                    EditorUtility.SetDirty(spawnData);
                }

                Handles.Label(spawnData.SpawnPositions[i] + Vector3.up * 1f, $"Spawn_{i}");
            }
        }
    }
}