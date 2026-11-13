using UnityEditor;
using UnityEngine;

namespace CareerQuest.Enemy.Edit
{
    public static class EnemySpawnerUtility
    {
        private const string MenuPath = "Tool/CareerQuest/Spawn Enemies (GUID-Persistent)";

        [MenuItem(MenuPath, false, 10)]
        private static void SpawnFromSelectedSO()
        {
            var spawnData = Selection.activeObject as EnemySpawnData;
            if (spawnData == null) return;

            ExecuteBulkSpawn(spawnData);
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateSpawnFromSelectedSO()
        {
            return Selection.activeObject is EnemySpawnData;
        }

        private static void ExecuteBulkSpawn(EnemySpawnData spawnData)
        {
            if (spawnData.SpawnPoints == null || spawnData.SpawnPoints.Length == 0) return;

            int totalCount = spawnData.SpawnPoints.Length;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Bulk Spawn Enemies (GUID-Optimized)");

            GameObject rootContainer = new GameObject("Generated_Enemies_Root");
            Undo.RegisterCreatedObjectUndo(rootContainer, "Create Enemy Root");

            try
            {
                for (int i = 0; i < totalCount; i++)
                {
                    var point = spawnData.SpawnPoints[i];
                    if (point.Candidates == null || point.Candidates.Length == 0) continue;

                    string enemyTypeName = point.Candidates[0].EnemyID.ToString();

                    // ★ファイル名やパスが変わっても絶対に壊れない「GUID」ベースでプレハブをロード
                    GameObject prefabToSpawn = LoadPrefabByGUIDKey(enemyTypeName);
                    if (prefabToSpawn == null) continue;

                    if (i % 100 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Generating Enemies",
                            "Spawning entities in progress...",
                            (float)i / totalCount
                        );
                    }

                    GameObject spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, rootContainer.transform);
                    if (spawnedObj != null)
                    {
                        spawnedObj.transform.position = point.Position;
                        Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawn Enemy");
                    }
                }

                EditorUtility.SetDirty(spawnData);
                Debug.Log($"<color=cyan>[EnemySpawner] Successfully spawned {totalCount} enemies using persistent GUID tracking!</color>");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Undo.CollapseUndoOperations(undoGroup);
            }
        }

        private static GameObject LoadPrefabByGUIDKey(string typeName)
        {
            // 1. 初回または名前検索でヒットしたプレハブの「不変GUID」を EditorPrefs にキャッシュする
            string prefKey = $"CareerQuest_PrefabGUID_{typeName}";
            string cachedGuid = EditorPrefs.GetString(prefKey, string.Empty);

            string assetPath = string.Empty;

            if (!string.IsNullOrEmpty(cachedGuid))
            {
                assetPath = AssetDatabase.GUIDToAssetPath(cachedGuid);
            }

            // キャッシュがない、またはファイルが移動されて見つからない場合は、名前から再検索してGUIDを更新
            if (string.IsNullOrEmpty(assetPath))
            {
                string[] guids = AssetDatabase.FindAssets($"Enemy_{typeName} t:Prefab");
                if (guids == null || guids.Length == 0) return null;

                cachedGuid = guids[0];
                EditorPrefs.SetString(prefKey, cachedGuid);
                assetPath = AssetDatabase.GUIDToAssetPath(cachedGuid);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }
    }
}