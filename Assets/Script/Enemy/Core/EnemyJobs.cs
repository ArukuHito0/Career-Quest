using CareerQuest.Enemy;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CareerQuest.Enemy
{
    [BurstCompile]
    public struct SearchJob : IJobParallelFor
    {
        public NativeArray<EnemyData> Datas;  // 敵データ
        public NativeArray<Vector3> TreasurePositions;  // お宝座標
        [ReadOnly] public NativeParallelMultiHashMap<int, int> CellToEntityMap;  // <セルID, セル内の宝数>のMap

        public float SearchRadius;  // 探索半径

        public int GridWidth;  // グリッド横幅
        public float CellSize;  // 1つのセルのサイズ
        public float DeltaTime;

        public void Execute(int index)
        {
            var data = Datas[index];
            float minDistance = float.MaxValue;
            int nearestIndex = -1;

            int myX = Mathf.FloorToInt(data.Position.x / CellSize);
            int myZ = Mathf.FloorToInt(data.Position.z / CellSize);
            int myCellId = myX + (myZ * GridWidth);

            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int targetCellId = (myX + dx) + ((myZ + dz) * GridWidth);

                    if (CellToEntityMap.TryGetFirstValue(targetCellId, out int entityIndex, out var iterator))
                    {
                        do
                        {
                            float dist = Vector3.Distance(data.Position, TreasurePositions[entityIndex]);
                            
                            if (dist < data.SearchRadius && dist < minDistance)
                            {
                                minDistance = dist;
                                nearestIndex = entityIndex;
                            }

                        } while (CellToEntityMap.TryGetNextValue(out entityIndex, ref iterator));
                    }
                }
            }
            data.TargetIndex = nearestIndex;
            Datas[index] = data;
        }
    }
}

namespace CareerQuest.Enemy
{
    [BurstCompile]
    public struct MoveJob : IJobParallelFor
    {
        public NativeArray<EnemyData> Datas;  // 敵データ
        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
        public float DeltaTime;

        public void Execute(int index)
        {
            var data = Datas[index];
            if (data.TargetIndex < 0) return;

            Vector3 targetPos = TreasurePositions[data.TargetIndex];
            Vector3 dir = (targetPos - data.Position).normalized;
            
            Vector3 avoidance = Vector3.zero;
            for (int i = 0; i < Datas.Length; i++)
            {
                if (i == index) continue;

                float dist = Vector3.Distance(data.Position, Datas[i].Position);
                if (dist < 1.0f)
                {
                    avoidance += (data.Position - Datas[i].Position).normalized * (1.0f - dist);
                }
            }

            data.Position += (dir + avoidance * 10.0f) * data.MoveSpeed * DeltaTime;
            Datas[index] = data;
        }
    }
}

[BurstCompile]
public struct AvoidanceJob : IJobParallelFor
{
    public NativeArray<EnemyData> Datas;  // 敵データ

    public void Execute(int index)
    {
        var data = Datas[index];
        Datas[index] = data;
    }
}