using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace CareerQuest.Enemy
{
    //  周囲探索
    [BurstCompile]
    public struct SearchTreasureJob : IJobParallelFor
    {
        public NativeArray<EnemyData> InputDatas; // 読み取り用
        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
        [ReadOnly] public NativeParallelMultiHashMap<int, int> CellToEntityMap;  // <セルID, セル内の宝数>のMap

        public int GridWidth;  // グリッド横幅
        public float CellSize;  // 1つのセルのサイズ
        public float DeltaTime;

        public void Execute(int index)
        {
            var data = InputDatas[index];
            
            if(
                data.ID != EnemyID.Golem ||
                data.State == (byte)EnemyState.Attack
                )
                return;

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
                            if (entityIndex < 0 || entityIndex >= TreasurePositions.Length)
                                continue;
                            float dist = Vector3.Distance(data.Position, TreasurePositions[entityIndex]);

                            if (dist < data.GolemSearchRadius && dist < minDistance)
                            {
                                minDistance = dist;
                                nearestIndex = entityIndex;
                            }

                        } while (CellToEntityMap.TryGetNextValue(out entityIndex, ref iterator));
                    }
                }
            }
            data.TargetIndex = nearestIndex;
            data.State = (byte)EnemyState.Search;
            InputDatas[index] = data;
        }
    }

    //  プレイヤー探索
    [BurstCompile]
    public struct SearchPlayerJob : IJobParallelFor
    {
        public NativeArray<EnemyData> InputDatas; // 読み取り用
        [ReadOnly] public NativeArray<Vector3> PlayerPositions;  // プレイヤー座標
        [ReadOnly] public NativeParallelMultiHashMap<int, int> CellToEntityMap;  // <セルID, セル内のプレイヤー>のMap

        public int GridWidth;  // グリッド横幅
        public float CellSize;  // 1つのセルのサイズ
        public float DeltaTime;

        public void Execute(int index)
        {
            var data = InputDatas[index];

            if (
                data.ID != EnemyID.Ghost ||
                data.State == (byte)EnemyState.Attack
                )
                return;

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
                            if (entityIndex < 0 || entityIndex >= PlayerPositions.Length)
                                continue;

                            float dist = Vector3.Distance(data.Position, PlayerPositions[entityIndex]);

                            if (dist < data.GolemSearchRadius && dist < minDistance)
                            {
                                minDistance = dist;
                                nearestIndex = entityIndex;
                            }

                        } while (CellToEntityMap.TryGetNextValue(out entityIndex, ref iterator));
                    }
                }
            }
            data.TargetIndex = nearestIndex;
            data.State = (byte)EnemyState.Search;
            InputDatas[index] = data;
        }
    }

    //  移動
    [BurstCompile]
    public struct MoveJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EnemyData> InputDatas; // 読み取り用
        public NativeArray<EnemyData> OutputDatas;          // 書き込み用
        public int ActiveEnmeyCount;
        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
        [ReadOnly] public NativeArray<float> TreasureTickness;  // お宝の厚み
        [ReadOnly] public NativeArray<Vector3> PlaeyrPositions;  // プレイヤー座標
        [ReadOnly] public NativeArray<float> PlayerTickness;  // プレイヤーの厚み
        [ReadOnly] public NativeArray<Vector3> WallPositions; // 壁の座標

        public float WallAvoidRadius;  // 壁を避け始める距離
        public float EnemyAvoidRadius;  // 敵同士で避け始める距離

        public float DeltaTime;
        public void Execute(int index)
        {
            var data = InputDatas[index];
            if (data.TargetIndex < 0) return;
            if (data.State == (byte)EnemyState.Attack) return;


            switch (data.ID)
            {
                case EnemyID.Golem:
                    HandleGolemMovement(
                        ref data,
                        index,
                        InputDatas,
                        OutputDatas,
                        ActiveEnmeyCount,
                        TreasurePositions,
                        TreasureTickness,
                        WallPositions,
                        WallAvoidRadius,
                        EnemyAvoidRadius,
                        DeltaTime
                        );
                    break;
                case EnemyID.Ghost:
                    HandleGhostMovement(
                        ref data,
                        index,
                        InputDatas,
                        OutputDatas,
                        ActiveEnmeyCount,
                        PlaeyrPositions,
                        PlayerTickness,
                        WallPositions,
                        WallAvoidRadius,
                        EnemyAvoidRadius,
                        DeltaTime
                        );
                    break;
            }

        }

        #region ゴーレム移動ロジック
        static void HandleGolemMovement(
        ref EnemyData data,
        int index,
        NativeArray<EnemyData> inputEnemyDatas,
        NativeArray<EnemyData> outputEnemyDatas,
        int ActiveEnemyCount,
        NativeArray<Vector3> treasurePositions,
        NativeArray<float> treasureTickness,
        NativeArray<Vector3> wallPositions,
        float wallAvoidRadius,
        float enemyAvoidRadius,
        float deltaTime
            )
        {

            Vector3 toTarget = treasurePositions[data.TargetIndex] - data.Position;
            float distSqToTarget = toTarget.sqrMagnitude;

            float targetRadius = treasureTickness[data.TargetIndex];
            float effectiveAttackRange = data.GolemAttackRange + data.GhostBodyTickness + targetRadius;

            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
            {
                data.State = (byte)EnemyState.Attack;
                outputEnemyDatas[index] = data;

                return;
            }

            Vector3 dir = toTarget / Mathf.Sqrt(distSqToTarget);
            dir.y = 0;
            Vector3 avoidance = Vector3.zero;

            for (int i = 0; i < ActiveEnemyCount; i++)
            {
                if (i == index) continue;

                float combinedRadius = data.GolemBodyTickness + inputEnemyDatas[i].GolemBodyTickness;
                float effectiveAvoidRadius = enemyAvoidRadius + combinedRadius;
                float sqrEffectiveAvoidRadius = effectiveAvoidRadius * effectiveAvoidRadius;

                Vector3 diff = data.Position - inputEnemyDatas[i].Position;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < sqrEffectiveAvoidRadius)
                {
                    avoidance += (data.Position - inputEnemyDatas[i].Position).normalized * (sqrEffectiveAvoidRadius - sqrDist);
                }
            }

            float wallAvoidRadSq = wallAvoidRadius * wallAvoidRadius;
            for (int i = 0; i < wallPositions.Length; i++)
            {
                Vector3 diff = data.Position - wallPositions[i];
                diff.y = 0;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < wallAvoidRadSq)
                {
                    float dist = Mathf.Sqrt(sqrDist);
                    avoidance += diff / dist * (wallAvoidRadSq - dist) * 2;
                }
            }

            avoidance.y = 0;

            data.Position += (dir + avoidance) * data.GolemMoveSpeed * deltaTime;
            data.State = (byte)EnemyState.Move;
            outputEnemyDatas[index] = data;
        }
        #endregion

        #region ゴースト移動ロジック
        static void HandleGhostMovement(
        ref EnemyData data,
        int index,
        NativeArray<EnemyData> inputEnemyDatas,
        NativeArray<EnemyData> outputEnemyDatas,
        int ActiveEnemyCount,
        NativeArray<Vector3> playerPositions,
        NativeArray<float> playerTickness,
        NativeArray<Vector3> wallPositions,
        float wallAvoidRadius,
        float enemyAvoidRadius,
        float deltaTime
            )
        {

            Vector3 toTarget = playerPositions[data.TargetIndex] - data.Position;
            float distSqToTarget = toTarget.sqrMagnitude;

            float targetRadius = playerTickness[data.TargetIndex];
            float effectiveAttackRange = data.GolemAttackRange + data.GhostBodyTickness + targetRadius;

            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
            {
                data.State = (byte)EnemyState.Attack;
                outputEnemyDatas[index] = data;

                return;
            }

            Vector3 dir = toTarget / Mathf.Sqrt(distSqToTarget);
            dir.y = 0;
            Vector3 avoidance = Vector3.zero;

            for (int i = 0; i < ActiveEnemyCount; i++)
            {
                if (i == index) continue;

                float combinedRadius = data.GolemBodyTickness + inputEnemyDatas[i].GolemBodyTickness;
                float effectiveAvoidRadius = enemyAvoidRadius + combinedRadius;
                float sqrEffectiveAvoidRadius = effectiveAvoidRadius * effectiveAvoidRadius;

                Vector3 diff = data.Position - inputEnemyDatas[i].Position;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < sqrEffectiveAvoidRadius)
                {
                    avoidance += (data.Position - inputEnemyDatas[i].Position).normalized * (sqrEffectiveAvoidRadius - sqrDist);
                }
            }

            float wallAvoidRadSq = wallAvoidRadius * wallAvoidRadius;
            for (int i = 0; i < wallPositions.Length; i++)
            {
                Vector3 diff = data.Position - wallPositions[i];
                diff.y = 0;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < wallAvoidRadSq)
                {
                    float dist = Mathf.Sqrt(sqrDist);
                    avoidance += diff / dist * (wallAvoidRadSq - dist) * 2;
                }
            }

            avoidance.y = 0;

            data.Position += (dir + avoidance) * data.GolemMoveSpeed * deltaTime;
            data.State = (byte)EnemyState.Move;
            outputEnemyDatas[index] = data;
        }
#endregion
    }
}


//    Vector3 toTarget = TreasurePositions[data.TargetIndex] - data.Position;
//    float distSqToTarget = toTarget.sqrMagnitude;

//    float targetRadius = TreasureTickness[data.TargetIndex];
//    float effectiveAttackRange = data.GolemAttackRange + data.GhostBodyTickness + targetRadius;

//    if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
//    {
//        data.State = (byte)EnemyState.Attack;
//        Datas[index] = data;

//        return;
//    }

//    Vector3 dir = toTarget / Mathf.Sqrt(distSqToTarget);
//    dir.y = 0;
//    Vector3 avoidance = Vector3.zero;

//    for (int i = 0; i < Datas.Length; i++)
//    {
//        if (i == index) continue;

//        float combinedRadius = data.GolemBodyTickness + Datas[i].GolemBodyTickness;
//        float effectiveAvoidRadius = EnemyAvoidRadius + combinedRadius;
//        float sqrEffectiveAvoidRadius = effectiveAvoidRadius * effectiveAvoidRadius;

//        Vector3 diff = data.Position - Datas[i].Position;
//        float sqrDist = diff.sqrMagnitude;

//        if (sqrDist < sqrEffectiveAvoidRadius)
//        {
//            avoidance += (data.Position - Datas[i].Position).normalized * (sqrEffectiveAvoidRadius - sqrDist);
//        }
//    }

//    float wallAvoidRadSq = WallAvoidRadius * WallAvoidRadius;
//    for (int i = 0; i < WallPositions.Length; i++)
//    {
//        Vector3 diff = data.Position - WallPositions[i];
//        diff.y = 0;
//        float sqrDist = diff.sqrMagnitude;

//        if (sqrDist < wallAvoidRadSq)
//        {
//            float dist = Mathf.Sqrt(sqrDist);
//            avoidance += diff / dist * (wallAvoidRadSq - dist) * 2;
//        }
//    }

//    avoidance.y = 0;

//    data.Position += (dir + avoidance) * data.GolemMoveSpeed * DeltaTime;
//    data.State = (byte)EnemyState.Move;
//    Datas[index] = data;