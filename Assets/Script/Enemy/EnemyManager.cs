using CareerQuest.Core;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace CareerQuest.Enemy
{
    //  敵の挙動を制御するクラス
    public sealed class EnemyManager : EnemyManagerBase<TreasureChest>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        //  --  FOR ALPHA  --  //
        float timer1 = 0f;
        [SerializeField] Transform testPos1;
        //  --  FOR ALPHA  --  //
        
        void Update()
        {

            //  --  FOR ALPHA  --  //
            timer1 += Time.deltaTime;
            if (timer1 >= 5f)
            {
                timer1 = 0f;
                SpawnEnemy(testPos1.position);
            }
            //  --  FOR ALPHA  --  //

            if (activeTreasureEntities.Count == 0) return;
            if (activeEnemyEntities.Count == 0) return;

            var readBuffer = isUsingBufferA ? bufferA : bufferB;
            var writeBuffer = isUsingBufferA ? bufferB : bufferA;

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                readBuffer[i] = new EnemyData
                {
                    Position = activeEnemyEntities[i].transform.position,
                    ID = activeEnemyEntities[i].EnemyID,

                    GolemAttackPower = golemAttackPower,
                    //GolemMoveSpeed = golemMoveSpeed, 今はNavmeshのスピードを使ってる
                    GolemAttackRange = golemAttackRange,
                    GolemSearchRadius = golemSearchRadius,
                    GolemTickness = golemBodyTickness,

                    //GhostMoveSpeed = ghostMoveSpeed, 今はNavmeshのスピードを使ってる
                    GhostAttackPower = ghostAttackPower,
                    GhostAttackRange = ghostAttackRange,
                    GhostSearchRadius = ghostSearchRadius,
                    GhostTickness = ghostBodyTickness,
                };
            }

            var searchTreasureJob = new SearchTreasureJob
            {
                InputDatas = readBuffer,
                TreasurePositions = treasureHashManager.Positions,
                CellToEntityMap = treasureHashManager.CellEnemyIndexMap,
                CellSize = treasureHashManager.cellSize,
                GridWidth = treasureHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };

            JobHandle searchTreasureHandle = searchTreasureJob.Schedule(activeEnemyEntities.Count, 64);

            searchTreasureHandle.Complete();

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                if (
                    readBuffer[i].TargetIndex <= -1 ||
                    readBuffer[i].TargetIndex >= treasureHashManager.ActiveEntities.Count
                    )
                    continue;

                activeEnemyEntities[i].Target= treasureHashManager.ActiveEntities[readBuffer[i].TargetIndex];
            }

            var searchPlayerJob = new SearchPlayerJob
            {
                InputDatas = readBuffer,
                PlayerPositions = playerHashManager.Positions,
                CellToEntityMap = playerHashManager.CellEnemyIndexMap,
                CellSize = playerHashManager.cellSize,
                GridWidth = playerHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };

            JobHandle searchPlayerHandle = searchPlayerJob.Schedule(activeEnemyEntities.Count, 64, searchTreasureHandle);

            JobHandle combinedSearchHandle = JobHandle.CombineDependencies(searchTreasureHandle, searchPlayerHandle);
            combinedSearchHandle.Complete();
            MyLogger.Log("周囲探索完了");

            var attackDicisionjob = new AttackDicisionob
            {
                InputDatas = readBuffer,
                OutputDatas = writeBuffer,
                TreasurePositions = treasureHashManager.Positions,
                TreasureTickness = treasureHashManager.Ticknesses,
                PlaeyrPositions = playerHashManager.Positions,
                PlayerTickness = playerHashManager.Ticknesses,
                EnemyAvoidRadius = golemEnemyAvoidRadius,
                DeltaTime = Time.deltaTime
            };

            JobHandle attackDicisionHandle = attackDicisionjob.Schedule(activeEnemyEntities.Count, 64, combinedSearchHandle);
            attackDicisionHandle.Complete();
            
            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                activeEnemyEntities[i].EnemyData = writeBuffer[i];

                int targetIndex = writeBuffer[i].TargetIndex;  // 可読性のためのにintに移してます。
                if (targetIndex >= 0)
                {
                    if (
                        activeEnemyEntities[i].EnemyID == EnemyID.Golem
                        && targetIndex < treasureHashManager.ActiveEntities.Count
                        && treasureHashManager.ActiveEntities[targetIndex] != null
                        )
                    {
                        var target = activeTreasureEntities[targetIndex];
                        activeEnemyEntities[i].Target = target;
                        activeEnemyEntities[i].SetTarget(target.transform.position);
                    }
                    else if (
                        activeEnemyEntities[i].EnemyID == EnemyID.Ghost
                        && targetIndex < playerHashManager.ActiveEntities.Count
                        )
                    {
                        var target = activePlayerEntities[targetIndex];
                        activeEnemyEntities[i].Target = activePlayerEntities[targetIndex];
                        activeEnemyEntities[i].SetTarget(playerHashManager.Positions[targetIndex]);
                    }
                }
            }

            if (bulletManager == null || bulletManager.ActiveCount == 0) return;

            var collisionJob = new CollisionJob
            {
                Bullets = bulletManager.BulletBuffer,
                BulletCount = bulletManager.ActiveCount,
                Enemies = writeBuffer
            };

            var collisionHandle = collisionJob.Schedule(activeEnemyEntities.Count, 64, attackDicisionHandle);
            collisionHandle.Complete();
            isUsingBufferA = !isUsingBufferA;

            for (int i = activeEnemyEntities.Count - 1; i >= 0; i--)
            {
                var enemyData = isUsingBufferA ? bufferA[i] : bufferB[i];

                if (enemyData.State == (byte)EnemyState.Dead)
                {
                    var enemyController = activeEnemyEntities[i];
                    DespawnEnemy(enemyController);
                }
            }

            if(isUsingBufferA)
            { bufferA = writeBuffer; }
            else{ bufferB = writeBuffer; }
            MyLogger.Log("敵行動サイクル通った");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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