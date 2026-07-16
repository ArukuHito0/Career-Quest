using CareerQuest.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CareerQuest.Enemy
{
    //  周囲探索
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

    //  移動
    [BurstCompile]
    public struct MoveJob : IJobParallelFor
    {
        public NativeArray<EnemyData> Datas;  // 敵データ
        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
        [ReadOnly] public NativeArray<float> TreasureTickness;  // お宝の厚さ
        [ReadOnly] public NativeArray<Vector3> WallPositions; // 壁の座標

        public float WallAvoidRadius;  // 壁を避け始める距離
        public float EnemyAvoidRadius;  // 敵同士で避け始める距離
        public float AttackRange;  // 攻撃可能距離

        public float DeltaTime;

        public void Execute(int index)
        {
            var data = Datas[index];
            if (data.TargetIndex < 0) return;

            Vector3 toTarget = TreasurePositions[data.TargetIndex] - data.Position;
            float distSqToTarget = toTarget.sqrMagnitude;

            float targetRadius = TreasureTickness[data.TargetIndex];
            float effectiveAttackRange = AttackRange + data.BodyTickness + targetRadius;

            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
            {
                data.State = (byte)EnemyState.Attack;
                Datas[index] = data;

                return;
            }

            Vector3 dir = toTarget / Mathf.Sqrt(distSqToTarget);
            dir.y = 0;
            Vector3 avoidance = Vector3.zero;

            for (int i = 0; i < Datas.Length; i++)
            {
                if (i == index) continue;

                float combinedRadius = data.BodyTickness + Datas[i].BodyTickness;
                float effectiveAvoidRadius = EnemyAvoidRadius + combinedRadius;
                float sqrEffectiveAvoidRadius = effectiveAvoidRadius * effectiveAvoidRadius;

                Vector3 diff = data.Position - Datas[i].Position;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < sqrEffectiveAvoidRadius)
                {
                    avoidance += (data.Position - Datas[i].Position).normalized * (EnemyAvoidRadius - sqrDist);
                }
            }

            float wallAvoidRadSq = WallAvoidRadius * WallAvoidRadius;
            for (int i = 0; i < WallPositions.Length; i++)
            {
                Vector3 diff = data.Position - WallPositions[i];
                diff.y = 0;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < wallAvoidRadSq)
                {

                    float dist = Mathf.Sqrt(sqrDist);
                    avoidance += diff / dist * (WallAvoidRadius - dist) * 2;
                }
            }

            avoidance.y = 0;

            data.Position += (dir + avoidance) * data.MoveSpeed * DeltaTime;
            data.State = (byte)EnemyState.Move;
            Datas[index] = data;
        }
    }
}