using CareerQuest.Core;
using CareerQuest.Enemy;
using CareerQuest.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CareerQuest.Enemy
{
    //  サーチ結果返す用の構造体
    public struct SearchResult
    {
        public int Index;
        public EnemyTarget TargetType;

        public SearchResult(int index, EnemyTarget type)
        {
            Index = index;
            TargetType = type;
        }
    }

    //  周囲探索
    [BurstCompile]
    public struct SearchTargetJob : IJobParallelFor
    {
        public NativeArray<EnemyData> InputDatas; // 読み取り用
        [ReadOnly] public NativeArray<Vector3> TreasurePositions;  // お宝座標
        [ReadOnly] public NativeParallelMultiHashMap<int, int> TreasureCellMap;  // <セルID, セル内の宝数>のMap

        [ReadOnly] public NativeArray<Vector3> PlayerPositions;  // プレイヤー座標
        [ReadOnly] public NativeParallelMultiHashMap<int, int> PlayerCellMap;  // <セルID, セル内のプレイヤー>のMap

        public int GridWidth;  // グリッド横幅
        public float CellSize;  // 1つのセルのサイズ
        public float DeltaTime;

        public void Execute(int index)
        {
            var data = InputDatas[index];

            if (
                data.State == (byte)EnemyState.Attack
                )
                return;

            SearchResult searchResult = new(-1, EnemyTarget.None);

            float serachRadius = 0f;
            if (data.ID == EnemyID.Golem)
            { serachRadius = data.GolemSearchRadius; }
            if(data.ID == EnemyID.Ghost)
            { serachRadius = data.GhostSearchRadius; }
            if (data.ID == EnemyID.Swarm)
            { serachRadius = data.SwarmSearchRadius; }

            searchResult = SearchNearest(
                data.ID,
                data.Position,
                serachRadius,
                TreasurePositions,
                TreasureCellMap,
                PlayerPositions,
                PlayerCellMap,
                GridWidth,
                CellSize
            );

            data.TargetIndex = searchResult.Index;
            data.Target = searchResult.TargetType;
            data.State = (byte)EnemyState.Search;
            InputDatas[index] = data;

        }
        

        static SearchResult SearchNearest(
            EnemyID enemyId,
            Vector3 myPosition,
            float searchRadius,
            NativeArray<Vector3> treasurePositions,
            NativeParallelMultiHashMap<int, int> treasureCellMap,
            NativeArray<Vector3> playerPositions,
            NativeParallelMultiHashMap<int, int> playerCellMap,
            int gridWidth,
            float cellSize
        )
        {
            int myX = Mathf.FloorToInt(myPosition.x / cellSize);
            int myZ = Mathf.FloorToInt(myPosition.z / cellSize);
            int searchCell = (int)(searchRadius / cellSize);
            float searchRadiusSq = searchRadius * searchRadius;

            float minDistanceSq = float.MaxValue;
            int nearestIndex = -1;
            EnemyTarget targetType = EnemyTarget.None;

            for (int dz = -searchCell; dz <= searchCell; dz++)
            {
                for (int dx = -searchCell; dx <= searchCell; dx++)
                {
                    int targetCellId = (myX + dx) + ((myZ + dz) * gridWidth);

                    if (enemyId == EnemyID.Golem || enemyId == EnemyID.Swarm)
                    {
                        if (treasureCellMap.TryGetFirstValue(targetCellId, out int tIndex, out var tIter))
                        {
                            do
                            {
                                if (tIndex < 0 || tIndex >= treasurePositions.Length) continue;

                                float distSq = (treasurePositions[tIndex] - myPosition).sqrMagnitude;
                                if (distSq <= searchRadiusSq && distSq < minDistanceSq)
                                {
                                    minDistanceSq = distSq;
                                    nearestIndex = tIndex;
                                    targetType = EnemyTarget.Treasure;
                                }
                            } while (treasureCellMap.TryGetNextValue(out tIndex, ref tIter));
                        }
                    }

                    if (enemyId == EnemyID.Ghost || enemyId == EnemyID.Swarm)
                    {
                        if (playerCellMap.TryGetFirstValue(targetCellId, out int pIndex, out var pIter))
                        {
                            do
                            {
                                if (pIndex < 0 || pIndex >= playerPositions.Length) continue;

                                float distSq = (playerPositions[pIndex] - myPosition).sqrMagnitude;
                                if (distSq <= searchRadiusSq && distSq < minDistanceSq)
                                {
                                    minDistanceSq = distSq;
                                    nearestIndex = pIndex;
                                    targetType = EnemyTarget.Players;
                                }
                            } while (playerCellMap.TryGetNextValue(out pIndex, ref pIter));
                        }
                    }
                }
            }

            return new SearchResult(nearestIndex, targetType);
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