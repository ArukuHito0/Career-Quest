using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace CareerQuest.Enemy
{
    //  マップをセルに分割し管理するクラス
    [DisallowMultipleComponent]
    public sealed class SpatialHashManager : MonoBehaviour
    {
        NativeArray<Vector3> _enemyPositions;  // 敵座標配列
        NativeParallelMultiHashMap<int, int> _cellToEntityMap;  // <セルID, オブジェクト>のMap

        public int girdWidth = 1000;  // マップをいくつのセルで埋めるか
        public int cellSize = 10;  // 1つのセルの大きさ
        public int enemyCount = 1000;  // 敵の数

        void Start()
        {
            _enemyPositions = new NativeArray<Vector3>(enemyCount, Allocator.Persistent);
            _cellToEntityMap = new NativeParallelMultiHashMap<int, int>(enemyCount, Allocator.Persistent);
        }

        void Update()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                _enemyPositions[i] = new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100));
            }

            _cellToEntityMap.Clear();

            var job = new UpdateGridJob
            {
                Positions = _enemyPositions,
                CellMap = _cellToEntityMap.AsParallelWriter(),
                CellSize = cellSize,
                GridWidth = girdWidth,
            };

            JobHandle handle = job.Schedule(enemyCount, 64);

            handle.Complete();
        }

        void OnDestroy()
        {
            if (_enemyPositions.IsCreated) _enemyPositions.Dispose();
            if (_cellToEntityMap.IsCreated) _cellToEntityMap.Dispose();
        }

        public void GetEntitiesInCell(int cellId, List<int> results)
        {
            if (_cellToEntityMap.TryGetFirstValue(cellId, out int entityIndex, out var iterator))
            {
                do
                {
                    results.Add(entityIndex);
                } while (_cellToEntityMap.TryGetNextValue(out entityIndex, ref iterator));
            }
        }
    }
}