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
        public List<T> Entities = new List<T>();

        protected NativeArray<Vector3> enemyPositions;  // 敵座標配列
        protected NativeParallelMultiHashMap<int, int> cellToEntityMap;  // <セルID, セル内のオブジェクトの数>のMap

        [Min(1)] public readonly int girdWidth = 1000;  // マップをいくつのセルで埋めるか
        [Min(1)] public readonly int cellSize = 10;  // 1つのセルの大きさ
        protected int enemyCount = 1000;  // 敵の数

        //protected virtual void Awake()
        //{
        //    ServiceLocator.Register(this.GetType());  派生クラスのインスタンスを格納しないためコメントアウト中
        //}

        protected virtual void Start()
        {
            enemyPositions = new NativeArray<Vector3>(enemyCount, Allocator.Persistent);
            cellToEntityMap = new NativeParallelMultiHashMap<int, int>(enemyCount, Allocator.Persistent);
        }

        protected virtual void Update()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] != null)
                {
                    //  ポジションとセルマップとエンティティの要素一致個所
                    enemyPositions[i] = Entities[i].transform.position;
                }
            }

            cellToEntityMap.Clear();

            var job = new UpdateGridJob
            {
                Positions = enemyPositions,
                CellMap = cellToEntityMap.AsParallelWriter(),
                CellSize = cellSize,
                GridWidth = girdWidth,
            };

            JobHandle handle = job.Schedule(Entities.Count, 64);

            handle.Complete();
        }

        //protected virtual void OnDestroy()
        //{
        //    ServiceLocator.Unregister(this.GetType());  //  派生クラスのインスタンスを登録解除しないためコメントアウト中
        //}

        //  辞書に登録
        public virtual void Register(T entity)
        {
            entity.Index = Entities.Count;
            Entities.Add(entity);

            if (entity.Index >= enemyPositions.Length)
            {
                enemyPositions.Dispose();
                enemyPositions = new NativeArray<Vector3>(Entities.Count + 100, Allocator.Persistent);
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