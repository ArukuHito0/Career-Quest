using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using CareerQuest.Enemy;
using CareerQuest.Player;

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

            if (
                data.ID != EnemyID.Golem ||
                data.State == (byte)EnemyState.Attack
                )
                return;

            float minDistance = float.MaxValue;
            int nearestIndex = -1;

            int myX = Mathf.FloorToInt(data.Position.x / CellSize);
            int myZ = Mathf.FloorToInt(data.Position.z / CellSize);
            int myCellId = myX + (myZ * GridWidth);
            int sreachCell = (int)(data.GolemSearchRadius / CellSize);
            //int sreachCell = Mathf.CeilToInt(data.GhostSearchRadius / CellSize);  //  サーチ漏れがなくなるが処理負荷も上がるので実装するときは最適化と一緒にする。
            for (int dz = -sreachCell; dz <= sreachCell; dz++)
            {
                for (int dx = -sreachCell; dx <= sreachCell; dx++)
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

            int sreachCell = (int)(data.GhostSearchRadius / CellSize);
            //int sreachCell = Mathf.CeilToInt(data.GhostSearchRadius / CellSize);  //  サーチ漏れがなくなるが処理負荷も上がるので実装するときは最適化と一緒にする。
            for (int dz = -sreachCell; dz <= sreachCell; dz++)
            {
                for (int dx = -sreachCell; dx <= sreachCell; dx++)
                {
                    int targetCellId = (myX + dx) + ((myZ + dz) * GridWidth);

                    if (CellToEntityMap.TryGetFirstValue(targetCellId, out int entityIndex, out var iterator))
                    {
                        do
                        {
                            if (entityIndex < 0 || entityIndex >= PlayerPositions.Length)
                                continue;

                            float dist = Vector3.Distance(data.Position, PlayerPositions[entityIndex]);
                            if (dist < data.GhostSearchRadius && dist < minDistance)
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


    //  攻撃するか判断
    [BurstCompile]
    public struct AttackDecisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<EnemyData> InputDatas; // 読み取り用
        public NativeArray<EnemyData> OutputDatas;          // 書き込み用
        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
        [ReadOnly] public NativeArray<float> TreasureTickness;  // お宝の厚み
        [ReadOnly] public NativeArray<Vector3> PlaeyrPositions;  // プレイヤー座標
        [ReadOnly] public NativeArray<float> PlayerTickness;  // プレイヤーの厚み

        public float EnemyAvoidRadius;  // 敵同士で避け始める距離

        public float DeltaTime;
        public void Execute(int index)
        {
            var data = InputDatas[index];
            if (data.TargetIndex < 0)
            {
                OutputDatas[index] = data;
                return;
            }

            if ((data.CurrentAttackCoolDown -= DeltaTime) > 0)
            {
                data.State = (byte)EnemyState.Move;
                OutputDatas[index] = data;
                return;
            }

            switch (data.ID)
            {
                case EnemyID.Golem:
                    GolemDecision(
                        ref data,
                        index,
                        TreasurePositions,
                        TreasureTickness
                   );
                    break;
                case EnemyID.Ghost:
                    GhostDecision(
                        ref data,
                        index,
                        PlaeyrPositions,
                        PlayerTickness
                   );
                    break;
            }
            OutputDatas[index] = data;
        }

        #region ゴーレム攻撃判断
        static void GolemDecision(
        ref EnemyData data,
        int index,
        NativeArray<Vector3> treasurePositions,
        NativeArray<float> treasureTickness
            )
        {
            Vector3 toTarget = treasurePositions[data.TargetIndex] - data.Position;
            float distSqToTarget = toTarget.sqrMagnitude;

            float targetRadius = treasureTickness[data.TargetIndex];
            float effectiveAttackRange = data.GolemAttackRange + data.GhostTickness + targetRadius;

            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
            {
                data.State = (byte)EnemyState.Attack;
                data.CurrentAttackCoolDown = data.GolemAttackCoolDown;
            }
            else 
            {
                data.State = (byte)EnemyState.Move;
            }
        }
        #endregion

        #region ゴースト攻撃判断
        static void GhostDecision(
        ref EnemyData data,
        int index,
        NativeArray<Vector3> playerPositions,
        NativeArray<float> playerTickness
            )
        {

            Vector3 toTarget = playerPositions[data.TargetIndex] - data.Position;
            float distSqToTarget = toTarget.sqrMagnitude;

            float targetRadius = playerTickness[data.TargetIndex];
            float effectiveAttackRange = data.GolemAttackRange + data.GhostTickness + targetRadius;

            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
            {
                data.State = (byte)EnemyState.Attack;
                data.CurrentAttackCoolDown = data.GhostAttackCoolDown;
            }
            else
            {
                data.State = (byte)EnemyState.Move;
            }
        }
    }
}
#endregion

//  当たり判定判断
[BurstCompile]
public struct CollisionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<EnemyData> Enemies;
    [ReadOnly] public NativeArray<BulletData> Bullets;

    public NativeList<BulletHitData>.ParallelWriter HitData;
    public void Execute(int index)
    {
        var enemy = Enemies[index];
        if (enemy.CurrentHp <= 0) return;

        for (int p = 0; p < Bullets.Length; p++)
        {
            var proj = Bullets[p];
            if (!proj.IsActive) continue;

            float sqrDist = (enemy.Position - proj.Position).sqrMagnitude;
            float tickness = enemy.ID switch
            {
                EnemyID.Golem => enemy.GolemTickness,
                EnemyID.Ghost => enemy.GhostTickness,
                _ => 0f
            };
            float hitRadius = proj.Radius + tickness;

            if (sqrDist <= hitRadius * hitRadius)
            {
                HitData.AddNoResize(new BulletHitData 
                {
                    BulletIndex = p,
                    EnemyIndex = index,
                    Damage = proj.Damage,
                });

                break;
            }
        }
    }
}
//  移動はNavMeshを試用してみるのでコメントアウト
//    //  移動
//    [BurstCompile]
//    public struct MoveJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<EnemyData> InputDatas; // 読み取り用
//        public NativeArray<EnemyData> OutputDatas;          // 書き込み用
//        public int ActiveEnmeyCount;
//        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
//        [ReadOnly] public NativeArray<float> TreasureTickness;  // お宝の厚み
//        [ReadOnly] public NativeArray<Vector3> PlaeyrPositions;  // プレイヤー座標
//        [ReadOnly] public NativeArray<float> PlayerTickness;  // プレイヤーの厚み
//        [ReadOnly] public NativeArray<Vector3> WallPositions; // 壁の座標

//        public float WallAvoidRadius;  // 壁を避け始める距離
//        public float EnemyAvoidRadius;  // 敵同士で避け始める距離

//        public float DeltaTime;
//        public void Execute(int index)
//        {
//            var data = InputDatas[index];
//            if (data.TargetIndex < 0) return;
//            if (data.State == (byte)EnemyState.Attack) return;


//            switch (data.ID)
//            {
//                case EnemyID.Golem:
//                    HandleGolemMovement(
//                        ref data,
//                        index,
//                        InputDatas,
//                        OutputDatas,
//                        ActiveEnmeyCount,
//                        TreasurePositions,
//                        TreasureTickness,
//                        WallPositions,
//                        WallAvoidRadius,
//                        EnemyAvoidRadius,
//                        DeltaTime
//                        );
//                    break;
//                case EnemyID.Ghost:
//                    HandleGhostMovement(
//                        ref data,
//                        index,
//                        InputDatas,
//                        OutputDatas,
//                        ActiveEnmeyCount,
//                        PlaeyrPositions,
//                        PlayerTickness,
//                        WallPositions,
//                        WallAvoidRadius,
//                        EnemyAvoidRadius,
//                        DeltaTime
//                        );
//                    break;
//            }

//        }

//        #region ゴーレム移動ロジック
//        static void HandleGolemMovement(
//        ref EnemyData data,
//        int index,
//        NativeArray<EnemyData> inputEnemyDatas,
//        NativeArray<EnemyData> outputEnemyDatas,
//        int ActiveEnemyCount,
//        NativeArray<Vector3> treasurePositions,
//        NativeArray<float> treasureTickness,
//        NativeArray<Vector3> wallPositions,
//        float wallAvoidRadius,
//        float enemyAvoidRadius,
//        float deltaTime
//            )
//        {

//            Vector3 toTarget = treasurePositions[data.TargetIndex] - data.Position;
//            float distSqToTarget = toTarget.sqrMagnitude;

//            float targetRadius = treasureTickness[data.TargetIndex];
//            float effectiveAttackRange = data.GolemAttackRange + data.GhostTickness + targetRadius;

//            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
//            {
//                data.State = (byte)EnemyState.Attack;
//                outputEnemyDatas[index] = data;

//                return;
//            }

//            Vector3 dir = toTarget / Mathf.Sqrt(distSqToTarget);
//            dir.y = 0;
//            Vector3 avoidance = Vector3.zero;

//            for (int i = 0; i < ActiveEnemyCount; i++)
//            {
//                if (i == index) continue;

//                float combinedRadius = data.GolemTickness + inputEnemyDatas[i].GolemTickness;
//                float effectiveAvoidRadius = enemyAvoidRadius + combinedRadius;
//                float sqrEffectiveAvoidRadius = effectiveAvoidRadius * effectiveAvoidRadius;

//                Vector3 diff = data.Position - inputEnemyDatas[i].Position;
//                float sqrDist = diff.sqrMagnitude;

//                if (sqrDist < sqrEffectiveAvoidRadius)
//                {
//                    avoidance += (data.Position - inputEnemyDatas[i].Position).normalized * (sqrEffectiveAvoidRadius - sqrDist);
//                }
//            }

//            float wallAvoidRadSq = wallAvoidRadius * wallAvoidRadius;
//            for (int i = 0; i < wallPositions.Length; i++)
//            {
//                Vector3 diff = data.Position - wallPositions[i];
//                diff.y = 0;
//                float sqrDist = diff.sqrMagnitude;

//                if (sqrDist < wallAvoidRadSq)
//                {
//                    float dist = Mathf.Sqrt(sqrDist);
//                    avoidance += diff / dist * (wallAvoidRadSq - dist) * 2;
//                }
//            }

//            avoidance.y = 0;

//            data.Position += (dir + avoidance) * data.GolemMoveSpeed * deltaTime;
//            data.State = (byte)EnemyState.Move;
//            outputEnemyDatas[index] = data;
//        }
//        #endregion

//        #region ゴースト移動ロジック
//        static void HandleGhostMovement(
//        ref EnemyData data,
//        int index,
//        NativeArray<EnemyData> inputEnemyDatas,
//        NativeArray<EnemyData> outputEnemyDatas,
//        int ActiveEnemyCount,
//        NativeArray<Vector3> playerPositions,
//        NativeArray<float> playerTickness,
//        NativeArray<Vector3> wallPositions,
//        float wallAvoidRadius,
//        float enemyAvoidRadius,
//        float deltaTime
//            )
//        {

//            Vector3 toTarget = playerPositions[data.TargetIndex] - data.Position;
//            float distSqToTarget = toTarget.sqrMagnitude;

//            float targetRadius = playerTickness[data.TargetIndex];
//            float effectiveAttackRange = data.GolemAttackRange + data.GhostTickness + targetRadius;

//            if (distSqToTarget < effectiveAttackRange * effectiveAttackRange)
//            {
//                data.State = (byte)EnemyState.Attack;
//                outputEnemyDatas[index] = data;

//                return;
//            }

//            Vector3 dir = toTarget / Mathf.Sqrt(distSqToTarget);
//            dir.y = 0;
//            Vector3 avoidance = Vector3.zero;

//            for (int i = 0; i < ActiveEnemyCount; i++)
//            {
//                if (i == index) continue;

//                float combinedRadius = data.GolemTickness + inputEnemyDatas[i].GolemTickness;
//                float effectiveAvoidRadius = enemyAvoidRadius + combinedRadius;
//                float sqrEffectiveAvoidRadius = effectiveAvoidRadius * effectiveAvoidRadius;

//                Vector3 diff = data.Position - inputEnemyDatas[i].Position;
//                float sqrDist = diff.sqrMagnitude;

//                if (sqrDist < sqrEffectiveAvoidRadius)
//                {
//                    avoidance += (data.Position - inputEnemyDatas[i].Position).normalized * (sqrEffectiveAvoidRadius - sqrDist);
//                }
//            }

//            float wallAvoidRadSq = wallAvoidRadius * wallAvoidRadius;
//            for (int i = 0; i < wallPositions.Length; i++)
//            {
//                Vector3 diff = data.Position - wallPositions[i];
//                diff.y = 0;
//                float sqrDist = diff.sqrMagnitude;

//                if (sqrDist < wallAvoidRadSq)
//                {
//                    float dist = Mathf.Sqrt(sqrDist);
//                    avoidance += diff / dist * (wallAvoidRadSq - dist) * 2;
//                }
//            }

//            avoidance.y = 0;

//            data.Position += (dir + avoidance) * data.GolemMoveSpeed * deltaTime;
//            data.State = (byte)EnemyState.Move;
//            outputEnemyDatas[index] = data;
//        }
//        #endregion

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
//    探索処理の代替案
//for (int i = 0; i < PlayerPositions.Length; i++)
//{
//    float dist = Vector3.Distance(data.Position, PlayerPositions[i]);
//    if (dist < data.GhostSearchRadius && dist < minDistance)
//    {
//        minDistance = dist;
//        nearestIndex = i;
//    }
//}