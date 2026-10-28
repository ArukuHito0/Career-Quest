using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    //  敵の挙動を制御するクラス
    public sealed class EnemyManager : EnemyManagerBase
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }
        
        void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < runtimeSpawnPoints.Count; i++)
            {
                var pointData = runtimeSpawnPoints[i];
                pointData.CurrentTimer += deltaTime;

                if (pointData.CurrentTimer >= pointData.SpawnInterval)
                {
                    pointData.CurrentTimer = 0f;

                    if (activeEnemyEntities.Count < maxEnemyCount)
                    {
                        EnemyID selectedID = LotteryEnemy(pointData.Candidates);
                        SpawnEnemy(pointData.Position, selectedID);
                    }
                }
                runtimeSpawnPoints[i] = pointData;
            }
            if(CheckActiveCount())
            {
                return;
            }

            var readBuffer = isUsingBufferA ? bufferA : bufferB;
            var writeBuffer = isUsingBufferA ? bufferB : bufferA;

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                var data = readBuffer[i];
                data.Position = activeEnemyEntities[i].transform.position;
                readBuffer[i] = data;
            };

            var searchTargetJob = new SearchTargetJob
            {
                InputDatas = readBuffer,
                TreasurePositions = treasureHashManager.Positions,
                TreasureCellMap = treasureHashManager.CellEnemyIndexMap,
                PlayerPositions = playerHashManager.Positions,
                PlayerCellMap = playerHashManager.CellEnemyIndexMap,
                CellSize = treasureHashManager.cellSize,
                GridWidth = treasureHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };

            JobHandle searchTargetHandle = searchTargetJob.Schedule(activeEnemyEntities.Count, 64);
            searchTargetHandle.Complete();
            MyLogger.Log("周囲探索完了");

            //  攻撃判断ロジック
            var attackDicisionjob = new AttackDecisionJob
            {
                InputDatas = readBuffer,
                OutputDatas = writeBuffer,
                TreasurePositions = treasureHashManager.Positions,
                TreasureTickness = treasureHashManager.Ticknesses,
                PlaeyrPositions = playerHashManager.Positions,
                PlayerTickness = playerHashManager.Ticknesses,
                DeltaTime = Time.deltaTime
            };
            JobHandle attackDicisionHandle = attackDicisionjob.Schedule(activeEnemyEntities.Count, 64, searchTargetHandle);
            attackDicisionHandle.Complete();
            
            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                activeEnemyEntities[i].EnemyData = writeBuffer[i];

                int targetIndex = writeBuffer[i].TargetIndex;
                if (targetIndex >= 0)
                {
                    if (
                        activeEnemyEntities[i].EnemyData.Target == EnemyTarget.Treasure
                        && targetIndex < treasureHashManager.ActiveEntities.Count
                        && treasureHashManager.ActiveEntities[targetIndex] != null
                        )
                    {
                        var target = activeTreasureEntities[targetIndex];
                        activeEnemyEntities[i].Target = target;
                        activeEnemyEntities[i].SetTarget(target.transform.position);
                    }
                    else if (
                        activeEnemyEntities[i].EnemyData.Target == EnemyTarget.Player
                        && targetIndex < playerHashManager.ActiveEntities.Count
                        )
                    {
                        var target = activePlayerEntities[targetIndex];
                        activeEnemyEntities[i].Target = activePlayerEntities[targetIndex];
                        activeEnemyEntities[i].SetTarget(playerHashManager.Positions[targetIndex]);
                    }
                    MyLogger.Log("ターゲット設定完了");
                }
            }

            for (int i = 0; i < pendingDamages.Count; i++)
            {
                var dmg = pendingDamages[i];
                if (dmg.Index >= 0 && dmg.Index < writeBuffer.Length)
                {
                    var enemy = writeBuffer[dmg.Index];
                    if (enemy.State != (byte)EnemyState.Invincible)
                    {
                        if ((enemy.CurrentHp -= dmg.Damage) <= 0)
                        {
                            enemy.State = (byte)EnemyState.Dead;
                        }
                        writeBuffer[dmg.Index] = enemy;
                    }
                }
            }
            pendingDamages.Clear();


            for (int i = 0; i < pendingStates.Count; i++)
            {
                var state = pendingStates[i];
                if (state.Index >= 0 && state.Index < writeBuffer.Length)
                {
                    var enemy = writeBuffer[state.Index];
                    enemy.State = (byte)state.State;
                    writeBuffer[state.Index] = enemy;
                }
            }
            pendingStates.Clear();

            for (int i = activeEnemyEntities.Count - 1; i >= 0; i--)
            {
                var enemyData = writeBuffer[i];

                if(enemyData.State == (byte)EnemyState.Attack)
                {
                    activeEnemyEntities[i].Attack();
                }
            }
            for (int i = activeEnemyEntities.Count - 1; i >= 0; i--)
            {
                if (i >= writeBuffer.Length) continue;

                var enemyData = writeBuffer[i];
                if (
                    enemyData.State == (byte)EnemyState.Dead
                    && enemyData.State != (byte)EnemyState.Invincible
                    )
                {
                    DespawnEnemy(activeEnemyEntities[i]);
                }
            }

            isUsingBufferA = !isUsingBufferA;
            MyLogger.Log("敵行動サイクル通った");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        //  マップの要素のアクティブ数を検査数r
        bool CheckActiveCount()
        {
            if (activeTreasureEntities.Count == 0)
            { 
                MyLogger.Log($"Enemy:アクティブなお宝の数{activeTreasureEntities.Count}");
                return true;
            }
            else if (activePlayerEntities.Count == 0)
            {
                MyLogger.Log($"Enemy:アクティブなプレイヤーの数{activePlayerEntities.Count}");
                return true;
            }
            else if (activeEnemyEntities.Count == 0)
            {
                MyLogger.Log($"Enemy:アクティブな敵の数{activeEnemyEntities.Count}");
                return false;
            }
            return false;
        }
    }
}


/* navmeshで代替してみるのでコメントアウト */
//var moveJob = new MoveJob
//{
//    InputDatas = readBuffer,
//    OutputDatas = writeBuffer,
//    TreasurePositions = treasureHashManager.Positions,
//    TreasureTickness = treasureHashManager.Ticknesses,
//    PlaeyrPositions = playerHashManager.Positions,
//    PlayerTickness = playerHashManager.Ticknesses,
//    WallPositions = wallPositions,
//    WallAvoidRadius = golemWallAvoidRadius,
//    EnemyAvoidRadius = golemEnemyAvoidRadius,
//    DeltaTime = Time.deltaTime
//};

/*  配列の中身が動的に変化して安定しなかったのでCO */
//if (bulletManager != null && bulletManager.ActiveCount > 0)
//{
//    int maxBullet = bulletManager.ActiveCount * 3 + activeEnemyEntities.Count;  //  弾が処理中に増える可能性があるので余裕を持たせた
//    var hitData = new NativeList<BulletHitData>(maxBullet, Allocator.TempJob);
//    hitData.Capacity = maxBullet; // 容量を固定
//    var collisionJob = new CollisionJob
//    {
//        Enemies = writeBuffer,
//        Bullets = bulletManager.BulletBuffer,
//        HitData = hitData.AsParallelWriter()
//    };

//    var collisionHandle = collisionJob.Schedule(activeEnemyEntities.Count, 64, attackDicisionHandle);
//    collisionHandle.Complete();

//    for (int i = 0; i < hitData.Length; i++)
//    {
//        var hd = hitData[i];
//        if (hd.EnemyIndex < 0 || hd.EnemyIndex >= writeBuffer.Length) continue;

//        var enemy = writeBuffer[hd.EnemyIndex];
//        if (
//            enemy.CurrentHp <= 0
//            || enemy.State == (byte)EnemyState.Invincible
//            )
//            continue;

//        if (hd.BulletIndex >= 0 && hd.BulletIndex < BulletManager.ActiveBullets.Count)
//        {
//            var targetBullet = BulletManager.ActiveBullets[hd.BulletIndex];
//            if (targetBullet != null && targetBullet.IsActive)
//            {
//                targetBullet.IsActive = false;
//                targetBullet.Destroy();
//            }
//        }

//        enemy.CurrentHp -= hd.Damage;
//        writeBuffer[hd.EnemyIndex] = enemy;
//    }
//    hitData.Dispose();
//}