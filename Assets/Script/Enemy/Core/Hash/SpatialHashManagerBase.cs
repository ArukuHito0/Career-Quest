using CareerQuest.Core;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CareerQuest.Enemy
{
    //  マップをグリッドで管理するマネージャーベース
    [DisallowMultipleComponent]
    public abstract class SpatialHashManagerBase<T> : MonoBehaviour where T : Component, ISpatialEntity
    {
        public List<T> ActiveEntities = new List<T>();

        protected NativeArray<Vector3> positions;  // 敵座標配列
        protected NativeParallelMultiHashMap<int, int> cellToEntityMap;  // <セルID, セル内のオブジェクトの数>のMap
        protected NativeArray<float> ticknesses;  // <セルID, セル内のオブジェクトの数>のMap

        [Min(1)] public readonly int girdWidth = 1000;  // マップをいくつのセルで埋めるか
        [Min(1)] public readonly int cellSize = 10;  // 1つのセルの大きさ
        protected int maxEntitie = 50;  // 敵の数

        public NativeArray<Vector3> Positions { get => positions; }
        public NativeParallelMultiHashMap<int, int> CellToEntityMap { get => cellToEntityMap; }
        public NativeArray<float> Ticknesses { get => ticknesses; }

        protected virtual void Start()
        {
            positions = new NativeArray<Vector3>(maxEntitie, Allocator.Persistent);
            cellToEntityMap = new NativeParallelMultiHashMap<int, int>(maxEntitie, Allocator.Persistent);
            ticknesses = new NativeArray<float>(maxEntitie, Allocator.Persistent);
        }

        protected virtual void Update()
        {
            for (int i = 0; i < ActiveEntities.Count; i++)
            {
                if (ActiveEntities[i] != null)
                {
                    //  ポジションとセルマップとエンティティの要素一致個所
                    positions[i] = ActiveEntities[i].transform.position;
                    ticknesses[i] = ActiveEntities[i].Tickness;
                }
            }

            cellToEntityMap.Clear();

            var job = new UpdateGridJob
            {
                Positions = positions,
                CellMap = cellToEntityMap.AsParallelWriter(),
                CellSize = cellSize,
                GridWidth = girdWidth,
            };

            JobHandle handle = job.Schedule(ActiveEntities.Count, 64);

            handle.Complete();
        }

        protected virtual void OnDestroy()
        {
            if (positions.IsCreated) positions.Dispose();
            if (cellToEntityMap.IsCreated) cellToEntityMap.Dispose();
            if (ticknesses.IsCreated) ticknesses.Dispose();
        }

        //  辞書に登録
        public virtual void Register(T entity)
        {
            entity.Index = ActiveEntities.Count;
            ActiveEntities.Add(entity);

            if (entity.Index >= positions.Length)
            {
                positions.Dispose();
                positions = new NativeArray<Vector3>(ActiveEntities.Count + 100, Allocator.Persistent);
            }
        }

        //  セルIDから周囲の状況を取得
        public void GetEntitiesInCell(int cellId, List<int> results)
        {
            if (cellToEntityMap.TryGetFirstValue(cellId, out int entityIndex, out var iterator))
            {
                results.Add(entityIndex);

                while (cellToEntityMap.TryGetNextValue(out entityIndex, ref iterator))
                {
                    results.Add(entityIndex);
                }
            }
        }
    }

    //  グリッド計算構造体
    [BurstCompile]
    public struct UpdateGridJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> Positions;
        public NativeParallelMultiHashMap<int, int>.ParallelWriter CellMap;
        public int CellSize;
        public int GridWidth;

        public void Execute(int index)
        {
            int x = Mathf.FloorToInt(Positions[index].x / CellSize);
            int z = Mathf.FloorToInt(Positions[index].z / CellSize);

            //  セルのX位置 = CellIds[x] % GridWidth
            //  セルのZ位置 = CellIds[z] / GridWidth
            int cellIds = x + (z * GridWidth);

            //  ポジションとセルマップとエンティティの要素一致個所
            CellMap.Add(cellIds, index);
        }
    }
}